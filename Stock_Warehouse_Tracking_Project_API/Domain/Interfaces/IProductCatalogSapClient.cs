using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

public interface IProductCatalogSapClient
{
    Task<IReadOnlyList<SapProductRow>> GetProductListAsync(CancellationToken ct = default);
}
