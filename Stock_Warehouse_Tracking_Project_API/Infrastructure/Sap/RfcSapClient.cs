using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

/// <summary>
/// Production SAP RFC client. Her metot gerçek NCo3/SapNwRfc çağrısıyla implement edilmelidir.
/// </summary>
public class RfcSapClient : ISapClient
{
    public Task<IReadOnlyList<SapStockRow>> GetStockListAsync(string? matnr = null, string? whId = null, CancellationToken ct = default)
        => throw new NotImplementedException("TODO: Z_GET_STOCK_LIST RFC çağrısı implement edilmeli.");

    public Task<SapStockRow?> GetStockDetailAsync(string matnr, string whId, CancellationToken ct = default)
        => throw new NotImplementedException("TODO: Z_GET_STOCK_DETAIL RFC çağrısı implement edilmeli.");

    public Task<SapCreateProductResult> CreateProductAsync(SapCreateProductRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("TODO: Z_CREATE_PRODUCT RFC çağrısı implement edilmeli.");

    public Task<SapMovementResult> StockInAsync(SapStockInRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("TODO: Z_STOCK_IN RFC çağrısı implement edilmeli.");

    public Task<SapMovementResult> StockOutAsync(SapStockOutRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("TODO: Z_STOCK_OUT RFC çağrısı implement edilmeli.");

    public Task<SapMovementResult> TransferStockAsync(SapTransferRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("TODO: Z_TRANSFER_STOCK RFC çağrısı implement edilmeli.");
}
