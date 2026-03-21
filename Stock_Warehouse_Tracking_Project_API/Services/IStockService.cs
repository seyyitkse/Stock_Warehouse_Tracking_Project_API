using Stock_Warehouse_Tracking_Project_API.Models.Stock;

namespace Stock_Warehouse_Tracking_Project_API.Services
{
    public interface IStockService
    {
        Task<IReadOnlyList<StockDto>> GetStocksAsync(CancellationToken cancellationToken = default);
    }
}
