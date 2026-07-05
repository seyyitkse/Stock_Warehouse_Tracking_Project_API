using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Stock;
using Stock_Warehouse_Tracking_Project_API.Application.Services;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Tests;

public class NewStockServiceTests
{
    [Fact]
    public async Task StockInAsync_CreatesMovementWhenProductMissingLocally()
    {
        await using var db = TestDbFactory.Create(nameof(StockInAsync_CreatesMovementWhenProductMissingLocally));
        var sap = new MockSapClient();
        var opLog = new Mock<IOperationLogService>();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(1);

        var service = new NewStockService(
            db, sap, opLog.Object, currentUser.Object, NullLogger<NewStockService>.Instance);

        await service.StockInAsync(new StockInRequest
        {
            MaterialNo = "MAT-1001",
            WarehouseId = "WH-01",
            Quantity = 1
        });

        Assert.Single(db.StockMovements);
        Assert.Single(db.Products);
        Assert.Single(db.Warehouses);
    }
}
