namespace Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public byte[]? AttachmentBytes { get; set; }
    public string? AttachmentFileName { get; set; }
    public string AttachmentContentType { get; set; } = "text/csv";
}

public interface INotificationProvider
{
    string Name { get; }
    Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
    Task<bool> SendEmailAsync(EmailMessage message, CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
