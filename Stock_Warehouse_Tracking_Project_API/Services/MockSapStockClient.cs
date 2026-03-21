using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Services
{
    public class MockSapStockClient : ISapStockClient
    {
        public Task<IReadOnlyList<SapStockRow>> GetStockListAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SapStockRow> rows = new List<SapStockRow>
            {
                new()
                {
                    Matnr = "MAT-1001",
                    WhId = "WH-01",
                    Quantity = 120,
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-15)
                },
                new()
                {
                    Matnr = "MAT-1002",
                    WhId = "WH-01",
                    Quantity = 45,
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-8)
                },
                new()
                {
                    Matnr = "MAT-1003",
                    WhId = "WH-02",
                    Quantity = 200,
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-2)
                }
            };

            return Task.FromResult(rows);
        }
    }
}
