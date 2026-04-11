using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Domain.Enums;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class NewStockService : INewStockService
{
    private readonly AppDbContext _db;
    private readonly ISapClient _sap;
    private readonly IOperationLogService _opLog;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<NewStockService> _logger;

    public NewStockService(
        AppDbContext db,
        ISapClient sap,
        IOperationLogService opLog,
        ICurrentUserService currentUser,
        ILogger<NewStockService> logger)
    {
        _db = db; _sap = sap; _opLog = opLog; _currentUser = currentUser; _logger = logger;
    }

    public async Task<IReadOnlyList<StockDto>> GetStocksAsync(string? matnr = null, string? whId = null, CancellationToken ct = default)
    {
        var rows = await _sap.GetStockListAsync(matnr, whId, ct);
        return rows.Select(ToDto).ToList();
    }

    public async Task<StockDto?> GetStockDetailAsync(string matnr, string whId, CancellationToken ct = default)
    {
        var row = await _sap.GetStockDetailAsync(matnr, whId, ct);
        return row is null ? null : ToDto(row);
    }

    public async Task<StockDto> StockInAsync(StockInRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Stok girişi: Matnr={Matnr}, WhId={WhId}, Qty={Qty}", request.MaterialNo, request.WarehouseId, request.Quantity);

        var sapResult = await _sap.StockInAsync(new SapStockInRequest
        {
            Matnr = request.MaterialNo,
            WhId = request.WarehouseId,
            Quantity = request.Quantity,
            RefNo = request.RefNo
        }, ct);

        if (!sapResult.Success)
        {
            _logger.LogWarning("SAP stok girişi başarısız: {Error}", sapResult.ErrorMessage);
            await _opLog.LogAsync(_currentUser.UserId, "StockIn", "Stock", false,
                $"Matnr={request.MaterialNo}, WhId={request.WarehouseId}, Qty={request.Quantity}",
                sapResult.ErrorMessage, ct);
            throw new InvalidOperationException($"SAP hatası: {sapResult.ErrorMessage}");
        }

        await SaveMovementAsync(request.MaterialNo, request.WarehouseId, null, MovementType.In, request.Quantity, request.RefNo ?? sapResult.SapDocNo, ct);

        await _opLog.LogAsync(_currentUser.UserId, "StockIn", "Stock", true,
            $"Matnr={request.MaterialNo}, WhId={request.WarehouseId}, Qty={request.Quantity}, SapDoc={sapResult.SapDocNo}", ct: ct);

        var updated = await _sap.GetStockDetailAsync(request.MaterialNo, request.WarehouseId, ct);
        return updated is not null ? ToDto(updated) : new StockDto { MaterialNo = request.MaterialNo, WarehouseId = request.WarehouseId, Quantity = request.Quantity };
    }

    public async Task<StockDto> StockOutAsync(StockOutRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Stok çıkışı: Matnr={Matnr}, WhId={WhId}, Qty={Qty}", request.MaterialNo, request.WarehouseId, request.Quantity);

        var sapResult = await _sap.StockOutAsync(new SapStockOutRequest
        {
            Matnr = request.MaterialNo,
            WhId = request.WarehouseId,
            Quantity = request.Quantity,
            RefNo = request.RefNo
        }, ct);

        if (!sapResult.Success)
        {
            _logger.LogWarning("SAP stok çıkışı başarısız: {Error}", sapResult.ErrorMessage);
            await _opLog.LogAsync(_currentUser.UserId, "StockOut", "Stock", false,
                $"Matnr={request.MaterialNo}, WhId={request.WarehouseId}, Qty={request.Quantity}",
                sapResult.ErrorMessage, ct);
            throw new InvalidOperationException($"SAP hatası: {sapResult.ErrorMessage}");
        }

        await SaveMovementAsync(request.MaterialNo, request.WarehouseId, null, MovementType.Out, request.Quantity, request.RefNo ?? sapResult.SapDocNo, ct);

        await _opLog.LogAsync(_currentUser.UserId, "StockOut", "Stock", true,
            $"Matnr={request.MaterialNo}, WhId={request.WarehouseId}, Qty={request.Quantity}, SapDoc={sapResult.SapDocNo}", ct: ct);

        var updated = await _sap.GetStockDetailAsync(request.MaterialNo, request.WarehouseId, ct);
        return updated is not null ? ToDto(updated) : new StockDto { MaterialNo = request.MaterialNo, WarehouseId = request.WarehouseId, Quantity = 0 };
    }

    public async Task<StockDto> TransferAsync(StockTransferRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Depo transferi: Matnr={Matnr}, Src={Src}, Dest={Dest}, Qty={Qty}",
            request.MaterialNo, request.SourceWarehouseId, request.DestWarehouseId, request.Quantity);

        var sapResult = await _sap.TransferStockAsync(new SapTransferRequest
        {
            Matnr = request.MaterialNo,
            SourceWhId = request.SourceWarehouseId,
            DestWhId = request.DestWarehouseId,
            Quantity = request.Quantity,
            RefNo = request.RefNo
        }, ct);

        if (!sapResult.Success)
        {
            _logger.LogWarning("SAP transfer başarısız: {Error}", sapResult.ErrorMessage);
            await _opLog.LogAsync(_currentUser.UserId, "Transfer", "Stock", false,
                $"Matnr={request.MaterialNo}, Src={request.SourceWarehouseId}, Dest={request.DestWarehouseId}, Qty={request.Quantity}",
                sapResult.ErrorMessage, ct);
            throw new InvalidOperationException($"SAP hatası: {sapResult.ErrorMessage}");
        }

        await SaveMovementAsync(request.MaterialNo, request.SourceWarehouseId, request.DestWarehouseId,
            MovementType.Transfer, request.Quantity, request.RefNo ?? sapResult.SapDocNo, ct);

        await _opLog.LogAsync(_currentUser.UserId, "Transfer", "Stock", true,
            $"Matnr={request.MaterialNo}, Src={request.SourceWarehouseId}, Dest={request.DestWarehouseId}, Qty={request.Quantity}, SapDoc={sapResult.SapDocNo}", ct: ct);

        var updated = await _sap.GetStockDetailAsync(request.MaterialNo, request.DestWarehouseId, ct);
        return updated is not null ? ToDto(updated) : new StockDto { MaterialNo = request.MaterialNo, WarehouseId = request.DestWarehouseId, Quantity = request.Quantity };
    }

    private async Task SaveMovementAsync(
        string matnr, string srcWhCode, string? destWhCode,
        MovementType type, decimal qty, string? refNo,
        CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Code == matnr, ct);
        var srcWarehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.Code == srcWhCode, ct);
        var destWarehouse = destWhCode is not null
            ? await _db.Warehouses.FirstOrDefaultAsync(w => w.Code == destWhCode, ct)
            : null;

        if (product is null || srcWarehouse is null) return; // MSSQL'de kayıt yoksa hareketi kaydetme

        _db.StockMovements.Add(new StockMovement
        {
            Type = type,
            Quantity = qty,
            Date = DateTime.UtcNow,
            UserId = _currentUser.UserId ?? 0,
            ProductId = product.ProductId,
            SourceWarehouseId = srcWarehouse.WarehouseId,
            DestWarehouseId = destWarehouse?.WarehouseId,
            RefNo = refNo,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }

    private static StockDto ToDto(SapStockRow row) => new()
    {
        MaterialNo = row.Matnr,
        WarehouseId = row.WhId,
        Quantity = row.Quantity,
        UpdatedAt = row.UpdatedAt
    };
}
