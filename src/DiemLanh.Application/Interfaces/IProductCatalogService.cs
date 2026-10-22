using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Product;

namespace DiemLanh.Application.Interfaces;

public interface IProductCatalogService
{
    Task<PagedResult<ProductCardDto>> GetCatalogProductsAsync(ProductCatalogFilterDto filter);
    Task<ProductDetailViewDto?> GetProductDetailBySlugAsync(string slug);
    Task<ProductDetailViewDto?> GetProductDetailByIdAsync(int id);
    Task<ProductCardDto?> GetQuickViewAsync(int id);
    Task<List<ProductAutocompleteDto>> AutocompleteSearchAsync(string query, int maxResults = 8);
    Task<ProductCompareDto> CompareProductsAsync(List<int> productIds);
    Task<List<ProductCardDto>> GetRelatedProductsAsync(int productId, int limit = 6);
    Task<List<ProductCardDto>> GetSameBrandProductsAsync(int brandId, int currentProductId, int limit = 6);
    Task<PagedResult<ProductReviewDto>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10);
    Task<ServiceResult<ProductReviewDto>> AddProductReviewAsync(string customerId, string customerName, ProductReviewCreateDto dto);
    Task<ProductAiContextDto> GetAiProductContextAsync(int productId);
    Task<List<ProductBrandDto>> GetAllBrandsAsync();
}

public class ProductBrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public int ProductCount { get; set; }
}
