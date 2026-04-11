using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAllAsync(CancellationToken ct = default);
    Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default);
    Task<WarehouseDto> UpdateAsync(int id, UpdateWarehouseRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
