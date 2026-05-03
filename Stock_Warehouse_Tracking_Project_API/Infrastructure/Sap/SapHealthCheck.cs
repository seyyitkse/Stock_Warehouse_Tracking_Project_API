using Microsoft.Extensions.Diagnostics.HealthChecks;
using SapNwRfc;
using SapNwRfc.Exceptions;
using SapNwRfc.Pooling;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

public sealed class SapHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    private readonly ISapConnectionPool _pool;

    public SapHealthCheck(IConfiguration configuration, ISapConnectionPool pool)
    {
        _configuration = configuration;
        _pool = pool;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // When mock is enabled, we intentionally skip native RFC dependency checks.
        if (_configuration.GetValue<bool>("SapClient:UseMock"))
            return Task.FromResult(HealthCheckResult.Healthy("SAP mock is enabled."));

        try
        {
            // Fail fast with a meaningful message when native SDK DLLs are missing.
            SapLibrary.EnsureLibraryPresent();

            var conn = _pool.GetConnection(cancellationToken);
            try
            {
                var ok = conn.Ping();
                return Task.FromResult(ok
                    ? HealthCheckResult.Healthy("SAP RFC ping ok.")
                    : HealthCheckResult.Unhealthy("SAP RFC ping failed."));
            }
            finally
            {
                _pool.ReturnConnection(conn);
            }
        }
        catch (SapLibraryNotFoundException ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("SAP NW RFC SDK binaries not found.", ex));
        }
        catch (SapException ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("SAP RFC error.", ex));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Unexpected SAP healthcheck error.", ex));
        }
    }
}

