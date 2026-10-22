using DiemLanh.Application.DTOs.Admin;
using DiemLanh.Application.DTOs.Order;
using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.DTOs.Repair;
using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
[Route("admin/stores")]
public class AdminStoreController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IStoreService _storeService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IRepairService _repairService;
    private readonly ITechnicianService _technicianService;

    public AdminStoreController(
        IAdminService adminService,
        IStoreService storeService,
        IProductService productService,
        IOrderService orderService,
        IRepairService repairService,
        ITechnicianService technicianService)
    {
        _adminService = adminService;
        _storeService = storeService;
        _productService = productService;
        _orderService = orderService;
        _repairService = repairService;
        _technicianService = technicianService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] string? keyword, [FromQuery] string? status, [FromQuery] int page = 1)
    {
        var result = await _adminService.GetStoresAsync(keyword, status, page, 20);
        ViewBag.Keyword = keyword;
        ViewBag.Status = status;
        return View(result);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new AdminStoreCreateDto());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminStoreCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var (success, message) = await _adminService.CreateStoreAsync(dto);
        if (success) { TempData["Success"] = message; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", message);
        return View(dto);
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _adminService.DeleteStoreAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/toggle-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var (success, message) = await _adminService.ToggleStoreStatusAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    // ─── Single Store Hub ────────────────────────────────────────────────────────
    [HttpGet("{id:int}/manage")]
    public async Task<IActionResult> Manage(int id, [FromQuery] string tab = "info")
    {
        var store = await _storeService.GetStoreByIdAsync(id);
        if (store == null) return NotFound();

        ViewBag.Tab = tab;
        ViewBag.Store = store;
        ViewBag.ServiceAreas = await _storeService.GetServiceAreasAsync(id);
        ViewBag.Technicians = await _technicianService.GetTechniciansAsync(id);
        ViewBag.Products = await _productService.GetProductsAsync(id, new ProductFilterDto { PageSize = 100 });
        ViewBag.Orders = await _orderService.GetOrdersAsync(id, new OrderFilterDto { PageSize = 100 });
        ViewBag.Repairs = await _repairService.GetRepairRequestsAsync(id, new RepairFilterDto { PageSize = 100 });
        ViewBag.Installations = await _orderService.GetInstallationsAsync(id, new InstallationFilterDto { PageSize = 100 });

        ViewData["AdminTitle"] = $"Quản lý Cửa hàng: {store.Name}";
        return View(store);
    }

    [HttpPost("{id:int}/update-profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(int id, StoreUpdateDto dto)
    {
        dto.Id = id;
        var result = await _storeService.UpdateStoreProfileAsync(id, dto);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật thông tin cửa hàng." : string.Join(", ", result.Errors);
        return RedirectToAction(nameof(Manage), new { id, tab = "info" });
    }

    [HttpPost("{id:int}/add-service-area")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddServiceArea(int id, StoreServiceAreaCreateDto dto)
    {
        dto.StoreId = id;
        var result = await _storeService.AddServiceAreaAsync(dto);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã thêm khu vực phục vụ." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Manage), new { id, tab = "areas" });
    }

    [HttpPost("{id:int}/remove-service-area/{areaId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveServiceArea(int id, int areaId)
    {
        var result = await _storeService.RemoveServiceAreaAsync(areaId, id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa khu vực phục vụ." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Manage), new { id, tab = "areas" });
    }

    [HttpPost("{id:int}/add-technician")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTechnician(int id, TechnicianCreateDto dto)
    {
        dto.StoreId = id;
        var result = await _technicianService.CreateTechnicianAsync(dto);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã thêm kỹ thuật viên." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Manage), new { id, tab = "techs" });
    }

    [HttpPost("{id:int}/delete-technician/{techId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTechnician(int id, int techId)
    {
        var result = await _technicianService.DeleteTechnicianAsync(techId, id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa kỹ thuật viên." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Manage), new { id, tab = "techs" });
    }
}
