using DiemLanh.Domain.Common;
using DiemLanh.Domain.Enums;

namespace DiemLanh.Domain.Entities.Product;

public class ProductCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? IconClass { get; set; } // Bootstrap icon class
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ProductAttribute> CategoryAttributes { get; set; } = new List<ProductAttribute>();
}

public class Product : AuditableEntity
{
    public int StoreId { get; set; }
    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? HighlightsJson { get; set; } // JSON list of bullet highlights
    public string? ProsConsJson { get; set; } // JSON key-value of Pros and Cons
    public string? Brand { get; set; } // Legacy or text fallback
    public string? Model { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int Stock { get; set; } = 0;
    public int WarrantyMonths { get; set; } = 12;
    public string? MainImageUrl { get; set; }
    public string? SpecificationsJson { get; set; } // JSON key-value pairs grouped by section

    // Faceted filtering quick fields
    public bool IsInverter { get; set; } = false;
    public int? CapacityBtu { get; set; } // 9000, 12000, 18000, 24000
    public string? RoomAreaCode { get; set; } // UNDER_15, FROM_15_TO_20, FROM_20_TO_30, OVER_30
    public string? GasType { get; set; } // R32, R410A, R22

    // Performance & Ratings
    public double AverageRating { get; set; } = 5.0;
    public int ReviewCount { get; set; } = 0;
    public string? RatingDistributionJson { get; set; } // JSON: {"1":0,"2":0,"3":1,"4":5,"5":20}

    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public bool IsHidden { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public int SoldCount { get; set; } = 0;

    // Navigation
    public Store.Store Store { get; set; } = null!;
    public ProductCategory Category { get; set; } = null!;
    public ProductBrand? BrandEntity { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public ICollection<ProductPromotion> Promotions { get; set; } = new List<ProductPromotion>();
    public ICollection<ProductVideo> Videos { get; set; } = new List<ProductVideo>();
}

public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsMain { get; set; } = false;

    // Navigation
    public Product Product { get; set; } = null!;
}
