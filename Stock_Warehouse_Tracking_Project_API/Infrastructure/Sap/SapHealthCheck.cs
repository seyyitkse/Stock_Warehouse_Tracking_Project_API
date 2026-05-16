using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SapNwRfc;
using SapNwRfc.Exceptions;
using SapNwRfc.Pooling;
using Stock_Warehouse_Tracking_Project_API.Configuration;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap.Http;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

public sealed class SapHealthCheck : IHealthCheck
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public SapHealthCheck(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var provider = SapClientConfiguration.GetProvider(_configuration);

        return provider switch
        {
            SapClientProvider.Mock => HealthCheckResult.Healthy("SAP mock is enabled."),
            SapClientProvider.Http => await CheckHttpAsync(cancellationToken),
            SapClientProvider.Rfc => CheckRfc(cancellationToken),
            _ => HealthCheckResult.Unhealthy($"Unknown SapClient provider: {provider}")
        };
    }

    private async Task<HealthCheckResult> CheckHttpAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            var options = _serviceProvider.GetRequiredService<IOptions<SapHttpOptions>>().Value;
            var client = factory.CreateClient(SapClientConfiguration.HttpClientName);

            using var response = await client.GetAsync(options.StockListPath, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"SAP HTTP stock list failed ({(int)response.StatusCode}).");
            }

            if (!string.IsNullOrWhiteSpace(body))
            {
                JsonSerializer.Deserialize<List<SapStockJsonDto>>(body, JsonOptions);
            }

            return HealthCheckResult.Healthy("SAP HTTP stock list ok.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SAP HTTP healthcheck error.", ex);
        }
    }

    private HealthCheckResult CheckRfc(CancellationToken cancellationToken)
    {
        try
        {
            SapLibrary.EnsureLibraryPresent();

            var pool = _serviceProvider.GetRequiredService<ISapConnectionPool>();
            var conn = pool.GetConnection(cancellationToken);
            try
            {
                var ok = conn.Ping();
                return ok
                    ? HealthCheckResult.Healthy("SAP RFC ping ok.")
                    : HealthCheckResult.Unhealthy("SAP RFC ping failed.");
            }
            finally
            {
                pool.ReturnConnection(conn);
            }
        }
        catch (SapLibraryNotFoundException ex)
        {
            return HealthCheckResult.Unhealthy("SAP NW RFC SDK binaries not found.", ex);
        }
        catch (SapException ex)
        {
            return HealthCheckResult.Unhealthy("SAP RFC error.", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Unexpected SAP healthcheck error.", ex);
        }
    }
}
