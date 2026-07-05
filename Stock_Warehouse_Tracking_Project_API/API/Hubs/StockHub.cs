using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Stock_Warehouse_Tracking_Project_API.API.Hubs;

[Authorize]
public class StockHub : Hub
{
    public async Task SubscribeStockUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "stock-updates");
    }
}

public interface IStockNotificationService
{
    Task NotifyStockUpdatedAsync(string materialNo, string warehouseId, decimal quantity);
}

public class StockNotificationService : IStockNotificationService
{
    private readonly IHubContext<StockHub> _hub;

    public StockNotificationService(IHubContext<StockHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyStockUpdatedAsync(string materialNo, string warehouseId, decimal quantity)
    {
        return _hub.Clients.Group("stock-updates").SendAsync("StockUpdated", new
        {
            materialNo,
            warehouseId,
            quantity,
            updatedAt = DateTime.UtcNow
        });
    }
}
