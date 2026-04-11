using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _db;
    private readonly IOperationLogService _opLog;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        AppDbContext db,
        IOperationLogService opLog,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<WarehouseService> logger)
    {
        _db = db; _opLog = opLog; _currentUser = currentUser; _mapper = mapper; _logger = logger;
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetAllAsync(CancellationToken ct = default)
    {
        var warehouses = await _db.Warehouses.AsNoTracking().ToListAsync(ct);
        return _mapper.Map<List<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var warehouse = await _db.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.WarehouseId == id, ct);
        return warehouse is null ? null : _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Depo oluşturuluyor: {Code}", request.Code);

        var codeExists = await _db.Warehouses.AnyAsync(w => w.Code == request.Code, ct);
        if (codeExists)
            throw new InvalidOperationException($"'{request.Code}' kodlu depo zaten mevcut.");

        var warehouse = new Warehouse
        {
            Code = request.Code,
            Name = request.Name,
            Location = request.Location,
            CreatedAt = DateTime.UtcNow
        };
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Depo oluşturuldu: Id={Id}, Code={Code}", warehouse.WarehouseId, warehouse.Code);
        await _opLog.LogAsync(_currentUser.UserId, "CreateWarehouse", "Warehouse", true,
            $"Id={warehouse.WarehouseId}, Code={warehouse.Code}", ct: ct);

        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(int id, UpdateWarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await _db.Warehouses.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"WarehouseId={id} bulunamadı.");

        warehouse.Name = request.Name;
        warehouse.Location = request.Location;
        warehouse.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Depo güncellendi: Id={Id}", id);
        await _opLog.LogAsync(_currentUser.UserId, "UpdateWarehouse", "Warehouse", true, $"Id={id}", ct: ct);

        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var warehouse = await _db.Warehouses.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"WarehouseId={id} bulunamadı.");

        warehouse.IsDeleted = true;
        warehouse.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Depo silindi (soft): Id={Id}", id);
        await _opLog.LogAsync(_currentUser.UserId, "DeleteWarehouse", "Warehouse", true, $"Id={id}", ct: ct);
    }
}
