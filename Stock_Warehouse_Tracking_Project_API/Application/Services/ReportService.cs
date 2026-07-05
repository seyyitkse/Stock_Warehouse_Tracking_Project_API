using System.Text;
using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Report;
using Stock_Warehouse_Tracking_Project_API.Domain.Enums;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly INewStockService _stockService;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;
    private readonly IStockThresholdService _thresholdService;

    public ReportService(
        AppDbContext db,
        INewStockService stockService,
        IProductService productService,
        IWarehouseService warehouseService,
        IStockThresholdService thresholdService)
    {
        _db = db;
        _stockService = stockService;
        _productService = productService;
        _warehouseService = warehouseService;
        _thresholdService = thresholdService;
    }

    public async Task<StockSummaryReportDto> GetStockSummaryAsync(CancellationToken ct = default)
    {
        var stocks = await _stockService.GetStocksAsync(ct: ct);
        var products = await _productService.GetAllAsync(ct);
        var warehouses = await _warehouseService.GetAllAsync(ct);
        var lowStock = await _thresholdService.GetLowStockCountAsync(ct);

        return new StockSummaryReportDto
        {
            TotalQuantity = stocks.Sum(s => s.Quantity),
            ProductCount = products.Count,
            WarehouseCount = warehouses.Count,
            LowStockCount = lowStock,
            EmptyStockLines = stocks.Count(s => s.Quantity <= 0)
        };
    }

    public async Task<IReadOnlyList<MovementTrendPointDto>> GetMovementTrendAsync(string granularity = "daily", CancellationToken ct = default)
    {
        var since = granularity == "weekly"
            ? DateTime.UtcNow.AddDays(-84)
            : DateTime.UtcNow.AddDays(-30);

        var movements = await _db.StockMovements
            .AsNoTracking()
            .Where(m => m.Date >= since)
            .ToListAsync(ct);

        IEnumerable<IGrouping<DateTime, Domain.Entities.StockMovement>> grouped = granularity == "weekly"
            ? movements.GroupBy(m => StartOfWeek(m.Date))
            : movements.GroupBy(m => m.Date.Date);

        return grouped
            .OrderBy(g => g.Key)
            .Select(g => new MovementTrendPointDto
            {
                Label = granularity == "weekly"
                    ? $"Hafta {g.Key:dd.MM.yyyy}"
                    : g.Key.ToString("dd.MM.yyyy"),
                InCount = g.Count(x => x.Type == MovementType.In),
                OutCount = g.Count(x => x.Type == MovementType.Out),
                TransferCount = g.Count(x => x.Type == MovementType.Transfer)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<WarehouseComparisonDto>> GetWarehouseComparisonAsync(CancellationToken ct = default)
    {
        var stocks = await _stockService.GetStocksAsync(ct: ct);
        var warehouses = await _warehouseService.GetAllAsync(ct);
        var warehouseByCode = warehouses.ToDictionary(w => w.Code, StringComparer.OrdinalIgnoreCase);

        return stocks
            .GroupBy(s => s.WarehouseId)
            .Select(g =>
            {
                warehouseByCode.TryGetValue(g.Key, out var wh);
                return new WarehouseComparisonDto
                {
                    WarehouseCode = g.Key,
                    WarehouseName = wh?.Name ?? g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    LineCount = g.Count()
                };
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ToList();
    }

    public async Task<byte[]> ExportMovementsCsvAsync(CancellationToken ct = default)
    {
        var movements = await _db.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.SourceWarehouse)
            .Include(m => m.DestWarehouse)
            .Include(m => m.User)
            .OrderByDescending(m => m.Date)
            .Take(5000)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Tarih,Islem,Malzeme,Miktar,KaynakDepo,HedefDepo,Kullanici,RefNo");
        foreach (var m in movements)
        {
            sb.AppendLine(string.Join(",",
                m.Date.ToString("O"),
                m.Type,
                Escape(m.Product.Code),
                m.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Escape(m.SourceWarehouse?.Code),
                Escape(m.DestWarehouse?.Code),
                Escape(m.User?.Name),
                Escape(m.RefNo)));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static string Escape(string? value)
    {
        var text = value ?? "";
        return text.Contains(',') ? $"\"{text.Replace("\"", "\"\"")}\"" : text;
    }
}
