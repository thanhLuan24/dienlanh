using AutoMapper;
using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Entities.Product;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DiemLanh.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDbContext db, IMapper mapper, ILogger<ProductService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ProductListDto>> GetProductsAsync(int storeId, ProductFilterDto filter)
    {
        var query = _db.Products.Where(p => p.StoreId == storeId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(p => p.Name.Contains(filter.Keyword) || (p.Brand != null && p.Brand.Contains(filter.Keyword)));

        if (filter.CategoryId.HasValue) query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Brand)) query = query.Where(p => p.Brand == filter.Brand);
        if (filter.Status.HasValue) query = query.Where(p => p.Status == filter.Status.Value);
        if (filter.IsHidden.HasValue) query = query.Where(p => p.IsHidden == filter.IsHidden.Value);
        if (filter.MinPrice.HasValue) query = query.Where(p => p.Price >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue) query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        query = filter.SortBy?.ToLower() switch
        {
            "price" => filter.SortDesc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "sold" => filter.SortDesc ? query.OrderByDescending(p => p.SoldCount) : query.OrderBy(p => p.SoldCount),
            "name" => filter.SortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            _ => filter.SortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return PagedResult<ProductListDto>.Create(_mapper.Map<List<ProductListDto>>(items), total, filter.Page, filter.PageSize);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, int storeId)
    {
        var product = await _db.Products
            .Include(p => p.Store)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.StoreId == storeId);

        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ServiceResult<ProductDto>> CreateProductAsync(ProductCreateDto dto)
    {
        var slug = GenerateSlug(dto.Name);
        if (await _db.Products.AnyAsync(p => p.Slug == slug))
            slug = $"{slug}-{DateTime.UtcNow.Ticks % 10000}";

        var product = _mapper.Map<Product>(dto);
        product.Slug = slug;

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return ServiceResult<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
    }

    public async Task<ServiceResult<ProductDto>> UpdateProductAsync(ProductUpdateDto dto, int storeId)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == dto.Id && p.StoreId == storeId);
        if (product == null) return ServiceResult<ProductDto>.Fail("Không tìm thấy sản phẩm");

        _mapper.Map(dto, product);
        product.SpecificationsJson = JsonSerializer.Serialize(dto.Specifications);
        await _db.SaveChangesAsync();

        return ServiceResult<ProductDto>.Ok(_mapper.Map<ProductDto>(product));
    }

    public async Task<ServiceResult> DeleteProductAsync(int id, int storeId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.StoreId == storeId);
        if (product == null) return ServiceResult.Fail("Không tìm thấy sản phẩm");

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ToggleHiddenAsync(int id, int storeId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.StoreId == storeId);
        if (product == null) return ServiceResult.Fail("Không tìm thấy sản phẩm");

        product.IsHidden = !product.IsHidden;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok(product.IsHidden ? "Đã ẩn sản phẩm" : "Đã hiện sản phẩm");
    }

    public async Task<ServiceResult> UpdateStockAsync(int id, int stock, int storeId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id && p.StoreId == storeId);
        if (product == null) return ServiceResult.Fail("Không tìm thấy sản phẩm");

        product.Stock = stock;
        product.Status = stock == 0 ? ProductStatus.OutOfStock : ProductStatus.Active;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AddProductImagesAsync(int productId, List<string> imageUrls, int storeId)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId);
        if (product == null) return ServiceResult.Fail("Không tìm thấy sản phẩm");

        var isFirst = !product.Images.Any();
        var maxSort = product.Images.Any() ? product.Images.Max(i => i.SortOrder) : 0;

        foreach (var url in imageUrls)
        {
            var img = new ProductImage { ProductId = productId, ImageUrl = url, SortOrder = ++maxSort, IsMain = isFirst };
            _db.ProductImages.Add(img);
            isFirst = false;
        }

        if (product.MainImageUrl == null && imageUrls.Any()) product.MainImageUrl = imageUrls.First();

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RemoveProductImageAsync(int imageId, int storeId)
    {
        var image = await _db.ProductImages.Include(i => i.Product).FirstOrDefaultAsync(i => i.Id == imageId && i.Product.StoreId == storeId);
        if (image == null) return ServiceResult.Fail("Không tìm thấy ảnh");

        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<ProductCategoryDto>> GetCategoriesAsync()
    {
        var cats = await _db.ProductCategories.Include(c => c.Products).Where(c => c.IsActive && !c.IsDeleted).OrderBy(c => c.SortOrder).ToListAsync();
        return _mapper.Map<List<ProductCategoryDto>>(cats);
    }

    private static string GenerateSlug(string name)
    {
        var normalized = name.ToLower()
            .Replace("đ", "d")
            .Replace("à", "a").Replace("á", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a");
        return Regex.Replace(normalized, @"[^a-z0-9\s-]", "").Trim().Replace(" ", "-").Replace("--", "-");
    }
}
