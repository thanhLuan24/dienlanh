using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Admin;

namespace DiemLanh.Application.Interfaces;

public interface IAdminService
{
    // Overview
    Task<AdminOverviewDto> GetOverviewAsync();

    // Products
    Task<PagedResult<AdminProductListDto>> GetProductsAsync(string? keyword, int? categoryId, int? brandId, int page = 1, int pageSize = 20);
    Task<AdminProductCreateEditDto?> GetProductForEditAsync(int id);
    Task<(bool Success, string Message, int Id)> CreateProductAsync(AdminProductCreateEditDto dto);
    Task<(bool Success, string Message)> UpdateProductAsync(AdminProductCreateEditDto dto);
    Task<(bool Success, string Message)> DeleteProductAsync(int id);

    // Stores
    Task<PagedResult<AdminStoreListDto>> GetStoresAsync(string? keyword, string? status, int page = 1, int pageSize = 20);
    Task<(bool Success, string Message)> CreateStoreAsync(AdminStoreCreateDto dto);
    Task<(bool Success, string Message)> DeleteStoreAsync(int id);
    Task<(bool Success, string Message)> ToggleStoreStatusAsync(int id);

    // Users
    Task<PagedResult<AdminUserListDto>> GetUsersAsync(string? keyword, string? role, int page = 1, int pageSize = 20);
    Task<(bool Success, string Message)> ToggleUserStatusAsync(string userId);
    Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string newRole, int? storeId = null);
    Task<List<StoreSimpleDto>> GetActiveStoresSimpleAsync();
}
