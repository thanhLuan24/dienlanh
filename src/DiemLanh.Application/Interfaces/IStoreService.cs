using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Store;

namespace DiemLanh.Application.Interfaces;

public interface IStoreService
{
    // Public queries
    Task<PagedResult<StoreListDto>> GetStoresAsync(StoreFilterDto filter);
    Task<StoreDto?> GetStoreByIdAsync(int id);
    Task<StoreDto?> GetStoreBySlugAsync(string slug);
    Task<IEnumerable<StoreListDto>> GetStoresByProvinceAsync(string province);
    Task<IEnumerable<StoreListDto>> GetNearbyStoresAsync(double latitude, double longitude, double radiusKm = 20);

    // CRUD
    Task<ServiceResult<StoreDto>> CreateStoreAsync(StoreCreateDto dto);
    Task<ServiceResult<StoreDto>> UpdateStoreAsync(StoreUpdateDto dto);
    Task<ServiceResult> DeleteStoreAsync(int id);
    Task<ServiceResult> UpdateStoreStatusAsync(int id, Domain.Enums.StoreStatus status);

    // Profile
    Task<ServiceResult> UpdateStoreProfileAsync(int storeId, StoreUpdateDto dto);
    Task<ServiceResult> UpdateLogoAsync(int storeId, string logoUrl);
    Task<ServiceResult> UpdateCoverAsync(int storeId, string coverUrl);

    // Service Areas
    Task<IEnumerable<StoreServiceAreaDto>> GetServiceAreasAsync(int storeId);
    Task<IEnumerable<StoreServiceAreaDto>> GetAllServiceAreasAsync(int? storeId);
    Task<ServiceResult<StoreServiceAreaDto>> AddServiceAreaAsync(StoreServiceAreaCreateDto dto);
    Task<ServiceResult> RemoveServiceAreaAsync(int areaId, int storeId);
    Task<ServiceResult> AdminRemoveServiceAreaAsync(int areaId);

    // Brands
    Task<IEnumerable<StoreBrandDto>> GetBrandsAsync(int storeId);
    Task<ServiceResult<StoreBrandDto>> AddBrandAsync(StoreBrandCreateDto dto);
    Task<ServiceResult> RemoveBrandAsync(int brandId, int storeId);

    // Store ownership check
    Task<bool> IsStoreOwnerAsync(int storeId, string userId);
    Task<int?> GetStoreIdByOwnerAsync(string userId);
}
