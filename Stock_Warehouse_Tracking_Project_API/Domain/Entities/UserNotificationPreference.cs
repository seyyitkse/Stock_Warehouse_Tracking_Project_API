namespace Stock_Warehouse_Tracking_Project_API.Domain.Entities;

public class UserNotificationPreference
{
    public int PreferenceId { get; set; }
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string? AlertEmail { get; set; }
    public bool EmailEnabled { get; set; } = true;
    public bool WeeklyReportEnabled { get; set; }
    public DayOfWeek WeeklyReportDay { get; set; } = DayOfWeek.Monday;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
