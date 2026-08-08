using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Dashboard;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Movement;
using Stock_Warehouse_Tracking_Project_API.Configuration;
using Stock_Warehouse_Tracking_Project_API.Domain.Enums;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;
    private readonly INewStockService _stockService;
    private readonly IMovementService _movementService;
    private readonly IStockThresholdService _thresholdService;
    private readonly ISapClient _sap;
    private readonly IConfiguration _configuration;
    private readonly IOperationLogService _opLog;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IProductService productService,
        IWarehouseService warehouseService,
        INewStockService stockService,
        IMovementService movementService,
        IStockThresholdService thresholdService,
        ISapClient sap,
        IConfiguration configuration,
        IOperationLogService opLog,
        ILogger<DashboardService> logger)
    {
        _productService = productService;
        _warehouseService = warehouseService;
        _stockService = stockService;
        _movementService = movementService;
        _thresholdService = thresholdService;
        _sap = sap;
        _configuration = configuration;
        _opLog = opLog;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        // Aynı scoped DbContext üzerinde paralel EF sorguları çalıştırılamaz.
        var products = await _productService.GetAllAsync(ct);
        var warehouses = await _warehouseService.GetAllAsync(ct);
        var stocks = await _stockService.GetStocksAsync(ct: ct);
        var movements = await _movementService.GetMovementsAsync(new MovementFilterRequest { Page = 1, PageSize = 8 }, ct);

        var warehouseByCode = warehouses.ToDictionary(w => w.Code, StringComparer.OrdinalIgnoreCase);
        var productByCode = products.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

        var warehouseTotals = stocks
            .GroupBy(s => s.WarehouseId)
            .Select(g => new DistributionItemDto
            {
                Code = g.Key,
                Name = warehouseByCode.TryGetValue(g.Key, out var wh) ? wh.Name : g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(6)
            .ToList();

        var stockByMaterial = stocks
            .GroupBy(s => s.MaterialNo)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity), StringComparer.OrdinalIgnoreCase);

        var categoryTotals = products
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Category) ? "Genel" : p.Category!)
            .Select(g =>
            {
                var qty = g.Sum(p => stockByMaterial.GetValueOrDefault(p.Code, 0));
                return new DistributionItemDto { Code = g.Key, Name = g.Key, Quantity = qty };
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToList();

        var sapStatus = await ResolveSapStatusAsync(ct);
        var lowStockCount = await _thresholdService.GetLowStockCountAsync(ct);

        return new DashboardSummaryDto
        {
            SapStatus = sapStatus,
            ProductCount = products.Count,
            WarehouseCount = warehouses.Count,
            TotalStockQuantity = stocks.Sum(s => s.Quantity),
            EmptyStockLines = stocks.Count(s => s.Quantity <= 0),
            SapOnlyProductCount = products.Count(p => p.ProductId == 0),
            LowStockCount = lowStockCount,
            RecentMovements = movements.Items,
            WarehouseStockDistribution = warehouseTotals,
            CategoryStockDistribution = categoryTotals
        };
    }

    private async Task<string> ResolveSapStatusAsync(CancellationToken ct)
    {
        var provider = SapClientConfiguration.GetProvider(_configuration);
        if (provider == SapClientProvider.Mock)
            return "healthy";

        try
        {
            await _sap.GetStockListAsync(ct: ct);
            return "healthy";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SAP health check failed during dashboard summary.");
            await _opLog.LogAsync(
                null,
                "SapUnhealthy",
                "SAP",
                false,
                details: ex.Message,
                errorMessage: "SAP bağlantı kontrolü başarısız.",
                source: EventLogSource.System,
                severity: EventLogSeverity.Warning,
                ct: ct);
            return "unhealthy";
        }
    }
}
