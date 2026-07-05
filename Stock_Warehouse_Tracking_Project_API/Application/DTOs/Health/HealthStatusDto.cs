namespace Stock_Warehouse_Tracking_Project_API.Application.DTOs.Health;

public class HealthStatusDto
{
    public string Api { get; set; } = "healthy";
    public string Database { get; set; } = "unknown";
    public string Sap { get; set; } = "unknown";
}
