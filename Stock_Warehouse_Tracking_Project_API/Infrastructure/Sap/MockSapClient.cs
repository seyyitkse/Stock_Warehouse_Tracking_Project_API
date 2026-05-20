using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

public class MockSapClient : ISapClient, IProductCatalogSapClient
{
    private readonly List<SapProductRow> _products =
    [
        new SapProductRow { Matnr = "MAT-1001", Name = "Mock Ürün 1001", Unit = "ADT", CreatedAt = DateTime.UtcNow.AddDays(-10) },
        new SapProductRow { Matnr = "MAT-1002", Name = "Mock Ürün 1002", Unit = "ADT", CreatedAt = DateTime.UtcNow.AddDays(-8) },
        new SapProductRow { Matnr = "MAT-1003", Name = "Mock Ürün 1003", Unit = "KG", CreatedAt = DateTime.UtcNow.AddDays(-6) }
    ];

    private readonly Dictionary<string, SapStockRow> _stock = new()
    {
        ["MAT-1001|WH-01"] = new SapStockRow { Matnr = "MAT-1001", WhId = "WH-01", Quantity = 120, UpdatedAt = DateTime.UtcNow.AddMinutes(-15) },
        ["MAT-1002|WH-01"] = new SapStockRow { Matnr = "MAT-1002", WhId = "WH-01", Quantity = 45,  UpdatedAt = DateTime.UtcNow.AddMinutes(-8) },
        ["MAT-1003|WH-02"] = new SapStockRow { Matnr = "MAT-1003", WhId = "WH-02", Quantity = 200, UpdatedAt = DateTime.UtcNow.AddMinutes(-2) },
    };

    public Task<IReadOnlyList<SapStockRow>> GetStockListAsync(string? matnr = null, string? whId = null, CancellationToken ct = default)
    {
        var rows = _stock.Values.AsEnumerable();
        if (matnr is not null) rows = rows.Where(r => r.Matnr == matnr);
        if (whId  is not null) rows = rows.Where(r => r.WhId  == whId);
        return Task.FromResult<IReadOnlyList<SapStockRow>>(rows.ToList());
    }

    public Task<IReadOnlyList<SapProductRow>> GetProductListAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<SapProductRow>>(_products.ToList());
    }

    public Task<SapStockRow?> GetStockDetailAsync(string matnr, string whId, CancellationToken ct = default)
    {
        _stock.TryGetValue($"{matnr}|{whId}", out var row);
        return Task.FromResult(row);
    }

    private static string NewDocNo(string prefix) => $"{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

    public Task<SapCreateProductResult> CreateProductAsync(SapCreateProductRequest request, CancellationToken ct = default)
    {
        return Task.FromResult(new SapCreateProductResult
        {
            Success = true,
            SapDocNo = NewDocNo("SAP-MAT")
        });
    }

    public Task<SapMovementResult> StockInAsync(SapStockInRequest request, CancellationToken ct = default)
    {
        var key = $"{request.Matnr}|{request.WhId}";
        if (_stock.TryGetValue(key, out var row))
            row.Quantity += request.Quantity;
        else
            _stock[key] = new SapStockRow { Matnr = request.Matnr, WhId = request.WhId, Quantity = request.Quantity, UpdatedAt = DateTime.UtcNow };

        return Task.FromResult(new SapMovementResult { Success = true, SapDocNo = NewDocNo("SAP-IN") });
    }

    public Task<SapMovementResult> StockOutAsync(SapStockOutRequest request, CancellationToken ct = default)
    {
        var key = $"{request.Matnr}|{request.WhId}";
        if (!_stock.TryGetValue(key, out var row) || row.Quantity < request.Quantity)
            return Task.FromResult(new SapMovementResult { Success = false, ErrorMessage = "Yetersiz stok." });

        row.Quantity -= request.Quantity;
        row.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(new SapMovementResult { Success = true, SapDocNo = NewDocNo("SAP-OUT") });
    }

    public Task<SapMovementResult> TransferStockAsync(SapTransferRequest request, CancellationToken ct = default)
    {
        var srcKey  = $"{request.Matnr}|{request.SourceWhId}";
        var destKey = $"{request.Matnr}|{request.DestWhId}";

        if (!_stock.TryGetValue(srcKey, out var src) || src.Quantity < request.Quantity)
            return Task.FromResult(new SapMovementResult { Success = false, ErrorMessage = "Kaynak depoda yetersiz stok." });

        src.Quantity -= request.Quantity;
        src.UpdatedAt = DateTime.UtcNow;

        if (_stock.TryGetValue(destKey, out var dest))
        {
            dest.Quantity += request.Quantity;
            dest.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _stock[destKey] = new SapStockRow { Matnr = request.Matnr, WhId = request.DestWhId, Quantity = request.Quantity, UpdatedAt = DateTime.UtcNow };
        }

        return Task.FromResult(new SapMovementResult { Success = true, SapDocNo = NewDocNo("SAP-TR") });
    }
}
