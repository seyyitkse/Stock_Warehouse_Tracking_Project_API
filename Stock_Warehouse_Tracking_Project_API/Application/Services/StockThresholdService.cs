using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Alert;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class StockThresholdService : IStockThresholdService
{
    private readonly AppDbContext _db;
    private readonly INewStockService _stockService;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;

    public StockThresholdService(
        AppDbContext db,
        INewStockService stockService,
        IProductService productService,
        IWarehouseService warehouseService)
    {
        _db = db;
        _stockService = stockService;
        _productService = productService;
        _warehouseService = warehouseService;
    }

    public async Task<IReadOnlyList<LowStockAlertDto>> GetLowStockAlertsAsync(CancellationToken ct = default)
    {
        var stocks = await _stockService.GetStocksAsync(ct: ct);
        var products = await _productService.GetAllAsync(ct);
        var warehouses = await _warehouseService.GetAllAsync(ct);

        var productByCode = products.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);
        var warehouseByCode = warehouses.ToDictionary(w => w.Code, StringComparer.OrdinalIgnoreCase);

        var thresholdRows = await _db.Stocks
            .AsNoTracking()
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.MinLevel > 0)
            .ToListAsync(ct);

        var thresholdByKey = thresholdRows.ToDictionary(
            s => $"{s.Product.Code}|{s.Warehouse.Code}",
            s => s.MinLevel,
            StringComparer.OrdinalIgnoreCase);

        var alerts = new List<LowStockAlertDto>();

        foreach (var stock in stocks)
        {
            productByCode.TryGetValue(stock.MaterialNo, out var product);
            warehouseByCode.TryGetValue(stock.WarehouseId, out var warehouse);

            var minLevel = ResolveMinLevel(stock.MaterialNo, stock.WarehouseId, product, thresholdByKey);
            if (minLevel <= 0 || stock.Quantity >= minLevel)
                continue;

            alerts.Add(new LowStockAlertDto
            {
                MaterialNo = stock.MaterialNo,
                ProductName = product?.Name ?? stock.MaterialNo,
                WarehouseId = stock.WarehouseId,
                WarehouseName = warehouse?.Name ?? stock.WarehouseId,
                Quantity = stock.Quantity,
                MinLevel = minLevel,
                Deficit = minLevel - stock.Quantity
            });
        }

        return alerts.OrderByDescending(a => a.Deficit).ToList();
    }

    public async Task<int> GetLowStockCountAsync(CancellationToken ct = default)
    {
        var alerts = await GetLowStockAlertsAsync(ct);
        return alerts.Count;
    }

    public async Task UpdateThresholdAsync(string matnr, string whId, decimal minLevel, CancellationToken ct = default)
    {
        var code = matnr.Trim();
        var warehouseCode = whId.Trim();

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Code == code && !p.IsDeleted, ct)
            ?? throw new KeyNotFoundException($"Ürün bulunamadı: {code}");

        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.Code == warehouseCode && !w.IsDeleted, ct)
            ?? throw new KeyNotFoundException($"Depo bulunamadı: {warehouseCode}");

        var stock = await _db.Stocks.FirstOrDefaultAsync(
            s => s.ProductId == product.ProductId && s.WarehouseId == warehouse.WarehouseId, ct);

        if (stock is null)
        {
            stock = new Stock
            {
                ProductId = product.ProductId,
                WarehouseId = warehouse.WarehouseId,
                Quantity = 0,
                MinLevel = minLevel,
                CreatedAt = DateTime.UtcNow
            };
            _db.Stocks.Add(stock);
        }
        else
        {
            stock.MinLevel = minLevel;
            stock.UpdatedAt = DateTime.UtcNow;
        }

        product.MinStock = minLevel;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    private static decimal ResolveMinLevel(
        string materialNo,
        string warehouseId,
        Application.DTOs.Product.ProductDto? product,
        Dictionary<string, decimal> thresholdByKey)
    {
        if (thresholdByKey.TryGetValue($"{materialNo}|{warehouseId}", out var specific))
            return specific;
        return product?.MinStock ?? 0;
    }
}
