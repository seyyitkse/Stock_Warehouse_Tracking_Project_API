using Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Tests;

public class MockSapClientTests
{
    [Fact]
    public async Task GetStockListAsync_ReturnsSeededRows()
    {
        var client = new MockSapClient();
        var rows = await client.GetStockListAsync();
        Assert.NotEmpty(rows);
    }

    [Fact]
    public async Task StockInAsync_IncreasesQuantity()
    {
        var client = new MockSapClient();
        var before = await client.GetStockDetailAsync("MAT-1001", "WH-01");
        Assert.NotNull(before);
        var expected = before!.Quantity + 5;

        await client.StockInAsync(new Models.Sap.SapStockInRequest
        {
            Matnr = "MAT-1001",
            WhId = "WH-01",
            Quantity = 5
        });

        var after = await client.GetStockDetailAsync("MAT-1001", "WH-01");
        Assert.Equal(expected, after!.Quantity);
    }
}
