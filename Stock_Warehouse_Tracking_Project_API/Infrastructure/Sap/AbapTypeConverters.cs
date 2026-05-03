namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

/// <summary>
/// Helpers for SAP RFC types (DATS, optional CHAR, BOOLE_D).
/// SapNwRfc maps RFCTYPE_DATE to DateTime; use this for edge cases.
/// </summary>
public static class AbapTypeConverters
{
    /// <summary>
    /// SAP optional filtering: empty or whitespace means "no filter" (matches ABAP IS INITIAL for CHAR).
    /// </summary>
    public static string ToAbapOptionalFilter(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    /// <summary>
    /// Interpret BOOLE_D / ABAP_BOOL style fields from RFC (X / space or bool).
    /// </summary>
    public static bool IsAbapTrue(object? value) => value switch
    {
        bool b => b,
        string s => !string.IsNullOrWhiteSpace(s) && s.Trim().Equals("X", StringComparison.OrdinalIgnoreCase),
        char c => c == 'X' || c == 'x',
        _ => false
    };

    public static DateTime ToUtcDate(DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Utc) return dt;
        if (dt.Kind == DateTimeKind.Local) return dt.ToUniversalTime();
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }
}
