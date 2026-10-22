using DiemLanh.Domain.Enums;

namespace DiemLanh.Application.DTOs.Store;

public class StoreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? FanpageUrl { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public StoreStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        StoreStatus.Active => "Đang hoạt động",
        StoreStatus.Inactive => "Tạm ngưng",
        StoreStatus.Pending => "Chờ phê duyệt",
        StoreStatus.Suspended => "Bị đình chỉ",
        StoreStatus.Rejected => "Bị từ chối",
        _ => "Không xác định"
    };
    public DateTime JoinedAt { get; set; }
    public double? AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? WarrantyPolicy { get; set; }
    public string? ReturnPolicy { get; set; }
    public StoreAddressDto? Address { get; set; }
    public List<StoreServiceAreaDto> ServiceAreas { get; set; } = new();
    public List<StoreBrandDto> Brands { get; set; } = new();
}

public class StoreListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public StoreStatus Status { get; set; }
    public double? AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public List<string> ServiceProvinces { get; set; } = new();
    public List<string> BrandNames { get; set; } = new();
}

public class StoreCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? FanpageUrl { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public TimeOnly OpenTime { get; set; } = new TimeOnly(8, 0);
    public TimeOnly CloseTime { get; set; } = new TimeOnly(21, 0);
    public string OwnerId { get; set; } = string.Empty;
    // Address
    public string Street { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class StoreUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BusinessDescription { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? FanpageUrl { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string? WarrantyPolicy { get; set; }
    public string? ReturnPolicy { get; set; }
    // Address
    public string Street { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class StoreAddressDto
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string FullAddress => $"{Street}, {Ward}, {District}, {Province}".Replace(", ,", ",");
}

public class StoreServiceAreaDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? Ward { get; set; }
    public double? CenterLatitude { get; set; }
    public double? CenterLongitude { get; set; }
    public double? RadiusKm { get; set; }
    public bool IsActive { get; set; }
}

public class StoreServiceAreaCreateDto
{
    public int StoreId { get; set; }
    public string Province { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? Ward { get; set; }
    public double? CenterLatitude { get; set; }
    public double? CenterLongitude { get; set; }
    public double? RadiusKm { get; set; }
}

public class StoreBrandDto
{
    public int Id { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
}

public class StoreBrandCreateDto
{
    public int StoreId { get; set; }
    public string BrandName { get; set; } = string.Empty;
}

public class StoreFilterDto
{
    public string? Keyword { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Brand { get; set; }
    public StoreStatus? Status { get; set; }
    public double? MinRating { get; set; }
    public string SortBy { get; set; } = "name";
    public bool SortDesc { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
