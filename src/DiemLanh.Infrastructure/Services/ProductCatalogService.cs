using System.Text.Json;
using AutoMapper;
using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Entities.Product;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiemLanh.Infrastructure.Services;

public class ProductCatalogService : IProductCatalogService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductCatalogService> _logger;

    public ProductCatalogService(ApplicationDbContext db, IMapper mapper, ILogger<ProductCatalogService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ProductCardDto>> GetCatalogProductsAsync(ProductCatalogFilterDto filter)
    {
        var query = _db.Products
            .Include(x => x.Category)
            .Include(x => x.Store)
            .Include(x => x.Images)
            .Include(x => x.BrandEntity)
            .Where(x => !x.IsHidden && x.Status == ProductStatus.Active);

        // Filter Category
        if (!string.IsNullOrEmpty(filter.CategorySlug))
        {
            query = query.Where(x => x.Category.Slug == filter.CategorySlug);
        }

        // Filter Brands
        if (filter.BrandSlugs != null && filter.BrandSlugs.Any())
        {
            query = query.Where(x => (x.BrandEntity != null && filter.BrandSlugs.Contains(x.BrandEntity.Slug)) 
                                  || (x.Brand != null && filter.BrandSlugs.Contains(x.Brand.ToLower())));
        }

        // Filter Price Range
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(x => (x.SalePrice ?? x.Price) >= filter.MinPrice.Value);
        }
        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(x => (x.SalePrice ?? x.Price) <= filter.MaxPrice.Value);
        }

        // Filter Capacity BTU
        if (filter.CapacityBtu.HasValue)
        {
            query = query.Where(x => x.CapacityBtu == filter.CapacityBtu.Value);
        }

        // Filter Inverter
        if (filter.IsInverter.HasValue)
        {
            query = query.Where(x => x.IsInverter == filter.IsInverter.Value);
        }

        // Filter Room Area Code
        if (!string.IsNullOrEmpty(filter.RoomAreaCode))
        {
            query = query.Where(x => x.RoomAreaCode == filter.RoomAreaCode);
        }

        // Filter Gas Type
        if (!string.IsNullOrEmpty(filter.GasType))
        {
            query = query.Where(x => x.GasType == filter.GasType);
        }

        // Filter Rating
        if (filter.MinRating.HasValue)
        {
            query = query.Where(x => x.AverageRating >= filter.MinRating.Value);
        }

        // Filter Store
        if (filter.StoreId.HasValue)
        {
            query = query.Where(x => x.StoreId == filter.StoreId.Value);
        }

        // Filter In-Stock
        if (filter.InStockOnly)
        {
            query = query.Where(x => x.Stock > 0);
        }

        // Keyword Search
        if (!string.IsNullOrEmpty(filter.Keyword))
        {
            var kw = filter.Keyword.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(kw) 
                                  || (x.Model != null && x.Model.ToLower().Contains(kw)) 
                                  || (x.Brand != null && x.Brand.ToLower().Contains(kw))
                                  || x.Sku.ToLower().Contains(kw));
        }

        // Sorting
        query = filter.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(x => x.SalePrice ?? x.Price),
            "price_desc" => query.OrderByDescending(x => x.SalePrice ?? x.Price),
            "new" => query.OrderByDescending(x => x.CreatedAt),
            "best_seller" => query.OrderByDescending(x => x.SoldCount),
            "rating" => query.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.ReviewCount),
            _ => query.OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.SoldCount).ThenByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ProductCardDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Sku = x.Sku,
                Brand = x.BrandEntity != null ? x.BrandEntity.Name : x.Brand,
                Model = x.Model,
                MainImageUrl = x.MainImageUrl ?? (x.Images.FirstOrDefault(i => i.IsMain) != null ? x.Images.FirstOrDefault(i => i.IsMain)!.ImageUrl : null),
                HoverImageUrl = x.Images.Where(i => !i.IsMain).OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                Price = x.Price,
                SalePrice = x.SalePrice,
                AverageRating = x.AverageRating,
                ReviewCount = x.ReviewCount,
                Stock = x.Stock,
                Status = x.Status,
                IsInverter = x.IsInverter,
                IsFeatured = x.IsFeatured,
                IsNew = (DateTime.UtcNow - x.CreatedAt).TotalDays <= 30,
                SoldCount = x.SoldCount,
                StoreId = x.StoreId,
                StoreName = x.Store.Name,
                CategoryName = x.Category.Name
            })
            .ToListAsync();

        return PagedResult<ProductCardDto>.Create(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<ProductDetailViewDto?> GetProductDetailBySlugAsync(string slug)
    {
        var product = await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Store)
                .ThenInclude(s => s.Address)
            .Include(x => x.BrandEntity)
            .Include(x => x.Images)
            .Include(x => x.Videos)
            .Include(x => x.Promotions)
            .FirstOrDefaultAsync(x => x.Slug == slug && !x.IsHidden);

        if (product == null) return null;

        // Increment view count
        product.ViewCount++;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();

        return await BuildProductDetailViewDtoAsync(product);
    }

    public async Task<ProductDetailViewDto?> GetProductDetailByIdAsync(int id)
    {
        var product = await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Store)
                .ThenInclude(s => s.Address)
            .Include(x => x.BrandEntity)
            .Include(x => x.Images)
            .Include(x => x.Videos)
            .Include(x => x.Promotions)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsHidden);

        if (product == null) return null;
        return await BuildProductDetailViewDtoAsync(product);
    }

    public async Task<ProductCardDto?> GetQuickViewAsync(int id)
    {
        var p = await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Store)
            .Include(x => x.Images)
            .Include(x => x.BrandEntity)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new ProductCardDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Sku = p.Sku,
            Brand = p.BrandEntity != null ? p.BrandEntity.Name : p.Brand,
            Model = p.Model,
            MainImageUrl = p.MainImageUrl,
            Price = p.Price,
            SalePrice = p.SalePrice,
            AverageRating = p.AverageRating,
            ReviewCount = p.ReviewCount,
            Stock = p.Stock,
            Status = p.Status,
            IsInverter = p.IsInverter,
            IsFeatured = p.IsFeatured,
            SoldCount = p.SoldCount,
            StoreId = p.StoreId,
            StoreName = p.Store.Name,
            CategoryName = p.Category.Name
        };
    }

    public async Task<List<ProductAutocompleteDto>> AutocompleteSearchAsync(string query, int maxResults = 8)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<ProductAutocompleteDto>();
        var kw = query.Trim().ToLower();

        return await _db.Products
            .Include(x => x.Category)
            .Include(x => x.BrandEntity)
            .Where(x => !x.IsHidden && x.Status == ProductStatus.Active 
                     && (x.Name.ToLower().Contains(kw) 
                      || (x.Model != null && x.Model.ToLower().Contains(kw))
                      || (x.Brand != null && x.Brand.ToLower().Contains(kw))
                      || x.Sku.ToLower().Contains(kw)))
            .OrderByDescending(x => x.SoldCount)
            .Take(maxResults)
            .Select(x => new ProductAutocompleteDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Brand = x.BrandEntity != null ? x.BrandEntity.Name : x.Brand,
                ImageUrl = x.MainImageUrl,
                Price = x.Price,
                SalePrice = x.SalePrice,
                CategoryName = x.Category.Name
            })
            .ToListAsync();
    }

    public async Task<ProductCompareDto> CompareProductsAsync(List<int> productIds)
    {
        if (productIds == null || !productIds.Any()) return new ProductCompareDto();

        var ids = productIds.Take(4).ToList();
        var products = await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Store)
            .Include(x => x.BrandEntity)
            .Include(x => x.Images)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        var cardList = products.Select(x => new ProductCardDto
        {
            Id = x.Id,
            Name = x.Name,
            Slug = x.Slug,
            Sku = x.Sku,
            Brand = x.BrandEntity != null ? x.BrandEntity.Name : x.Brand,
            Model = x.Model,
            MainImageUrl = x.MainImageUrl,
            Price = x.Price,
            SalePrice = x.SalePrice,
            AverageRating = x.AverageRating,
            ReviewCount = x.ReviewCount,
            Stock = x.Stock,
            Status = x.Status,
            IsInverter = x.IsInverter,
            IsFeatured = x.IsFeatured,
            SoldCount = x.SoldCount,
            StoreId = x.StoreId,
            StoreName = x.Store.Name,
            CategoryName = x.Category.Name
        }).ToList();

        // Build Spec Matrix
        var specKeys = new List<string>
        {
            "Thương hiệu", "Model", "Công nghệ Inverter", "Công suất (BTU)", "Loại Gas",
            "Bảo hành", "Tồn kho", "Đánh giá", "Cửa hàng bán"
        };

        var specMatrix = new Dictionary<string, Dictionary<int, string>>();

        foreach (var key in specKeys)
        {
            specMatrix[key] = new Dictionary<int, string>();
            foreach (var p in products)
            {
                var val = key switch
                {
                    "Thương hiệu" => p.BrandEntity?.Name ?? p.Brand ?? "--",
                    "Model" => p.Model ?? "--",
                    "Công nghệ Inverter" => p.IsInverter ? "Có (Inverter tiết kiệm điện)" : "Không",
                    "Công suất (BTU)" => p.CapacityBtu.HasValue ? $"{p.CapacityBtu.Value:N0} BTU" : "--",
                    "Loại Gas" => p.GasType ?? "--",
                    "Bảo hành" => $"{p.WarrantyMonths} tháng",
                    "Tồn kho" => p.Stock > 0 ? $"Còn hàng ({p.Stock})" : "Hết hàng",
                    "Đánh giá" => $"{p.AverageRating:F1}⭐ ({p.ReviewCount} đánh giá)",
                    "Cửa hàng bán" => p.Store.Name,
                    _ => "--"
                };
                specMatrix[key][p.Id] = val;
            }
        }

        return new ProductCompareDto
        {
            Products = cardList,
            SpecKeys = specKeys,
            SpecMatrix = specMatrix
        };
    }

    public async Task<List<ProductCardDto>> GetRelatedProductsAsync(int productId, int limit = 6)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null) return new List<ProductCardDto>();

        return await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Store)
            .Include(x => x.BrandEntity)
            .Where(x => x.CategoryId == product.CategoryId && x.Id != productId && !x.IsHidden)
            .OrderByDescending(x => x.SoldCount)
            .Take(limit)
            .Select(x => new ProductCardDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Brand = x.BrandEntity != null ? x.BrandEntity.Name : x.Brand,
                MainImageUrl = x.MainImageUrl,
                Price = x.Price,
                SalePrice = x.SalePrice,
                AverageRating = x.AverageRating,
                ReviewCount = x.ReviewCount,
                Stock = x.Stock,
                StoreName = x.Store.Name,
                CategoryName = x.Category.Name
            })
            .ToListAsync();
    }

    public async Task<List<ProductCardDto>> GetSameBrandProductsAsync(int brandId, int currentProductId, int limit = 6)
    {
        return await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Store)
            .Include(x => x.BrandEntity)
            .Where(x => x.BrandId == brandId && x.Id != currentProductId && !x.IsHidden)
            .OrderByDescending(x => x.IsFeatured)
            .Take(limit)
            .Select(x => new ProductCardDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Brand = x.BrandEntity != null ? x.BrandEntity.Name : x.Brand,
                MainImageUrl = x.MainImageUrl,
                Price = x.Price,
                SalePrice = x.SalePrice,
                AverageRating = x.AverageRating,
                ReviewCount = x.ReviewCount,
                Stock = x.Stock,
                StoreName = x.Store.Name,
                CategoryName = x.Category.Name
            })
            .ToListAsync();
    }

    public async Task<PagedResult<ProductReviewDto>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10)
    {
        var query = _db.ProductReviews
            .Where(x => x.ProductId == productId && x.IsApproved);

        var total = await query.CountAsync();
        var reviews = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductReviewDto
            {
                Id = x.Id,
                ProductId = x.ProductId,
                CustomerName = x.CustomerName,
                CustomerAvatarUrl = x.CustomerAvatarUrl,
                Rating = x.Rating,
                Title = x.Title,
                Comment = x.Comment,
                Pros = x.Pros,
                Cons = x.Cons,
                ImageUrls = !string.IsNullOrEmpty(x.ImageUrlsJson) ? JsonSerializer.Deserialize<List<string>>(x.ImageUrlsJson, (JsonSerializerOptions)null!) ?? new() : new(),
                VideoUrl = x.VideoUrl,
                IsVerifiedPurchase = x.IsVerifiedPurchase,
                CreatedAt = x.CreatedAt,
                StoreResponseNote = x.StoreResponseNote,
                StoreResponseAt = x.StoreResponseAt
            })
            .ToListAsync();

        return PagedResult<ProductReviewDto>.Create(reviews, total, page, pageSize);
    }

    public async Task<ServiceResult<ProductReviewDto>> AddProductReviewAsync(string customerId, string customerName, ProductReviewCreateDto dto)
    {
        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product == null) return ServiceResult<ProductReviewDto>.Fail("Sản phẩm không tồn tại.");

        var review = new ProductReview
        {
            ProductId = dto.ProductId,
            StoreId = product.StoreId,
            CustomerId = customerId,
            CustomerName = customerName,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Title = dto.Title,
            Comment = dto.Comment,
            Pros = dto.Pros,
            Cons = dto.Cons,
            ImageUrlsJson = dto.ImageUrls.Any() ? JsonSerializer.Serialize(dto.ImageUrls) : null,
            IsVerifiedPurchase = true,
            IsApproved = true
        };

        _db.ProductReviews.Add(review);
        await _db.SaveChangesAsync();

        // Recalculate AverageRating & ReviewCount
        var allReviews = await _db.ProductReviews
            .Where(x => x.ProductId == dto.ProductId && x.IsApproved)
            .ToListAsync();

        product.ReviewCount = allReviews.Count;
        product.AverageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 5.0;

        _db.Products.Update(product);
        await _db.SaveChangesAsync();

        var resultDto = new ProductReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            CustomerName = review.CustomerName,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            Pros = review.Pros,
            Cons = review.Cons,
            ImageUrls = dto.ImageUrls,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            CreatedAt = review.CreatedAt
        };

        return ServiceResult<ProductReviewDto>.Ok(resultDto, "Cảm ơn bạn đã gửi đánh giá!");
    }

    public async Task<ProductAiContextDto> GetAiProductContextAsync(int productId)
    {
        var product = await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Store)
            .Include(x => x.BrandEntity)
            .FirstOrDefaultAsync(x => x.Id == productId);

        if (product == null) return new ProductAiContextDto();

        var areaRec = product.CapacityBtu switch
        {
            <= 9000 => "Phù hợp phòng dưới 15m² (diện tích phòng ngủ nhỏ, phòng làm việc)",
            <= 12000 => "Phù hợp phòng từ 15m² đến 20m² (phòng ngủ rộng, phòng khách nhỏ)",
            <= 18000 => "Phù hợp phòng từ 20m² đến 30m² (phòng khách vừa, văn phòng)",
            _ => "Phù hợp phòng trên 30m² (phòng khách rộng, hội trường)"
        };

        return new ProductAiContextDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Brand = product.BrandEntity?.Name ?? product.Brand ?? "Chưa rõ",
            Model = product.Model ?? "N/A",
            CurrentPrice = product.SalePrice ?? product.Price,
            WarrantyMonths = product.WarrantyMonths,
            IsInverter = product.IsInverter,
            CapacityBtu = product.CapacityBtu,
            RoomAreaRecommended = areaRec,
            KeyFeaturesSummary = $"{product.Category.Name} {product.Brand} {product.Model}. Tiết kiệm điện Inverter: {(product.IsInverter ? "Có" : "Không")}. Bảo hành: {product.WarrantyMonths} tháng chính hãng.",
            StoreName = product.Store.Name,
            SuggestedQuestions = new List<string>
            {
                $"Máy {product.Name} này có phù hợp cho phòng 20m² không?",
                $"Công nghệ Inverter của {product.Brand} giúp tiết kiệm bao nhiêu % điện năng?",
                $"So sánh {product.Name} với dòng sản phẩm cùng tầm giá?",
                $"Chính sách bảo hành và chi phí lắp đặt tại {product.Store.Name} như thế nào?"
            }
        };
    }

    public async Task<List<ProductBrandDto>> GetAllBrandsAsync()
    {
        return await _db.ProductBrands
            .Select(b => new ProductBrandDto
            {
                Id = b.Id,
                Name = b.Name,
                Slug = b.Slug,
                LogoUrl = b.LogoUrl,
                Country = b.Country,
                ProductCount = b.Products.Count(p => !p.IsHidden)
            })
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    private async Task<ProductDetailViewDto> BuildProductDetailViewDtoAsync(Product product)
    {
        // Parse Specs
        var specsGrouped = new Dictionary<string, List<KeyValuePair<string, string>>>();
        if (!string.IsNullOrEmpty(product.SpecificationsJson))
        {
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(product.SpecificationsJson);
                if (dict != null)
                {
                    specsGrouped["Thông số kỹ thuật"] = dict.Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value)).ToList();
                }
            }
            catch { }
        }

        // Ratings Breakdown
        var reviews = await _db.ProductReviews
            .Where(r => r.ProductId == product.Id && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        var dist = new Dictionary<int, int> { { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 } };
        foreach (var r in reviews)
        {
            if (dist.ContainsKey(r.Rating)) dist[r.Rating]++;
        }

        var galleryImages = product.Images.Select(i => new ProductImageDto
        {
            Id = i.Id,
            ImageUrl = i.ImageUrl,
            AltText = i.AltText,
            IsMain = i.IsMain
        }).ToList();

        if (!galleryImages.Any() && !string.IsNullOrEmpty(product.MainImageUrl))
        {
            galleryImages.Add(new ProductImageDto { ImageUrl = product.MainImageUrl, IsMain = true });
        }

        var dto = new ProductDetailViewDto
        {
            Id = product.Id,
            StoreId = product.StoreId,
            StoreName = product.Store.Name,
            StoreSlug = product.Store.Slug,
            StoreLogoUrl = product.Store.LogoUrl ?? string.Empty,
            StoreRating = product.Store.AverageRating ?? 5.0,
            StoreReviewCount = product.Store.TotalReviews,
            StoreProvince = product.Store.Address?.Province ?? "Toàn quốc",
            StorePhone = product.Store.PhoneNumber,
            WarrantyPolicy = product.Store.WarrantyPolicy,
            ReturnPolicy = product.Store.ReturnPolicy,

            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            CategorySlug = product.Category.Slug,
            BrandId = product.BrandId,
            Brand = product.BrandEntity?.Name ?? product.Brand,
            BrandLogoUrl = product.BrandEntity?.LogoUrl,

            Name = product.Name,
            Slug = product.Slug,
            Sku = product.Sku,
            SubTitle = product.SubTitle,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            Model = product.Model ?? string.Empty,
            Price = product.Price,
            SalePrice = product.SalePrice,
            Stock = product.Stock,
            WarrantyMonths = product.WarrantyMonths,
            MainImageUrl = product.MainImageUrl ?? galleryImages.FirstOrDefault()?.ImageUrl,
            GalleryImages = galleryImages,
            Videos = product.Videos.Select(v => new ProductVideoDto
            {
                Id = v.Id,
                Title = v.Title,
                VideoUrl = v.VideoUrl,
                ThumbnailUrl = v.ThumbnailUrl,
                VideoType = v.VideoType
            }).ToList(),

            SpecificationsGrouped = specsGrouped,
            IsInverter = product.IsInverter,
            CapacityBtu = product.CapacityBtu,
            RoomAreaCode = product.RoomAreaCode,
            GasType = product.GasType,

            AverageRating = product.AverageRating,
            ReviewCount = product.ReviewCount,
            RatingDistribution = dist,
            RecentReviews = reviews.Select(r => new ProductReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                CustomerName = r.CustomerName,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                Pros = r.Pros,
                Cons = r.Cons,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                CreatedAt = r.CreatedAt
            }).ToList(),

            ActivePromotions = product.Promotions.Where(p => p.IsActive).Select(p => new ProductPromotionDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                GiftItemName = p.GiftItemName,
                FreeInstallation = p.FreeInstallation
            }).ToList(),

            AiContext = await GetAiProductContextAsync(product.Id)
        };

        return dto;
    }
}
