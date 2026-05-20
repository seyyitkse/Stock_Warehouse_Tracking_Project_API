using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Globalization;
using Microsoft.Extensions.Options;
using Stock_Warehouse_Tracking_Project_API.Configuration;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap.Http;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

public sealed class HttpSapClient : ISapClient, IProductCatalogSapClient
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

    public async Task<IReadOnlyList<SapProductRow>> GetProductListAsync(CancellationToken ct = default)
    {
        var products = await GetProductJsonListAsync(_options.ProductsPath, ct);
        return products
            .Select(MapProductRow)
            .Where(product => !string.IsNullOrWhiteSpace(product.Matnr))
            .ToList();
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

    private async Task<List<SapProductJsonDto>> GetProductJsonListAsync(string relativePath, CancellationToken ct)
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
            return [];

        try
        {
            using var document = JsonDocument.Parse(body);
            var productArray = ResolveProductArray(document.RootElement);
            if (productArray.ValueKind != JsonValueKind.Array)
            {
                throw new SapHttpException(
                    "SAP HTTP products response must be a JSON array or contain a products/data/items array.")
                {
                    ResponseBody = body
                };
            }

            var products = new List<SapProductJsonDto>();
            foreach (var item in productArray.EnumerateArray())
            {
                var product = item.Deserialize<SapProductJsonDto>(JsonOptions);
                if (product is not null)
                    products.Add(product);
            }

            return products;
        }
        catch (JsonException ex)
        {
            throw new SapHttpException("SAP HTTP products response JSON parse failed.", ex) { ResponseBody = body };
        }
    }

    private static JsonElement ResolveProductArray(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
            return root;

        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in new[] { "products", "data", "items", "materials" })
            {
                if (TryGetProperty(root, propertyName, out var value) && value.ValueKind == JsonValueKind.Array)
                    return value;
            }
        }

        return default;
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

    private static SapProductRow MapProductRow(SapProductJsonDto dto) => new()
    {
        Matnr = FirstNonEmpty(dto.Matnr, dto.Code, GetExtraString(dto, "MATNR", "CODE")) ?? string.Empty,
        Name = FirstNonEmpty(dto.Name, dto.Matname, GetExtraString(dto, "MATNAME", "MAT_NAME", "NAME")) ?? string.Empty,
        Unit = FirstNonEmpty(dto.Unit, GetExtraString(dto, "UNIT")) ?? string.Empty,
        Category = FirstNonEmpty(dto.Category, GetExtraString(dto, "CATEGORY")),
        Barcode = FirstNonEmpty(dto.Barcode, GetExtraString(dto, "BARCODE")),
        CreatedAt = ParseSapDate(FirstNonEmpty(dto.CreatedAt, GetExtraString(dto, "CREATED_AT", "CREATEDAT")))
    };

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

    private static DateTime ParseSapDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.UtcNow;

        var trimmed = value.Trim();
        if (DateTime.TryParseExact(
                trimmed,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var sapDate))
        {
            return DateTime.SpecifyKind(sapDate, DateTimeKind.Utc);
        }

        if (DateTime.TryParse(
                trimmed,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var date))
        {
            return AbapTypeConverters.ToUtcDate(date);
        }

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

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    private static string? GetExtraString(SapProductJsonDto dto, params string[] names)
    {
        if (dto.Extra is null)
            return null;

        foreach (var (key, value) in dto.Extra)
        {
            foreach (var name in names)
            {
                if (!SapFieldEquals(key, name))
                    continue;

                return value.ValueKind switch
                {
                    JsonValueKind.Null or JsonValueKind.Undefined => null,
                    JsonValueKind.String => value.GetString(),
                    _ => value.ToString()
                };
            }
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (SapFieldEquals(property.Name, name))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool SapFieldEquals(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase)
               || string.Equals(NormalizeSapField(left), NormalizeSapField(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeSapField(string value)
        => value.Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

    private static string Truncate(string? value, int max = 500)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= max ? value : value[..max] + "...";
    }
}
