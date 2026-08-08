namespace Stock_Warehouse_Tracking_Project_API.Domain.Enums;

public static class EventLogSource
{
    public const string User = "User";
    public const string System = "System";
    public const string Integration = "Integration";
}

public static class EventLogSeverity
{
    public const string Info = "Info";
    public const string Warning = "Warning";
    public const string Error = "Error";
}
