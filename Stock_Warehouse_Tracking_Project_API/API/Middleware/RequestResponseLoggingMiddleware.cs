using System.Diagnostics;

namespace Stock_Warehouse_Tracking_Project_API.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "[{CorrelationId}] → {Method} {Path}{QueryString}",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString);

        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "[{CorrelationId}] ← {StatusCode} {ElapsedMs}ms",
            correlationId,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds);
    }
}
