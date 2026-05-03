namespace Stock_Warehouse_Tracking_Project_API.Configuration;

/// <summary>
/// SAP NetWeaver RFC connection settings (SapNwRfc connection string).
/// </summary>
public class SapRfcOptions
{
    public const string SectionName = "SapRfc";

    /// <summary>Application server host (ASHOST).</summary>
    public string AppServerHost { get; set; } = string.Empty;

    /// <summary>System number (SYSNR), e.g. 00.</summary>
    public string SystemNumber { get; set; } = "00";

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>SAP client (mandant), e.g. 001.</summary>
    public string Client { get; set; } = "100";

    public string Language { get; set; } = "EN";

    /// <summary>Connection pool size (SapConnectionPool).</summary>
    public int PoolSize { get; set; } = 5;

    /// <summary>Seconds before idle pooled connections are disposed.</summary>
    public int IdleTimeoutSeconds { get; set; } = 30;

    /// <summary>Builds a SapNwRfc-compatible connection string.</summary>
    public string BuildConnectionString()
    {
        // Semicolons separate parameters; values should not contain unescaped ';'
        return $"AppServerHost={AppServerHost}; SystemNumber={SystemNumber}; User={User}; Password={Password}; Client={Client}; Language={Language}; PoolSize={PoolSize}";
    }
}
