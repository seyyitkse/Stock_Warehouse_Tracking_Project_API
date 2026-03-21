using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Services
{
    public interface ISapStockClient
    {
        Task<IReadOnlyList<SapStockRow>> GetStockListAsync(CancellationToken cancellationToken = default);
    }
}
