using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Alert;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Domain.Enums;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;
using Stock_Warehouse_Tracking_Project_API.API.Hubs;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class NewStockService : INewStockService
{
    private readonly AppDbContext _db;
    private readonly ISapClient _sap;
    private readonly IOperationLogService _opLog;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<NewStockService> _logger;
    private readonly IStockNotificationService? _stockNotify;

    public NewStockService(
        AppDbContext db,
        ISapClient sap,
        IOperationLogService opLog,
        ICurrentUserService currentUser,
        ILogger<NewStockService> logger,
        IStockNotificationService? stockNotify = null)
    {
        _db = db; _sap = sap; _opLog = opLog; _currentUser = currentUser; _logger = logger;
        _stockNotify = stockNotify;
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
        var dto = updated is not null ? ToDto(updated) : new StockDto { MaterialNo = request.MaterialNo, WarehouseId = request.WarehouseId, Quantity = request.Quantity };
        await PublishStockUpdateAsync(dto);
        return dto;
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
        var dto = updated is not null ? ToDto(updated) : new StockDto { MaterialNo = request.MaterialNo, WarehouseId = request.WarehouseId, Quantity = 0 };
        await PublishStockUpdateAsync(dto);
        return dto;
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
        var dto = updated is not null ? ToDto(updated) : new StockDto { MaterialNo = request.MaterialNo, WarehouseId = request.DestWarehouseId, Quantity = request.Quantity };
        await PublishStockUpdateAsync(dto);
        return dto;
    }

    public async Task<BulkStockInResultDto> BulkStockInAsync(BulkStockInRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();
        var success = 0;

        foreach (var item in request.Items)
        {
            try
            {
                await StockInAsync(item, ct);
                success++;
            }
            catch (Exception ex)
            {
                errors.Add($"{item.MaterialNo}/{item.WarehouseId}: {ex.Message}");
            }
        }

        return new BulkStockInResultDto
        {
            SuccessCount = success,
            FailureCount = errors.Count,
            Errors = errors
        };
    }

    private async Task SaveMovementAsync(
        string matnr, string srcWhCode, string? destWhCode,
        MovementType type, decimal qty, string? refNo,
        CancellationToken ct)
    {
        var product = await EnsureProductAsync(matnr, ct);
        var srcWarehouse = await EnsureWarehouseAsync(srcWhCode, ct);
        Warehouse? destWarehouse = null;
        if (destWhCode is not null)
            destWarehouse = await EnsureWarehouseAsync(destWhCode, ct);

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

    private async Task<Product> EnsureProductAsync(string matnr, CancellationToken ct)
    {
        var code = matnr.Trim();
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Code == code && !p.IsDeleted, ct);
        if (existing is not null)
            return existing;

        string name = code;
        string unit = "ADET";
        string? category = null;

        if (_sap is IProductCatalogSapClient catalog)
        {
            var sapProducts = await catalog.GetProductListAsync(ct);
            var sapProduct = sapProducts.FirstOrDefault(p =>
                string.Equals(p.Matnr.Trim(), code, StringComparison.OrdinalIgnoreCase));
            if (sapProduct is not null)
            {
                name = string.IsNullOrWhiteSpace(sapProduct.Name) ? code : sapProduct.Name.Trim();
                unit = string.IsNullOrWhiteSpace(sapProduct.Unit) ? unit : sapProduct.Unit.Trim();
                category = sapProduct.Category?.Trim();
            }
        }

        var product = new Product
        {
            Code = code,
            Name = name,
            Unit = unit,
            Category = category,
            CreatedAt = DateTime.UtcNow
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("SAP hareketi için otomatik ürün oluşturuldu: Code={Code}", code);
        return product;
    }

    private async Task<Warehouse> EnsureWarehouseAsync(string whCode, CancellationToken ct)
    {
        var code = whCode.Trim();
        var existing = await _db.Warehouses.FirstOrDefaultAsync(w => w.Code == code && !w.IsDeleted, ct);
        if (existing is not null)
            return existing;

        var warehouse = new Warehouse
        {
            Code = code,
            Name = code,
            CreatedAt = DateTime.UtcNow
        };
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("SAP hareketi için otomatik depo oluşturuldu: Code={Code}", code);
        return warehouse;
    }

    private async Task PublishStockUpdateAsync(StockDto dto)
    {
        if (_stockNotify is null) return;
        try
        {
            await _stockNotify.NotifyStockUpdatedAsync(dto.MaterialNo, dto.WarehouseId, dto.Quantity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR stok bildirimi gönderilemedi.");
        }
    }

    private static StockDto ToDto(SapStockRow row) => new()
    {
        MaterialNo = row.Matnr,
        WarehouseId = row.WhId,
        Quantity = row.Quantity,
        UpdatedAt = row.UpdatedAt
    };
}
