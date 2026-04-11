using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface INewStockService
{
    Task<IReadOnlyList<StockDto>> GetStocksAsync(string? matnr = null, string? whId = null, CancellationToken ct = default);
    Task<StockDto?> GetStockDetailAsync(string matnr, string whId, CancellationToken ct = default);
    Task<StockDto> StockInAsync(StockInRequest request, CancellationToken ct = default);
    Task<StockDto> StockOutAsync(StockOutRequest request, CancellationToken ct = default);
    Task<StockDto> TransferAsync(StockTransferRequest request, CancellationToken ct = default);
}
