using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.Common;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Movement;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class MovementService : IMovementService
{
    private readonly AppDbContext _db;
    private readonly ILogger<MovementService> _logger;

    public MovementService(AppDbContext db, ILogger<MovementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<MovementDto>> GetMovementsAsync(MovementFilterRequest filter, CancellationToken ct = default)
    {
        var query = _db.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.SourceWarehouse)
            .Include(m => m.DestWarehouse)
            .Include(m => m.User)
            .AsQueryable();

        if (filter.ProductId.HasValue)
            query = query.Where(m => m.ProductId == filter.ProductId.Value);
        if (filter.WarehouseId.HasValue)
            query = query.Where(m => m.SourceWarehouseId == filter.WarehouseId.Value || m.DestWarehouseId == filter.WarehouseId.Value);
        if (filter.Type.HasValue)
            query = query.Where(m => m.Type == filter.Type.Value);
        if (filter.DateFrom.HasValue)
            query = query.Where(m => m.Date >= filter.DateFrom.Value);
        if (filter.DateTo.HasValue)
            query = query.Where(m => m.Date <= filter.DateTo.Value);
        if (filter.UserId.HasValue)
            query = query.Where(m => m.UserId == filter.UserId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.Date)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(m => new MovementDto
            {
                MovementId = m.MovementId,
                Type = m.Type,
                Quantity = m.Quantity,
                Date = m.Date,
                RefNo = m.RefNo,
                ProductCode = m.Product.Code,
                SourceWarehouseCode = m.SourceWarehouse != null ? m.SourceWarehouse.Code : null,
                DestWarehouseCode = m.DestWarehouse != null ? m.DestWarehouse.Code : null,
                UserName = m.User.Name
            })
            .ToListAsync(ct);

        _logger.LogInformation("Hareket sorgulama: TotalCount={Total}, Page={Page}", totalCount, filter.Page);

        return new PagedResult<MovementDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}
