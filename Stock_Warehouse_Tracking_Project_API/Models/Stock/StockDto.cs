namespace Stock_Warehouse_Tracking_Project_API.Models.Stock
{
    public class StockDto
    {
        public string MaterialNo { get; set; } = string.Empty;
        public string WarehouseId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
