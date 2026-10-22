using DiemLanh.Domain.Enums;

namespace DiemLanh.Application.DTOs.Product;

public class ProductCatalogFilterDto
{
    public string? Keyword { get; set; }
    public string? CategorySlug { get; set; }
    public List<string> BrandSlugs { get; set; } = new();
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? CapacityBtu { get; set; }
    public bool? IsInverter { get; set; }
    public string? RoomAreaCode { get; set; } // UNDER_15, FROM_15_TO_20, FROM_20_TO_30, OVER_30
    public string? GasType { get; set; } // R32, R410A, R22
    public double? MinRating { get; set; }
    public int? StoreId { get; set; }
    public bool InStockOnly { get; set; } = false;
    public string SortBy { get; set; } = "featured"; // featured, price_asc, price_desc, new, best_seller, rating
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class ProductCardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? MainImageUrl { get; set; }
    public string? HoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int DiscountPercent => SalePrice.HasValue && Price > 0 && SalePrice < Price
        ? (int)Math.Round((1 - SalePrice.Value / Price) * 100)
        : 0;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int Stock { get; set; }
    public ProductStatus Status { get; set; }
    public bool IsInverter { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public int SoldCount { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreProvince { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDetailViewDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreSlug { get; set; } = string.Empty;
    public string StoreLogoUrl { get; set; } = string.Empty;
    public double StoreRating { get; set; }
    public int StoreReviewCount { get; set; }
    public string StoreProvince { get; set; } = string.Empty;
    public string StorePhone { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public int? BrandId { get; set; }
    public string? Brand { get; set; }
    public string? BrandLogoUrl { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? SubTitle { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public List<string> Highlights { get; set; } = new();
    public List<string> Pros { get; set; } = new();
    public List<string> Cons { get; set; } = new();

    public string Model { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public int DiscountPercent => SalePrice.HasValue && Price > 0 && SalePrice < Price
        ? (int)Math.Round((1 - SalePrice.Value / Price) * 100)
        : 0;
    public int Stock { get; set; }
    public int WarrantyMonths { get; set; }
    public string? MainImageUrl { get; set; }
    public List<ProductImageDto> GalleryImages { get; set; } = new();
    public List<ProductVideoDto> Videos { get; set; } = new();

    // Grouped specs: CategoryName -> List of (Key, Value)
    public Dictionary<string, List<KeyValuePair<string, string>>> SpecificationsGrouped { get; set; } = new();

    public bool IsInverter { get; set; }
    public int? CapacityBtu { get; set; }
    public string? RoomAreaCode { get; set; }
    public string? GasType { get; set; }

    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new(); // 5: 20, 4: 5, etc.
    public List<ProductReviewDto> RecentReviews { get; set; } = new();

    public List<ProductPromotionDto> ActivePromotions { get; set; } = new();

    // Store Policies
    public string? WarrantyPolicy { get; set; }
    public string? ReturnPolicy { get; set; }

    // On-page AI Chatbot Context Payload
    public ProductAiContextDto AiContext { get; set; } = new();
}

public class ProductCompareDto
{
    public List<ProductCardDto> Products { get; set; } = new();
    public List<string> SpecKeys { get; set; } = new();
    public Dictionary<string, Dictionary<int, string>> SpecMatrix { get; set; } = new(); // SpecKey -> (ProductId -> SpecValue)
}

public class ProductReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAvatarUrl { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? StoreResponseNote { get; set; }
    public DateTime? StoreResponseAt { get; set; }
}

public class ProductReviewCreateDto
{
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}

public class ProductPromotionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? GiftItemName { get; set; }
    public bool FreeInstallation { get; set; }
}

public class ProductVideoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string VideoType { get; set; } = string.Empty;
}

public class ProductAutocompleteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductAiContextDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public int WarrantyMonths { get; set; }
    public bool IsInverter { get; set; }
    public int? CapacityBtu { get; set; }
    public string RoomAreaRecommended { get; set; } = string.Empty;
    public string KeyFeaturesSummary { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public List<string> SuggestedQuestions { get; set; } = new();
}
