using DiemLanh.Application.DTOs.Admin;
using DiemLanh.Application.DTOs.Order;
using DiemLanh.Application.DTOs.Repair;
using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.Interfaces;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IOrderService _orderService;
    private readonly IRepairService _repairService;
    private readonly ITechnicianService _technicianService;
    private readonly IStoreService _storeService;

    public AdminController(
        IAdminService adminService,
        IOrderService orderService,
        IRepairService repairService,
        ITechnicianService technicianService,
        IStoreService storeService)
    {
        _adminService = adminService;
        _orderService = orderService;
        _repairService = repairService;
        _technicianService = technicianService;
        _storeService = storeService;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var overview = await _adminService.GetOverviewAsync();
        return View(overview);
    }

    // ─── Users ────────────────────────────────────────────────────────────────
    [HttpGet("users")]
    public async Task<IActionResult> Users([FromQuery] string? keyword, [FromQuery] string? role, [FromQuery] int page = 1)
    {
        var result = await _adminService.GetUsersAsync(keyword, role, page, 20);
        ViewBag.Keyword = keyword;
        ViewBag.Role = role;
        ViewBag.Stores = await _adminService.GetActiveStoresSimpleAsync();
        return View(result);
    }

    [HttpPost("users/{id}/toggle-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var (success, message) = await _adminService.ToggleUserStatusAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Users));
    }

    [HttpPost("users/{id}/change-role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeUserRole(string id, [FromForm] string newRole, [FromForm] int? storeId)
    {
        var (success, message) = await _adminService.ChangeUserRoleAsync(id, newRole, storeId);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Users));
    }

    // ─── Orders ───────────────────────────────────────────────────────────────
    [HttpGet("orders")]
    public async Task<IActionResult> Orders([FromQuery] OrderFilterDto filter)
    {
        var result = await _orderService.GetAllOrdersAsync(filter);
        ViewBag.Filter = filter;
        ViewData["AdminTitle"] = "Quản lý Đơn hàng toàn hệ thống";
        return View(result);
    }

    [HttpGet("orders/{id:int}")]
    public async Task<IActionResult> OrderDetail(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        ViewData["AdminTitle"] = $"Đơn hàng #{order.OrderCode}";
        return View(order);
    }

    [HttpPost("orders/{id:int}/update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromForm] OrderUpdateStatusDto dto)
    {
        dto.OrderId = id;
        var result = await _orderService.AdminUpdateOrderStatusAsync(dto);
        TempData[result.Success ? "Success" : "Error"] = result.Message ?? result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(OrderDetail), new { id });
    }

    // ─── Installations ────────────────────────────────────────────────────────
    [HttpGet("installations")]
    public async Task<IActionResult> Installations([FromQuery] int? storeId, [FromQuery] InstallationFilterDto filter)
    {
        var result = await _orderService.GetAllInstallationsAsync(storeId, filter);
        ViewBag.Filter = filter;
        ViewBag.SelectedStoreId = storeId;
        ViewBag.Stores = await _adminService.GetActiveStoresSimpleAsync();
        ViewData["AdminTitle"] = "Quản lý Lịch lắp đặt toàn hệ thống";
        return View(result);
    }

    [HttpPost("installations/{id:int}/update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateInstallationStatus(int id, [FromForm] DiemLanh.Domain.Enums.InstallationStatus status, [FromForm] string? note)
    {
        var result = await _orderService.AdminUpdateInstallationStatusAsync(id, status, note);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật trạng thái lắp đặt." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Installations));
    }

    // ─── Repairs ──────────────────────────────────────────────────────────────
    [HttpGet("repairs")]
    public async Task<IActionResult> Repairs([FromQuery] int? storeId, [FromQuery] RepairFilterDto filter)
    {
        var result = await _repairService.GetAllRepairRequestsAsync(storeId, filter);
        ViewBag.Filter = filter;
        ViewBag.SelectedStoreId = storeId;
        ViewBag.Stores = await _adminService.GetActiveStoresSimpleAsync();
        ViewData["AdminTitle"] = "Quản lý Yêu cầu sửa chữa toàn hệ thống";
        return View(result);
    }

    [HttpGet("repairs/{id:int}")]
    public async Task<IActionResult> RepairDetail(int id)
    {
        var repair = await _repairService.GetRepairRequestByIdAsync(id);
        if (repair == null) return NotFound();

        var techs = await _technicianService.GetTechniciansAsync(repair.StoreId);
        ViewBag.Technicians = techs;
        ViewData["AdminTitle"] = $"Sửa chữa #{repair.Id} - {repair.StoreName}";
        return View(repair);
    }

    [HttpPost("repairs/{id:int}/update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRepairStatus(int id, [FromForm] RepairStatusUpdateDto dto)
    {
        dto.RepairId = id;
        var result = await _repairService.AdminUpdateRepairStatusAsync(dto);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật trạng thái yêu cầu sửa chữa." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(RepairDetail), new { id });
    }

    // ─── Technicians ──────────────────────────────────────────────────────────
    [HttpGet("technicians")]
    public async Task<IActionResult> Technicians([FromQuery] int? storeId)
    {
        var techs = await _technicianService.GetAllTechniciansAsync(storeId);
        ViewBag.SelectedStoreId = storeId;
        ViewBag.Stores = await _adminService.GetActiveStoresSimpleAsync();
        ViewData["AdminTitle"] = "Quản lý Kỹ thuật viên";
        return View(techs);
    }

    [HttpPost("technicians/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTechnician(TechnicianCreateDto dto)
    {
        var result = await _technicianService.CreateTechnicianAsync(dto);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã thêm kỹ thuật viên thành công." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Technicians), new { storeId = dto.StoreId });
    }

    [HttpPost("technicians/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTechnician(int id)
    {
        var result = await _technicianService.AdminDeleteTechnicianAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa kỹ thuật viên." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Technicians));
    }

    // ─── Service Areas ────────────────────────────────────────────────────────
    [HttpGet("service-areas")]
    public async Task<IActionResult> ServiceAreas([FromQuery] int? storeId)
    {
        var areas = await _storeService.GetAllServiceAreasAsync(storeId);
        ViewBag.SelectedStoreId = storeId;
        ViewBag.Stores = await _adminService.GetActiveStoresSimpleAsync();
        ViewData["AdminTitle"] = "Quản lý Khu vực phục vụ";
        return View(areas);
    }

    [HttpPost("service-areas/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddServiceArea(StoreServiceAreaCreateDto dto)
    {
        var result = await _storeService.AddServiceAreaAsync(dto);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã thêm khu vực phục vụ." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(ServiceAreas), new { storeId = dto.StoreId });
    }

    [HttpPost("service-areas/{id:int}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveServiceArea(int id)
    {
        var result = await _storeService.AdminRemoveServiceAreaAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa khu vực phục vụ." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(ServiceAreas));
    }
}
