using System.Net.Http.Json;
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

    public Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
        => SendEmailAsync(new EmailMessage { To = to, Subject = subject, Body = body }, ct);

    public async Task<bool> SendEmailAsync(EmailMessage message, CancellationToken ct = default)
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
        var content = new List<object>
        {
            new { type = "text/plain", value = message.Body }
        };
        if (!string.IsNullOrWhiteSpace(message.HtmlBody))
            content.Add(new { type = "text/html", value = message.HtmlBody });

        object payload;
        if (message.AttachmentBytes is { Length: > 0 } && !string.IsNullOrWhiteSpace(message.AttachmentFileName))
        {
            payload = new
            {
                personalizations = new[] { new { to = new[] { new { email = message.To } } } },
                from = new { email = fromEmail },
                subject = message.Subject,
                content,
                attachments = new[]
                {
                    new
                    {
                        content = Convert.ToBase64String(message.AttachmentBytes),
                        type = message.AttachmentContentType,
                        filename = message.AttachmentFileName,
                        disposition = "attachment"
                    }
                }
            };
        }
        else
        {
            payload = new
            {
                personalizations = new[] { new { to = new[] { new { email = message.To } } } },
                from = new { email = fromEmail },
                subject = message.Subject,
                content
            };
        }

        var response = await client.PostAsJsonAsync("https://api.sendgrid.com/v3/mail/send", payload, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("SendGrid gönderimi başarısız: {Status}", response.StatusCode);
            return false;
        }

        return true;
    }
}
