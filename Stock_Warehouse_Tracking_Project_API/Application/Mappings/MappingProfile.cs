using AutoMapper;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Product;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Warehouse;
using Stock_Warehouse_Tracking_Project_API.Domain.Entities;

namespace Stock_Warehouse_Tracking_Project_API.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<Warehouse, WarehouseDto>();
    }
}
