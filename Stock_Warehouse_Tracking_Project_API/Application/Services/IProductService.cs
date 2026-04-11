using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Product;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
