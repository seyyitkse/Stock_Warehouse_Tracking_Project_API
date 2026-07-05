namespace Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

public interface INotificationProvider
{
    string Name { get; }
    Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}
