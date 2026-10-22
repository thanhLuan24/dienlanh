using DiemLanh.Domain.Enums;

namespace DiemLanh.Application.DTOs.Product;

public class ProductDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal DisplayPrice => SalePrice.HasValue && SalePrice < Price ? SalePrice.Value : Price;
    public bool IsOnSale => SalePrice.HasValue && SalePrice < Price;
    public int Stock { get; set; }
    public int WarrantyMonths { get; set; }
    public string? MainImageUrl { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
    public ProductStatus Status { get; set; }
    public bool IsHidden { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public int SoldCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? MainImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Stock { get; set; }
    public ProductStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        ProductStatus.Active => "Đang bán",
        ProductStatus.Hidden => "Đang ẩn",
        ProductStatus.OutOfStock => "Hết hàng",
        ProductStatus.Discontinued => "Ngừng kinh doanh",
        _ => "Không xác định"
    };
    public bool IsHidden { get; set; }
    public bool IsFeatured { get; set; }
    public string? CategoryName { get; set; }
    public int SoldCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductCreateDto
{
    public int StoreId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Stock { get; set; } = 0;
    public int WarrantyMonths { get; set; } = 12;
    public bool IsFeatured { get; set; } = false;
    public bool IsHidden { get; set; } = false;
    public Dictionary<string, string> Specifications { get; set; } = new();
}

public class ProductUpdateDto : ProductCreateDto
{
    public int Id { get; set; }
    public new bool IsHidden { get; set; } = false;
    public ProductStatus Status { get; set; } = ProductStatus.Active;
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsMain { get; set; }
}

public class ProductCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? IconClass { get; set; }
    public int ProductCount { get; set; }
}

public class ProductFilterDto
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public string? Brand { get; set; }
    public ProductStatus? Status { get; set; }
    public bool? IsHidden { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public bool SortDesc { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
