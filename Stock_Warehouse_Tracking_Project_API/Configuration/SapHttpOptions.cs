namespace Stock_Warehouse_Tracking_Project_API.Configuration
{
    public class SapHttpOptions
    {
        public const string SectionName = "SapHttp";

        public bool UseMock { get; set; } = true;
        public string BaseUrl { get; set; } = string.Empty;
        public string StocksPath { get; set; } = "/sap/opu/odata/sap/ZSTOCK_SRV/Stocks?$format=json";
        public string? Client { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }
}
