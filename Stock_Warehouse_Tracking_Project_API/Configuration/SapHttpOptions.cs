namespace Stock_Warehouse_Tracking_Project_API.Configuration;

public class SapHttpOptions
{
    public const string SectionName = "SapHttp";

    public string BaseUrl { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Client { get; set; } = "001";

    public string Language { get; set; } = "EN";

    public int TimeoutSeconds { get; set; } = 30;

    public string StockListPath { get; set; } = "sap/bc/zstock/stock";

    public string StockDetailPath { get; set; } = "sap/bc/zstock/stock/{matnr}/{whId}";

    public string StockInPath { get; set; } = "sap/bc/zstock/stock/in";

    public string StockOutPath { get; set; } = "sap/bc/zstock/stock/out";

    public string TransferPath { get; set; } = "sap/bc/zstock/stock/transfer";

    public string ProductsPath { get; set; } = "sap/bc/zstock/products";
}
