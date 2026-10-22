using AutoMapper;
using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Entities.Store;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiemLanh.Infrastructure.Services;

public class StoreService : IStoreService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<StoreService> _logger;

    public StoreService(ApplicationDbContext db, IMapper mapper, ILogger<StoreService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<StoreListDto>> GetStoresAsync(StoreFilterDto filter)
    {
        var query = _db.Stores
            .Include(s => s.Address)
            .Include(s => s.ServiceAreas)
            .Include(s => s.Brands)
            .AsQueryable();

        // Filters
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(s => s.Name.Contains(filter.Keyword) ||
                                     (s.Description != null && s.Description.Contains(filter.Keyword)));

        if (!string.IsNullOrWhiteSpace(filter.Province))
            query = query.Where(s => s.Address != null && s.Address.Province == filter.Province ||
                                     s.ServiceAreas.Any(a => a.Province == filter.Province));

        if (!string.IsNullOrWhiteSpace(filter.District))
            query = query.Where(s => s.Address != null && s.Address.District == filter.District);

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(s => s.Brands.Any(b => b.BrandName.Contains(filter.Brand)));

        if (filter.Status.HasValue)
            query = query.Where(s => s.Status == filter.Status.Value);
        else
            query = query.Where(s => s.Status == StoreStatus.Active);

        if (filter.MinRating.HasValue)
            query = query.Where(s => s.AverageRating >= filter.MinRating.Value);

        // Sorting
        query = filter.SortBy?.ToLower() switch
        {
            "rating" => filter.SortDesc ? query.OrderByDescending(s => s.AverageRating) : query.OrderBy(s => s.AverageRating),
            "reviews" => filter.SortDesc ? query.OrderByDescending(s => s.TotalReviews) : query.OrderBy(s => s.TotalReviews),
            "joined" => filter.SortDesc ? query.OrderByDescending(s => s.JoinedAt) : query.OrderBy(s => s.JoinedAt),
            _ => filter.SortDesc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return PagedResult<StoreListDto>.Create(
            _mapper.Map<List<StoreListDto>>(items),
            totalCount, filter.Page, filter.PageSize);
    }

    public async Task<StoreDto?> GetStoreByIdAsync(int id)
    {
        var store = await _db.Stores
            .Include(s => s.Address)
            .Include(s => s.ServiceAreas)
            .Include(s => s.Brands)
            .Include(s => s.Images)
            .Include(s => s.WorkingHours)
            .FirstOrDefaultAsync(s => s.Id == id);

        return store == null ? null : _mapper.Map<StoreDto>(store);
    }

    public async Task<StoreDto?> GetStoreBySlugAsync(string slug)
    {
        var store = await _db.Stores
            .Include(s => s.Address)
            .Include(s => s.ServiceAreas)
            .Include(s => s.Brands)
            .Include(s => s.Images)
            .Include(s => s.WorkingHours)
            .FirstOrDefaultAsync(s => s.Slug == slug);

        return store == null ? null : _mapper.Map<StoreDto>(store);
    }

    public async Task<IEnumerable<StoreListDto>> GetStoresByProvinceAsync(string province)
    {
        var stores = await _db.Stores
            .Include(s => s.Address)
            .Include(s => s.ServiceAreas)
            .Include(s => s.Brands)
            .Where(s => s.Status == StoreStatus.Active &&
                        (s.Address!.Province == province ||
                         s.ServiceAreas.Any(a => a.Province == province && a.IsActive)))
            .OrderByDescending(s => s.AverageRating)
            .ToListAsync();

        return _mapper.Map<List<StoreListDto>>(stores);
    }

    public async Task<IEnumerable<StoreListDto>> GetNearbyStoresAsync(double latitude, double longitude, double radiusKm = 20)
    {
        // Basic implementation – filter by service areas with GPS coords
        // For production, use SQL Server geography types or NetTopologySuite
        var stores = await _db.Stores
            .Include(s => s.Address)
            .Include(s => s.ServiceAreas)
            .Include(s => s.Brands)
            .Where(s => s.Status == StoreStatus.Active &&
                        s.Address != null && s.Address.Latitude != null && s.Address.Longitude != null)
            .ToListAsync();

        var nearby = stores.Where(s =>
        {
            var dist = CalculateDistance(latitude, longitude, s.Address!.Latitude!.Value, s.Address.Longitude!.Value);
            return dist <= radiusKm;
        }).OrderBy(s => CalculateDistance(latitude, longitude, s.Address!.Latitude!.Value, s.Address!.Longitude!.Value));

        return _mapper.Map<List<StoreListDto>>(nearby);
    }

    public async Task<ServiceResult<StoreDto>> CreateStoreAsync(StoreCreateDto dto)
    {
        try
        {
            var slug = GenerateSlug(dto.Name);
            if (await _db.Stores.AnyAsync(s => s.Slug == slug))
                slug = $"{slug}-{DateTime.UtcNow.Ticks % 10000}";

            var store = _mapper.Map<Store>(dto);
            store.Slug = slug;

            _db.Stores.Add(store);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Store created: {StoreName} (ID: {StoreId})", store.Name, store.Id);
            return ServiceResult<StoreDto>.Ok(_mapper.Map<StoreDto>(store));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating store: {StoreName}", dto.Name);
            return ServiceResult<StoreDto>.Fail("Lỗi khi tạo cửa hàng: " + ex.Message);
        }
    }

    public async Task<ServiceResult<StoreDto>> UpdateStoreAsync(StoreUpdateDto dto)
    {
        var store = await _db.Stores.Include(s => s.Address).FirstOrDefaultAsync(s => s.Id == dto.Id);
        if (store == null)
            return ServiceResult<StoreDto>.Fail("Không tìm thấy cửa hàng");

        _mapper.Map(dto, store);

        if (store.Address != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.Street)) store.Address.Street = dto.Street;
            if (dto.Ward != null) store.Address.Ward = dto.Ward;
            if (!string.IsNullOrWhiteSpace(dto.District)) store.Address.District = dto.District;
            if (!string.IsNullOrWhiteSpace(dto.Province)) store.Address.Province = dto.Province;
            if (dto.Latitude.HasValue) store.Address.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) store.Address.Longitude = dto.Longitude;
        }
        else if (!string.IsNullOrWhiteSpace(dto.Street) || !string.IsNullOrWhiteSpace(dto.Province))
        {
            store.Address = new StoreAddress
            {
                StoreId = dto.Id,
                Street = dto.Street ?? "",
                Ward = dto.Ward,
                District = dto.District ?? "",
                Province = dto.Province ?? "",
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };
        }

        await _db.SaveChangesAsync();
        return ServiceResult<StoreDto>.Ok(_mapper.Map<StoreDto>(store));
    }

    public async Task<ServiceResult> DeleteStoreAsync(int id)
    {
        var store = await _db.Stores.FindAsync(id);
        if (store == null) return ServiceResult.Fail("Không tìm thấy cửa hàng");

        store.IsDeleted = true;
        store.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Đã xóa cửa hàng");
    }

    public async Task<ServiceResult> UpdateStoreStatusAsync(int id, StoreStatus status)
    {
        var store = await _db.Stores.FindAsync(id);
        if (store == null) return ServiceResult.Fail("Không tìm thấy cửa hàng");

        store.Status = status;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateStoreProfileAsync(int storeId, StoreUpdateDto dto)
    {
        dto.Id = storeId;
        var result = await UpdateStoreAsync(dto);
        return result.Success ? ServiceResult.Ok() : ServiceResult.Fail(result.Errors);
    }

    public async Task<ServiceResult> UpdateLogoAsync(int storeId, string logoUrl)
    {
        var store = await _db.Stores.FindAsync(storeId);
        if (store == null) return ServiceResult.Fail("Không tìm thấy cửa hàng");
        store.LogoUrl = logoUrl;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateCoverAsync(int storeId, string coverUrl)
    {
        var store = await _db.Stores.FindAsync(storeId);
        if (store == null) return ServiceResult.Fail("Không tìm thấy cửa hàng");
        store.CoverImageUrl = coverUrl;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<StoreServiceAreaDto>> GetServiceAreasAsync(int storeId)
    {
        var areas = await _db.StoreServiceAreas.Include(a => a.Store).Where(a => a.StoreId == storeId).ToListAsync();
        var dtos = _mapper.Map<List<StoreServiceAreaDto>>(areas);
        foreach (var d in dtos)
        {
            var area = areas.FirstOrDefault(a => a.Id == d.Id);
            if (area?.Store != null) d.StoreName = area.Store.Name;
        }
        return dtos;
    }

    public async Task<IEnumerable<StoreServiceAreaDto>> GetAllServiceAreasAsync(int? storeId)
    {
        var query = _db.StoreServiceAreas.Include(a => a.Store).AsQueryable();
        if (storeId.HasValue && storeId.Value > 0)
            query = query.Where(a => a.StoreId == storeId.Value);

        var areas = await query.ToListAsync();
        var dtos = _mapper.Map<List<StoreServiceAreaDto>>(areas);
        foreach (var d in dtos)
        {
            var area = areas.FirstOrDefault(a => a.Id == d.Id);
            if (area?.Store != null) d.StoreName = area.Store.Name;
        }
        return dtos;
    }

    public async Task<ServiceResult<StoreServiceAreaDto>> AddServiceAreaAsync(StoreServiceAreaCreateDto dto)
    {
        var area = _mapper.Map<StoreServiceArea>(dto);
        _db.StoreServiceAreas.Add(area);
        await _db.SaveChangesAsync();
        return ServiceResult<StoreServiceAreaDto>.Ok(_mapper.Map<StoreServiceAreaDto>(area));
    }

    public async Task<ServiceResult> RemoveServiceAreaAsync(int areaId, int storeId)
    {
        var area = await _db.StoreServiceAreas.FirstOrDefaultAsync(a => a.Id == areaId && a.StoreId == storeId);
        if (area == null) return ServiceResult.Fail("Không tìm thấy khu vực");
        _db.StoreServiceAreas.Remove(area);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AdminRemoveServiceAreaAsync(int areaId)
    {
        var area = await _db.StoreServiceAreas.FirstOrDefaultAsync(a => a.Id == areaId);
        if (area == null) return ServiceResult.Fail("Không tìm thấy khu vực");
        _db.StoreServiceAreas.Remove(area);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<StoreBrandDto>> GetBrandsAsync(int storeId)
    {
        var brands = await _db.StoreBrands.Where(b => b.StoreId == storeId && b.IsActive).ToListAsync();
        return _mapper.Map<List<StoreBrandDto>>(brands);
    }

    public async Task<ServiceResult<StoreBrandDto>> AddBrandAsync(StoreBrandCreateDto dto)
    {
        if (await _db.StoreBrands.AnyAsync(b => b.StoreId == dto.StoreId && b.BrandName == dto.BrandName))
            return ServiceResult<StoreBrandDto>.Fail("Hãng này đã được thêm");

        var brand = _mapper.Map<StoreBrand>(dto);
        _db.StoreBrands.Add(brand);
        await _db.SaveChangesAsync();
        return ServiceResult<StoreBrandDto>.Ok(_mapper.Map<StoreBrandDto>(brand));
    }

    public async Task<ServiceResult> RemoveBrandAsync(int brandId, int storeId)
    {
        var brand = await _db.StoreBrands.FirstOrDefaultAsync(b => b.Id == brandId && b.StoreId == storeId);
        if (brand == null) return ServiceResult.Fail("Không tìm thấy hãng");
        _db.StoreBrands.Remove(brand);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<bool> IsStoreOwnerAsync(int storeId, string userId)
        => await _db.Stores.AnyAsync(s => s.Id == storeId && s.OwnerId == userId);

    public async Task<int?> GetStoreIdByOwnerAsync(string userId)
    {
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.OwnerId == userId);
        return store?.Id;
    }

    // Helpers
    private static string GenerateSlug(string name)
    {
        var normalized = name.ToLower()
            .Replace("đ", "d")
            .Replace("à", "a").Replace("á", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
            .Replace("ă", "a").Replace("ằ", "a").Replace("ắ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
            .Replace("â", "a").Replace("ầ", "a").Replace("ấ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
            .Replace("è", "e").Replace("é", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
            .Replace("ê", "e").Replace("ề", "e").Replace("ế", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
            .Replace("ì", "i").Replace("í", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
            .Replace("ò", "o").Replace("ó", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
            .Replace("ô", "o").Replace("ồ", "o").Replace("ố", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
            .Replace("ơ", "o").Replace("ờ", "o").Replace("ớ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
            .Replace("ù", "u").Replace("ú", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
            .Replace("ư", "u").Replace("ừ", "u").Replace("ứ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
            .Replace("ỳ", "y").Replace("ý", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");

        return System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9\s-]", "")
            .Trim().Replace(" ", "-")
            .Replace("--", "-");
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
