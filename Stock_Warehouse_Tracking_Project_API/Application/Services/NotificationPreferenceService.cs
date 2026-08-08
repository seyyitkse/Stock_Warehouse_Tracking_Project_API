using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class NotificationPreferencesDto
{
    public bool EmailEnabled { get; set; }
    public string? AlertEmail { get; set; }
    public bool WeeklyReportEnabled { get; set; }
    public string WeeklyReportDay { get; set; } = "Monday";
}

public class UpdateNotificationPreferencesRequest
{
    public bool EmailEnabled { get; set; }
    public string? AlertEmail { get; set; }
    public bool WeeklyReportEnabled { get; set; }
    public string? WeeklyReportDay { get; set; }
}

public interface INotificationPreferenceService
{
    Task<NotificationPreferencesDto> GetForUserAsync(int userId, CancellationToken ct = default);
    Task<NotificationPreferencesDto> UpsertForUserAsync(int userId, UpdateNotificationPreferencesRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<UserNotificationPreference>> GetWeeklyRecipientsAsync(DayOfWeek day, CancellationToken ct = default);
}

public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public NotificationPreferenceService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<NotificationPreferencesDto> GetForUserAsync(int userId, CancellationToken ct = default)
    {
        var pref = await _db.UserNotificationPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (pref is null)
        {
            return new NotificationPreferencesDto
            {
                EmailEnabled = !string.IsNullOrWhiteSpace(_configuration["Integrations:SendGrid:AlertEmail"]),
                AlertEmail = _configuration["Integrations:SendGrid:AlertEmail"] ?? "",
                WeeklyReportEnabled = false,
                WeeklyReportDay = DayOfWeek.Monday.ToString()
            };
        }

        return Map(pref);
    }

    public async Task<NotificationPreferencesDto> UpsertForUserAsync(
        int userId,
        UpdateNotificationPreferencesRequest request,
        CancellationToken ct = default)
    {
        var pref = await _db.UserNotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        var day = DayOfWeek.Monday;
        if (!string.IsNullOrWhiteSpace(request.WeeklyReportDay) &&
            Enum.TryParse<DayOfWeek>(request.WeeklyReportDay, true, out var parsed))
        {
            day = parsed;
        }

        if (pref is null)
        {
            pref = new UserNotificationPreference { UserId = userId };
            _db.UserNotificationPreferences.Add(pref);
        }

        pref.AlertEmail = request.AlertEmail;
        pref.EmailEnabled = request.EmailEnabled;
        pref.WeeklyReportEnabled = request.WeeklyReportEnabled;
        pref.WeeklyReportDay = day;
        pref.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Map(pref);
    }

    public async Task<IReadOnlyList<UserNotificationPreference>> GetWeeklyRecipientsAsync(
        DayOfWeek day,
        CancellationToken ct = default)
    {
        return await _db.UserNotificationPreferences
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.WeeklyReportEnabled && p.EmailEnabled && p.WeeklyReportDay == day)
            .Where(p => p.AlertEmail != null && p.AlertEmail != "")
            .ToListAsync(ct);
    }

    private static NotificationPreferencesDto Map(UserNotificationPreference pref) => new()
    {
        EmailEnabled = pref.EmailEnabled,
        AlertEmail = pref.AlertEmail ?? "",
        WeeklyReportEnabled = pref.WeeklyReportEnabled,
        WeeklyReportDay = pref.WeeklyReportDay.ToString()
    };
}
