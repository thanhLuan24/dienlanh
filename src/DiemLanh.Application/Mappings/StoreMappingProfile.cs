using AutoMapper;
using DiemLanh.Application.DTOs.Dashboard;
using DiemLanh.Application.DTOs.Order;
using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.DTOs.Repair;
using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Entities.Store;
using DiemLanh.Domain.Entities.Product;
using DiemLanh.Domain.Enums;
using System.Text.Json;

namespace DiemLanh.Application.Mappings;

public class StoreMappingProfile : Profile
{
    public StoreMappingProfile()
    {
        // Store
        CreateMap<Store, StoreDto>()
            .ForMember(d => d.Address, o => o.MapFrom(s => s.Address))
            .ForMember(d => d.ServiceAreas, o => o.MapFrom(s => s.ServiceAreas))
            .ForMember(d => d.Brands, o => o.MapFrom(s => s.Brands));

        CreateMap<Store, StoreListDto>()
            .ForMember(d => d.Province, o => o.MapFrom(s => s.Address != null ? s.Address.Province : null))
            .ForMember(d => d.District, o => o.MapFrom(s => s.Address != null ? s.Address.District : null))
            .ForMember(d => d.ServiceProvinces, o => o.MapFrom(s => s.ServiceAreas.Select(a => a.Province).Distinct().ToList()))
            .ForMember(d => d.BrandNames, o => o.MapFrom(s => s.Brands.Select(b => b.BrandName).ToList()));

        CreateMap<StoreCreateDto, Store>()
            .ForMember(d => d.Status, o => o.MapFrom(_ => StoreStatus.Pending))
            .ForMember(d => d.JoinedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.Address, o => o.MapFrom(s => new StoreAddress
            {
                Street = s.Street,
                Ward = s.Ward,
                District = s.District,
                Province = s.Province,
                Latitude = s.Latitude,
                Longitude = s.Longitude
            }));

        CreateMap<StoreUpdateDto, Store>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.JoinedAt, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.OwnerId, o => o.Ignore());

        CreateMap<StoreAddress, StoreAddressDto>();
        CreateMap<StoreServiceArea, StoreServiceAreaDto>();
        CreateMap<StoreServiceAreaCreateDto, StoreServiceArea>();
        CreateMap<StoreBrand, StoreBrandDto>();
        CreateMap<StoreBrandCreateDto, StoreBrand>();

        // Order
        CreateMap<StoreOrder, OrderDto>()
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store.Name))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.Installation, o => o.MapFrom(s => s.Installation));

        CreateMap<StoreOrder, OrderListDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count))
            .ForMember(d => d.StatusDisplay, o => o.MapFrom(s => GetOrderStatusDisplay(s.Status)));

        CreateMap<StoreOrderItem, OrderItemDto>()
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.UnitPrice * s.Quantity));

        // Installation
        CreateMap<StoreInstallation, InstallationDto>()
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store != null ? s.Store.Name : ""))
            .ForMember(d => d.TechnicianName, o => o.MapFrom(s => s.Technician != null ? s.Technician.Name : null))
            .ForMember(d => d.OrderCode, o => o.MapFrom(s => s.Order.OrderCode))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Order.CustomerName))
            .ForMember(d => d.CustomerPhone, o => o.MapFrom(s => s.Order.CustomerPhone));

        CreateMap<StoreInstallation, UpcomingInstallationDto>()
            .ForMember(d => d.OrderCode, o => o.MapFrom(s => s.Order.OrderCode))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Order.CustomerName))
            .ForMember(d => d.CustomerPhone, o => o.MapFrom(s => s.Order.CustomerPhone))
            .ForMember(d => d.TechnicianName, o => o.MapFrom(s => s.Technician != null ? s.Technician.Name : null));

        // Repair
        CreateMap<StoreRepairRequest, RepairRequestDto>()
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store.Name))
            .ForMember(d => d.TechnicianName, o => o.MapFrom(s => s.Technician != null ? s.Technician.Name : null))
            .ForMember(d => d.ImageUrls, o => o.MapFrom(s => DeserializeStringList(s.ImageUrlsJson)));

        CreateMap<RepairRequestCreateDto, StoreRepairRequest>()
            .ForMember(d => d.Status, o => o.MapFrom(_ => RepairStatus.PendingReceive))
            .ForMember(d => d.ImageUrlsJson, o => o.Ignore());

        // Technician
        CreateMap<StoreTechnician, TechnicianDto>();
        CreateMap<TechnicianCreateDto, StoreTechnician>();
        CreateMap<TechnicianUpdateDto, StoreTechnician>()
            .ForMember(d => d.Id, o => o.Ignore());

        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.StoreName, o => o.MapFrom(s => s.Store.Name))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images))
            .ForMember(d => d.Specifications, o => o.MapFrom(s => DeserializeStringDict(s.SpecificationsJson)));

        CreateMap<Product, ProductListDto>();
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductCategory, ProductCategoryDto>()
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count(p => !p.IsDeleted && !p.IsHidden)));

        CreateMap<ProductCreateDto, Product>()
            .ForMember(d => d.SpecificationsJson, o => o.MapFrom(s => SerializeDict(s.Specifications)))
            .ForMember(d => d.Status, o => o.MapFrom(_ => ProductStatus.Active));

        CreateMap<ProductUpdateDto, Product>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.SpecificationsJson, o => o.MapFrom(s => SerializeDict(s.Specifications)));
    }

    // Static helpers to avoid CS0854 in expression trees
    private static List<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();
        return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
    }

    private static Dictionary<string, string> DeserializeStringDict(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new Dictionary<string, string>();
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
    }

    private static string SerializeDict(Dictionary<string, string> dict)
        => JsonSerializer.Serialize(dict);

    private static string GetOrderStatusDisplay(OrderStatus status) => status switch
    {
        OrderStatus.PendingConfirmation => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Preparing => "Đang chuẩn bị",
        OrderStatus.Shipping => "Đang giao",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.Installed => "Đã lắp đặt",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        OrderStatus.Rejected => "Bị từ chối",
        _ => "Không xác định"
    };
}
