using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Stock_Warehouse_Tracking_Project_API.Configuration;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap.Http;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

public sealed class HttpSapClient : ISapClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly SapHttpOptions _options;
    private readonly ILogger<HttpSapClient> _logger;

    public HttpSapClient(
        HttpClient httpClient,
        IOptions<SapHttpOptions> options,
        ILogger<HttpSapClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SapStockRow>> GetStockListAsync(
        string? matnr = null,
        string? whId = null,
        CancellationToken ct = default)
    {
        var path = BuildPathWithQuery(_options.StockListPath, matnr, whId);
        var rows = await GetJsonAsync<List<SapStockJsonDto>>(path, ct);
        return (rows ?? []).Select(MapStockRow).ToList();
    }

    public async Task<SapStockRow?> GetStockDetailAsync(
        string matnr,
        string whId,
        CancellationToken ct = default)
    {
        var path = ReplacePathTokens(_options.StockDetailPath, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["matnr"] = matnr.Trim(),
            ["whId"] = whId.Trim()
        });

        try
        {
            var row = await GetJsonAsync<SapStockJsonDto>(path, ct);
            return row is null ? null : MapStockRow(row);
        }
        catch (SapHttpException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<SapCreateProductResult> CreateProductAsync(
        SapCreateProductRequest request,
        CancellationToken ct = default)
    {
        var body = new SapCreateProductJsonRequest
        {
            Matnr = request.Matnr.Trim(),
            Name = request.Name.Trim(),
            Unit = request.Unit.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim()
        };

        var result = await PostJsonAsync(_options.ProductsPath, body, ct);
        return MapCreateProductResult(result);
    }

    public Task<SapMovementResult> StockInAsync(SapStockInRequest request, CancellationToken ct = default)
        => PostMovementAsync(_options.StockInPath, request.Matnr, request.WhId, request.Quantity, request.RefNo, ct);

    public Task<SapMovementResult> StockOutAsync(SapStockOutRequest request, CancellationToken ct = default)
        => PostMovementAsync(_options.StockOutPath, request.Matnr, request.WhId, request.Quantity, request.RefNo, ct);

    public async Task<SapMovementResult> TransferStockAsync(SapTransferRequest request, CancellationToken ct = default)
    {
        var body = new SapTransferJsonRequest
        {
            Matnr = request.Matnr.Trim(),
            SourceWhId = request.SourceWhId.Trim(),
            DestWhId = request.DestWhId.Trim(),
            Quantity = request.Quantity,
            RefNo = request.RefNo
        };

        var result = await PostJsonAsync(_options.TransferPath, body, ct);
        return MapMovementResult(result);
    }

    private async Task<SapMovementResult> PostMovementAsync(
        string path,
        string matnr,
        string whId,
        decimal quantity,
        string? refNo,
        CancellationToken ct)
    {
        var body = new SapStockMovementJsonRequest
        {
            Matnr = matnr.Trim(),
            WhId = whId.Trim(),
            Quantity = quantity,
            RefNo = refNo
        };

        var result = await PostJsonAsync(path, body, ct);
        return MapMovementResult(result);
    }

    private async Task<T?> GetJsonAsync<T>(string relativePath, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(relativePath, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("SAP HTTP GET {Path} failed: {Status} {Body}", relativePath, (int)response.StatusCode, body);
            throw new SapHttpException(
                $"SAP HTTP GET failed ({(int)response.StatusCode}): {Truncate(body)}")
            {
                StatusCode = (int)response.StatusCode,
                ResponseBody = body
            };
        }

        if (string.IsNullOrWhiteSpace(body))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new SapHttpException("SAP HTTP response JSON parse failed.", ex) { ResponseBody = body };
        }
    }

    private async Task<SapMovementJsonResponse?> PostJsonAsync<TRequest>(string relativePath, TRequest body, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(body, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(relativePath, content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("SAP HTTP POST {Path} failed: {Status} {Body}", relativePath, (int)response.StatusCode, responseBody);
            throw new SapHttpException(
                $"SAP HTTP POST failed ({(int)response.StatusCode}): {Truncate(responseBody)}")
            {
                StatusCode = (int)response.StatusCode,
                ResponseBody = responseBody
            };
        }

        if (string.IsNullOrWhiteSpace(responseBody))
            return new SapMovementJsonResponse { Success = true };

        try
        {
            return JsonSerializer.Deserialize<SapMovementJsonResponse>(responseBody, JsonOptions)
                   ?? new SapMovementJsonResponse { Success = true };
        }
        catch (JsonException ex)
        {
            throw new SapHttpException("SAP HTTP response JSON parse failed.", ex) { ResponseBody = responseBody };
        }
    }

    private static string BuildPathWithQuery(string path, string? matnr, string? whId)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(matnr))
            query.Add($"matnr={Uri.EscapeDataString(matnr.Trim())}");
        if (!string.IsNullOrWhiteSpace(whId))
            query.Add($"whId={Uri.EscapeDataString(whId.Trim())}");

        return query.Count == 0 ? path : $"{path}?{string.Join('&', query)}";
    }

    private static string ReplacePathTokens(string template, IReadOnlyDictionary<string, string> tokens)
    {
        var path = template;
        foreach (var (key, value) in tokens)
            path = path.Replace($"{{{key}}}", Uri.EscapeDataString(value), StringComparison.OrdinalIgnoreCase);
        return path;
    }

    private static SapStockRow MapStockRow(SapStockJsonDto dto) => new()
    {
        Matnr = dto.Matnr?.Trim() ?? string.Empty,
        WhId = dto.WhId?.Trim() ?? string.Empty,
        Quantity = dto.Quantity,
        UpdatedAt = ParseUpdatedAt(dto.UpdatedAt)
    };

    private static DateTime ParseUpdatedAt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.UtcNow;

        if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var dt))
            return AbapTypeConverters.ToUtcDate(dt);

        return DateTime.UtcNow;
    }

    private static SapMovementResult MapMovementResult(SapMovementJsonResponse? result) => new()
    {
        Success = result?.Success ?? false,
        SapDocNo = string.IsNullOrWhiteSpace(result?.SapDocNo) ? null : result.SapDocNo.Trim(),
        ErrorMessage = string.IsNullOrWhiteSpace(result?.ErrorMessage) ? null : result.ErrorMessage.Trim()
    };

    private static SapCreateProductResult MapCreateProductResult(SapMovementJsonResponse? result) => new()
    {
        Success = result?.Success ?? false,
        SapDocNo = string.IsNullOrWhiteSpace(result?.SapDocNo) ? null : result.SapDocNo.Trim(),
        ErrorMessage = string.IsNullOrWhiteSpace(result?.ErrorMessage) ? null : result.ErrorMessage.Trim()
    };

    private static string Truncate(string? value, int max = 500)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= max ? value : value[..max] + "...";
    }
}
