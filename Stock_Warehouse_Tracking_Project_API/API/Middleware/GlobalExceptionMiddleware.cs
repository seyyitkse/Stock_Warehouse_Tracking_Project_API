using System.Net;
using System.Text.Json;
using FluentValidation;
using SapNwRfc.Exceptions;

namespace Stock_Warehouse_Tracking_Project_API.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlenmeyen hata: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title, detail) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.UnprocessableEntity,
                "Doğrulama hatası",
                ve.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            ),
            SapLibraryNotFoundException => (
                HttpStatusCode.ServiceUnavailable,
                "SAP bağımlılığı eksik",
                new[] { "SAP NW RFC SDK (sapnwrfc) native kütüphaneleri bulunamadı." }.AsEnumerable()
            ),
            SapCommunicationFailedException => (
                HttpStatusCode.ServiceUnavailable,
                "SAP bağlantı hatası",
                new[] { exception.Message }.AsEnumerable()
            ),
            SapException => (
                HttpStatusCode.ServiceUnavailable,
                "SAP RFC hatası",
                new[] { exception.Message }.AsEnumerable()
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Yetkisiz erişim",
                new[] { exception.Message }.AsEnumerable()
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Kayıt bulunamadı",
                new[] { exception.Message }.AsEnumerable()
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "Geçersiz işlem",
                new[] { exception.Message }.AsEnumerable()
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Sunucu hatası",
                new[] { "Beklenmeyen bir hata oluştu." }.AsEnumerable()
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            title,
            errors = detail,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
