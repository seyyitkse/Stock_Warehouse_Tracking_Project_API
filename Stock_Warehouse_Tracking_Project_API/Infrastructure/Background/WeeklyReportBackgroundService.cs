using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Report;
using Stock_Warehouse_Tracking_Project_API.Application.Services;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Background;

public class WeeklyReportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeeklyReportBackgroundService> _logger;
    private DateOnly? _lastRunDay;

    public WeeklyReportBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WeeklyReportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Weekly report job failed.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (_lastRunDay == today)
            return;

        // Run once per UTC day around 07:00–08:00 window
        if (DateTime.UtcNow.Hour is < 7 or >= 8)
            return;

        using var scope = _scopeFactory.CreateScope();
        var prefs = scope.ServiceProvider.GetRequiredService<INotificationPreferenceService>();
        var reports = scope.ServiceProvider.GetRequiredService<IReportService>();

        var recipients = await prefs.GetWeeklyRecipientsAsync(DateTime.UtcNow.DayOfWeek, ct);
        foreach (var recipient in recipients)
        {
            var result = await reports.EmailReportAsync(
                new EmailReportRequest
                {
                    To = recipient.AlertEmail,
                    PeriodDays = 7,
                    IncludeCsv = true
                },
                recipient.UserId,
                ct);

            _logger.LogInformation(
                "Weekly report to {Email}: {Status}",
                recipient.AlertEmail,
                result.Sent ? "sent" : "failed");
        }

        _lastRunDay = today;
    }
}
