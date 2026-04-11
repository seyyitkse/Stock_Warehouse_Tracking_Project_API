using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

public interface ISapClient
{
    Task<IReadOnlyList<SapStockRow>> GetStockListAsync(
        string? matnr = null,
        string? whId = null,
        CancellationToken ct = default);

    Task<SapStockRow?> GetStockDetailAsync(
        string matnr,
        string whId,
        CancellationToken ct = default);

    Task<SapCreateProductResult> CreateProductAsync(
        SapCreateProductRequest request,
        CancellationToken ct = default);

    Task<SapMovementResult> StockInAsync(
        SapStockInRequest request,
        CancellationToken ct = default);

    Task<SapMovementResult> StockOutAsync(
        SapStockOutRequest request,
        CancellationToken ct = default);

    Task<SapMovementResult> TransferStockAsync(
        SapTransferRequest request,
        CancellationToken ct = default);
}
