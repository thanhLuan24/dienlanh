namespace DiemLanh.Application.DTOs.Admin;

// ─── Overview KPIs ───────────────────────────────────────────────────────────
public class AdminOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalStores { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int NewUsersToday { get; set; }
    public int NewOrdersToday { get; set; }
    public int PendingStores { get; set; }
    public List<AdminTopProductDto> TopViewedProducts { get; set; } = new();
    public List<AdminTopProductDto> TopSoldProducts { get; set; } = new();
    public List<AdminRecentUserDto> RecentUsers { get; set; } = new();
}

public class AdminTopProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Brand { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    public decimal Price { get; set; }
}

public class AdminRecentUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

// ─── Product Management ───────────────────────────────────────────────────────
public class AdminProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Stock { get; set; }
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    public double AverageRating { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsHidden { get; set; }
    public string? MainImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminProductCreateEditDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? Brand { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Stock { get; set; }
    public int WarrantyMonths { get; set; } = 12;
    public string? MainImageUrl { get; set; }
    public bool IsInverter { get; set; }
    public int? CapacityBtu { get; set; }
    public string? RoomAreaCode { get; set; }
    public string? GasType { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsHidden { get; set; }
}

// ─── Store Management ─────────────────────────────────────────────────────────
public class AdminStoreListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Province { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string? OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public double? AverageRating { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class AdminStoreCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? Province { get; set; }
    public string? District { get; set; }
    public string? Street { get; set; }
    public string? OwnerId { get; set; }
    public string? WarrantyPolicy { get; set; }
    public string? ReturnPolicy { get; set; }
}

// ─── User Management ──────────────────────────────────────────────────────────
public class AdminUserListDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class StoreSimpleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

