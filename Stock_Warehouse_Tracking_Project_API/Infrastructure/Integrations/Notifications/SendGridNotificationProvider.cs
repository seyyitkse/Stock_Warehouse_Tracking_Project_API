using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Integrations.Notifications;

public class SendGridNotificationProvider : INotificationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendGridNotificationProvider> _logger;

    public SendGridNotificationProvider(IConfiguration configuration, ILogger<SendGridNotificationProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string Name => "SendGrid";

    public Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        var apiKey = _configuration["Integrations:SendGrid:ApiKey"];
        return Task.FromResult(!string.IsNullOrWhiteSpace(apiKey));
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var apiKey = _configuration["Integrations:SendGrid:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("SendGrid API key yapılandırılmamış; e-posta gönderilmedi.");
            return false;
        }

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var fromEmail = _configuration["Integrations:SendGrid:FromEmail"] ?? "noreply@stockwarehouse.local";
        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = to } } } },
            from = new { email = fromEmail },
            subject,
            content = new[] { new { type = "text/plain", value = body } }
        };

        var response = await client.PostAsJsonAsync("https://api.sendgrid.com/v3/mail/send", payload, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("SendGrid gönderimi başarısız: {Status}", response.StatusCode);
            return false;
        }

        return true;
    }
}
