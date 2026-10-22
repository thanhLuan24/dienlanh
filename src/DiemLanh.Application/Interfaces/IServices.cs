using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Dashboard;
using DiemLanh.Application.DTOs.Order;
using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.DTOs.Repair;

namespace DiemLanh.Application.Interfaces;

public interface IOrderService
{
    // Store-side (scoped to a specific store)
    Task<PagedResult<OrderListDto>> GetOrdersAsync(int storeId, OrderFilterDto filter);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, int storeId);
    Task<ServiceResult> UpdateOrderStatusAsync(OrderUpdateStatusDto dto, int storeId);
    Task<ServiceResult<InstallationDto>> ScheduleInstallationAsync(InstallationScheduleDto dto, int storeId);
    Task<ServiceResult> RescheduleInstallationAsync(InstallationRescheduleDto dto, int storeId);
    Task<PagedResult<InstallationDto>> GetInstallationsAsync(int storeId, InstallationFilterDto filter);
    Task<InstallationDto?> GetInstallationByIdAsync(int installationId, int storeId);
    Task<ServiceResult> UpdateInstallationStatusAsync(int installationId, Domain.Enums.InstallationStatus status, string? note, int storeId);
    Task<IEnumerable<UpcomingInstallationDto>> GetTodayInstallationsAsync(int storeId);

    // Customer-side (checkout & order history)
    Task<ServiceResult<OrderDto>> PlaceOrderAsync(OrderPlaceDto dto, string customerId);
    Task<List<CustomerOrderSummaryDto>> GetCustomerOrdersAsync(string customerId);
    Task<CustomerOrderSummaryDto?> GetCustomerOrderByIdAsync(int orderId, string customerId);
    Task<ServiceResult> CancelCustomerOrderAsync(int orderId, string customerId);

    // Admin-side (system-wide access)
    Task<PagedResult<OrderListDto>> GetAllOrdersAsync(OrderFilterDto filter);
    Task<OrderDto?> GetOrderByIdAsync(int orderId);
    Task<ServiceResult> AdminUpdateOrderStatusAsync(OrderUpdateStatusDto dto);
    Task<PagedResult<InstallationDto>> GetAllInstallationsAsync(int? storeId, InstallationFilterDto filter);
    Task<InstallationDto?> GetInstallationByIdAsync(int installationId);
    Task<ServiceResult> AdminUpdateInstallationStatusAsync(int installationId, Domain.Enums.InstallationStatus status, string? note);
}

public interface IRepairService
{
    Task<PagedResult<RepairRequestDto>> GetRepairRequestsAsync(int storeId, RepairFilterDto filter);
    Task<RepairRequestDto?> GetRepairRequestByIdAsync(int id, int storeId);
    Task<ServiceResult<RepairRequestDto>> CreateRepairRequestAsync(RepairRequestCreateDto dto, string customerId);
    Task<ServiceResult> UpdateRepairStatusAsync(RepairStatusUpdateDto dto, int storeId);
    Task<ServiceResult> AddRepairImagesAsync(int repairId, List<string> imageUrls, int storeId);
    Task<IEnumerable<RepairRequestDto>> GetTodayRepairsAsync(int storeId);

    // Customer-side
    Task<List<RepairRequestDto>> GetCustomerRepairsAsync(string customerId);
    Task<RepairRequestDto?> GetRepairRequestByIdForCustomerAsync(int id, string customerId);
    Task<ServiceResult> CancelCustomerRepairAsync(int id, string customerId);

    // Admin-side
    Task<PagedResult<RepairRequestDto>> GetAllRepairRequestsAsync(int? storeId, RepairFilterDto filter);
    Task<RepairRequestDto?> GetRepairRequestByIdAsync(int id);
    Task<ServiceResult> AdminUpdateRepairStatusAsync(RepairStatusUpdateDto dto);
}

public interface IDashboardService
{
    Task<StoreDashboardDto> GetStoreDashboardAsync(int storeId);
    Task<StoreStatisticsDto> GetStoreStatisticsAsync(int storeId, int year, int? month = null);
}

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);
    Task DeleteFileAsync(string filePath);
    bool IsValidImageType(string contentType);
    bool IsValidFileSize(long fileSizeBytes, long maxSizeMb = 5);
    string GetPublicUrl(string filePath);
}

public interface IProductService
{
    Task<PagedResult<ProductListDto>> GetProductsAsync(int storeId, ProductFilterDto filter);
    Task<ProductDto?> GetProductByIdAsync(int id, int storeId);
    Task<ServiceResult<ProductDto>> CreateProductAsync(ProductCreateDto dto);
    Task<ServiceResult<ProductDto>> UpdateProductAsync(ProductUpdateDto dto, int storeId);
    Task<ServiceResult> DeleteProductAsync(int id, int storeId);
    Task<ServiceResult> ToggleHiddenAsync(int id, int storeId);
    Task<ServiceResult> UpdateStockAsync(int id, int stock, int storeId);
    Task<ServiceResult> AddProductImagesAsync(int productId, List<string> imageUrls, int storeId);
    Task<ServiceResult> RemoveProductImageAsync(int imageId, int storeId);
    Task<IEnumerable<ProductCategoryDto>> GetCategoriesAsync();
}

public interface ITechnicianService
{
    Task<IEnumerable<TechnicianDto>> GetTechniciansAsync(int storeId);
    Task<IEnumerable<TechnicianDto>> GetAllTechniciansAsync(int? storeId);
    Task<ServiceResult<TechnicianDto>> CreateTechnicianAsync(TechnicianCreateDto dto);
    Task<ServiceResult<TechnicianDto>> UpdateTechnicianAsync(TechnicianUpdateDto dto, int storeId);
    Task<ServiceResult> DeleteTechnicianAsync(int id, int storeId);
    Task<ServiceResult> AdminDeleteTechnicianAsync(int id);
}

public class TechnicianDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Specialty { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
}

public class TechnicianCreateDto
{
    public int StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Specialty { get; set; }
}

public class TechnicianUpdateDto : TechnicianCreateDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
}
