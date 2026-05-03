using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;
using SapNwRfc.Pooling;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

/// <summary>
/// Production SAP RFC client using SapNwRfc (<see cref="ISapPooledConnection"/>).
/// Requires SAP NW RFC SDK native DLLs on the host (see README).
/// </summary>
public sealed class RfcSapClient : ISapClient
{
    private readonly ISapPooledConnection _pooled;
    private readonly ILogger<RfcSapClient> _logger;

    public RfcSapClient(ISapPooledConnection pooled, ILogger<RfcSapClient> logger)
    {
        _pooled = pooled;
        _logger = logger;
    }

    public Task<IReadOnlyList<SapStockRow>> GetStockListAsync(
        string? matnr = null,
        string? whId = null,
        CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            var input = new ZGetStockListInput
            {
                IvMatnr = AbapTypeConverters.ToAbapOptionalFilter(matnr),
                IvWhId = AbapTypeConverters.ToAbapOptionalFilter(whId)
            };

            var result = _pooled.InvokeFunction<ZGetStockListOutput>("Z_GET_STOCK_LIST", input, ct);
            var rows = result.EtStock ?? Array.Empty<ZbkStockRfcRow>();
            IReadOnlyList<SapStockRow> list = rows.Select(MapRow).ToList();
            return list;
        }, ct);
    }

    public Task<SapStockRow?> GetStockDetailAsync(string matnr, string whId, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            var input = new ZGetStockDetailInput
            {
                IvMatnr = matnr.Trim(),
                IvWhId = whId.Trim()
            };

            var result = _pooled.InvokeFunction<ZGetStockDetailOutput>("Z_GET_STOCK_DETAIL", input, ct);
            if (!result.EvFound || result.EsStock is null)
                return (SapStockRow?)null;

            return MapRow(result.EsStock);
        }, ct);
    }

    public Task<SapCreateProductResult> CreateProductAsync(SapCreateProductRequest request, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            var input = new ZCreateProductInput
            {
                IvMatnr = request.Matnr.Trim(),
                IvName = request.Name.Trim(),
                IvUnit = request.Unit.Trim(),
                IvCategory = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim()
            };

            var result = _pooled.InvokeFunction<ZCreateProductOutput>("Z_CREATE_PRODUCT", input, ct);
            return new SapCreateProductResult
            {
                Success = result.EvSuccess,
                SapDocNo = result.EvDocNo,
                ErrorMessage = string.IsNullOrWhiteSpace(result.EvError) ? null : result.EvError
            };
        }, ct);
    }

    public Task<SapMovementResult> StockInAsync(SapStockInRequest request, CancellationToken ct = default)
    {
        return Task.Run(() => InvokeMovement("Z_STOCK_IN", request.Matnr, request.WhId, request.Quantity, request.RefNo, ct), ct);
    }

    public Task<SapMovementResult> StockOutAsync(SapStockOutRequest request, CancellationToken ct = default)
    {
        return Task.Run(() => InvokeMovement("Z_STOCK_OUT", request.Matnr, request.WhId, request.Quantity, request.RefNo, ct), ct);
    }

    public Task<SapMovementResult> TransferStockAsync(SapTransferRequest request, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            var input = new ZTransferStockInput
            {
                IvMatnr = request.Matnr.Trim(),
                IvSrcWh = request.SourceWhId.Trim(),
                IvDestWh = request.DestWhId.Trim(),
                IvQuantity = request.Quantity,
                IvRefNo = request.RefNo
            };

            var result = _pooled.InvokeFunction<ZStockMovementOutput>("Z_TRANSFER_STOCK", input, ct);
            return new SapMovementResult
            {
                Success = result.EvSuccess,
                SapDocNo = result.EvDocNo,
                ErrorMessage = string.IsNullOrWhiteSpace(result.EvError) ? null : result.EvError
            };
        }, ct);
    }

    private SapMovementResult InvokeMovement(
        string functionName,
        string matnr,
        string whId,
        decimal quantity,
        string? refNo,
        CancellationToken ct)
    {
        var input = new ZStockMovementInput
        {
            IvMatnr = matnr.Trim(),
            IvWhId = whId.Trim(),
            IvQuantity = quantity,
            IvRefNo = refNo
        };

        var result = _pooled.InvokeFunction<ZStockMovementOutput>(functionName, input, ct);
        return new SapMovementResult
        {
            Success = result.EvSuccess,
            SapDocNo = result.EvDocNo,
            ErrorMessage = string.IsNullOrWhiteSpace(result.EvError) ? null : result.EvError
        };
    }

    private static SapStockRow MapRow(ZbkStockRfcRow row) => new()
    {
        Matnr = row.Matnr?.Trim() ?? string.Empty,
        WhId = row.WhId?.Trim() ?? string.Empty,
        Quantity = row.Quantity,
        UpdatedAt = row.UpdateAt.HasValue
            ? AbapTypeConverters.ToUtcDate(row.UpdateAt.Value)
            : DateTime.UtcNow
    };
}
