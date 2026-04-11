using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Stock_Warehouse_Tracking_Project_API.Configuration;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Services
{
    public class HttpSapStockClient : ISapStockClient
    {
        private static readonly Regex SapDateRegex = new(@"/Date\((?<ms>-?\d+)", RegexOptions.Compiled);

        private readonly HttpClient _httpClient;
        private readonly SapHttpOptions _options;
        private readonly ILogger<HttpSapStockClient> _logger;

        public HttpSapStockClient(HttpClient httpClient, IOptions<SapHttpOptions> options, ILogger<HttpSapStockClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password))
            {
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
            }
        }

        public async Task<IReadOnlyList<SapStockRow>> GetStockListAsync(CancellationToken cancellationToken = default)
        {
            var requestPath = BuildRequestPath();
            using var request = new HttpRequestMessage(HttpMethod.Get, requestPath);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("SAP stock request failed. StatusCode: {StatusCode}, Body: {Body}", (int)response.StatusCode, payload);
                throw new HttpRequestException($"SAP stock request failed with status code {(int)response.StatusCode}.");
            }

            return ParseStocks(payload);
        }

        private string BuildRequestPath()
        {
            var path = _options.StocksPath;

            if (string.IsNullOrWhiteSpace(_options.Client) || path.Contains("sap-client=", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            var separator = path.Contains('?') ? '&' : '?';
            return $"{path}{separator}sap-client={Uri.EscapeDataString(_options.Client)}";
        }

        private static IReadOnlyList<SapStockRow> ParseStocks(string payload)
        {
            using var document = JsonDocument.Parse(payload);

            if (TryGetODataV2Results(document.RootElement, out var rowsElement) || TryGetODataV4Value(document.RootElement, out rowsElement))
            {
                return rowsElement.EnumerateArray().Select(MapRow).ToList();
            }

            throw new InvalidOperationException("SAP response format is not recognized.");
        }

        private static bool TryGetODataV2Results(JsonElement root, out JsonElement rows)
        {
            rows = default;

            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("d", out var d) || d.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!d.TryGetProperty("results", out rows) || rows.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            return true;
        }

        private static bool TryGetODataV4Value(JsonElement root, out JsonElement rows)
        {
            rows = default;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!root.TryGetProperty("value", out rows) || rows.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            return true;
        }

        private static SapStockRow MapRow(JsonElement row)
        {
            return new SapStockRow
            {
                Matnr = GetString(row, "MATNR"),
                WhId = GetString(row, "WH_ID"),
                Quantity = GetDecimal(row, "QUANTITY"),
                UpdatedAt = GetDateTime(row, "UPDATE_AT")
            };
        }

        private static string GetString(JsonElement row, string propertyName)
        {
            if (!row.TryGetProperty(propertyName, out var value))
            {
                return string.Empty;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                _ => string.Empty
            };
        }

        private static decimal GetDecimal(JsonElement row, string propertyName)
        {
            if (!row.TryGetProperty(propertyName, out var value))
            {
                return 0;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var decimalValue))
            {
                return decimalValue;
            }

            if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimalValue))
            {
                return decimalValue;
            }

            return 0;
        }

        private static DateTime GetDateTime(JsonElement row, string propertyName)
        {
            if (!row.TryGetProperty(propertyName, out var value))
            {
                return DateTime.UtcNow;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString() ?? string.Empty;

                if (TryParseSapDate(text, out var sapDate))
                {
                    return sapDate;
                }

                if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedDate))
                {
                    return parsedDate;
                }
            }

            return DateTime.UtcNow;
        }

        private static bool TryParseSapDate(string text, out DateTime value)
        {
            value = default;
            var match = SapDateRegex.Match(text);

            if (!match.Success)
            {
                return false;
            }

            if (!long.TryParse(match.Groups["ms"].Value, out var milliseconds))
            {
                return false;
            }

            value = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
            return true;
        }
    }
}
