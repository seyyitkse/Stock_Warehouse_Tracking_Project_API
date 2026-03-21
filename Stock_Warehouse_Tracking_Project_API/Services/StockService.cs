using Stock_Warehouse_Tracking_Project_API.Models.Stock;

namespace Stock_Warehouse_Tracking_Project_API.Services
{
    public class StockService : IStockService
    {
        private readonly ISapStockClient _sapStockClient;

        public StockService(ISapStockClient sapStockClient)
        {
            _sapStockClient = sapStockClient;
        }

        public async Task<IReadOnlyList<StockDto>> GetStocksAsync(CancellationToken cancellationToken = default)
        {
            var sapRows = await _sapStockClient.GetStockListAsync(cancellationToken);

            return sapRows.Select(x => new StockDto
            {
                MaterialNo = x.Matnr,
                WarehouseId = x.WhId,
                Quantity = x.Quantity,
                UpdatedAt = x.UpdatedAt
            }).ToList();
        }
    }
}
