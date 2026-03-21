namespace Stock_Warehouse_Tracking_Project_API.Models.Sap
{
    public class SapStockRow
    {
        public string Matnr { get; set; } = string.Empty;
        public string WhId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
