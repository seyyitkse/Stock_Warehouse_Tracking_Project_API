using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Product;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;
using Stock_Warehouse_Tracking_Project_API.Models.Sap;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly ISapClient _sap;
    private readonly IOperationLogService _opLog;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        AppDbContext db,
        ISapClient sap,
        IOperationLogService opLog,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _db = db; _sap = sap; _opLog = opLog;
        _currentUser = currentUser; _mapper = mapper; _logger = logger;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        if (_sap is IProductCatalogSapClient sapProductCatalog)
        {
            var sapProducts = await sapProductCatalog.GetProductListAsync(ct);
            return await MapSapProductsAsync(sapProducts, ct);
        }

        var products = await _db.Products.AsNoTracking().ToListAsync(ct);
        return _mapper.Map<List<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id, ct);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Ürün oluşturuluyor: {Code}", request.Code);

        var codeExists = await _db.Products.AnyAsync(p => p.Code == request.Code, ct);
        if (codeExists)
            throw new InvalidOperationException($"'{request.Code}' kodlu ürün zaten mevcut.");

        // SAP'a ürün oluştur
        var sapResult = await _sap.CreateProductAsync(new Models.Sap.SapCreateProductRequest
        {
            Matnr = request.Code,
            Name = request.Name,
            Unit = request.Unit,
            Category = request.Category
        }, ct);

        if (!sapResult.Success)
        {
            _logger.LogWarning("SAP ürün oluşturma başarısız: {Error}", sapResult.ErrorMessage);
            await _opLog.LogAsync(_currentUser.UserId, "CreateProduct", "Product", false,
                $"Code={request.Code}", sapResult.ErrorMessage, ct);
            throw new InvalidOperationException($"SAP hatası: {sapResult.ErrorMessage}");
        }

        var product = new Product
        {
            Code = request.Code,
            Name = request.Name,
            Unit = request.Unit,
            Category = request.Category,
            Barcode = request.Barcode,
            CreatedAt = DateTime.UtcNow
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Ürün oluşturuldu: Id={Id}, Code={Code}, SapDoc={Doc}", product.ProductId, product.Code, sapResult.SapDocNo);
        await _opLog.LogAsync(_currentUser.UserId, "CreateProduct", "Product", true,
            $"Id={product.ProductId}, Code={product.Code}, SapDocNo={sapResult.SapDocNo}", ct: ct);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"ProductId={id} bulunamadı.");

        product.Name = request.Name;
        product.Unit = request.Unit;
        product.Category = request.Category;
        product.Barcode = request.Barcode;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Ürün güncellendi: Id={Id}", id);
        await _opLog.LogAsync(_currentUser.UserId, "UpdateProduct", "Product", true, $"Id={id}", ct: ct);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync(new object[] { id }, ct)
            ?? throw new KeyNotFoundException($"ProductId={id} bulunamadı.");

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Ürün silindi (soft): Id={Id}", id);
        await _opLog.LogAsync(_currentUser.UserId, "DeleteProduct", "Product", true, $"Id={id}", ct: ct);
    }

    private async Task<IReadOnlyList<ProductDto>> MapSapProductsAsync(
        IReadOnlyList<SapProductRow> sapProducts,
        CancellationToken ct)
    {
        if (sapProducts.Count == 0)
            return [];

        var productCodes = sapProducts
            .Select(product => product.Matnr.Trim())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var localProducts = await _db.Products
            .AsNoTracking()
            .Where(product => productCodes.Contains(product.Code))
            .ToListAsync(ct);

        var localByCode = localProducts.ToDictionary(product => product.Code, StringComparer.OrdinalIgnoreCase);

        return sapProducts
            .Where(product => !string.IsNullOrWhiteSpace(product.Matnr))
            .Select(product =>
            {
                localByCode.TryGetValue(product.Matnr.Trim(), out var localProduct);
                return ToDto(product, localProduct);
            })
            .ToList();
    }

    private static ProductDto ToDto(SapProductRow sapProduct, Product? localProduct)
    {
        return new ProductDto
        {
            ProductId = localProduct?.ProductId ?? 0,
            Code = sapProduct.Matnr.Trim(),
            Name = string.IsNullOrWhiteSpace(sapProduct.Name) ? localProduct?.Name ?? string.Empty : sapProduct.Name.Trim(),
            Unit = string.IsNullOrWhiteSpace(sapProduct.Unit) ? localProduct?.Unit ?? string.Empty : sapProduct.Unit.Trim(),
            Category = string.IsNullOrWhiteSpace(sapProduct.Category) ? localProduct?.Category : sapProduct.Category.Trim(),
            Barcode = string.IsNullOrWhiteSpace(sapProduct.Barcode) ? localProduct?.Barcode : sapProduct.Barcode.Trim(),
            MinStock = localProduct?.MinStock ?? 0,
            CreatedAt = sapProduct.CreatedAt == default ? localProduct?.CreatedAt ?? DateTime.UtcNow : sapProduct.CreatedAt
        };
    }
}
