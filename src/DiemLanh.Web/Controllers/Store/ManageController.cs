using DiemLanh.Application.DTOs.Order;
using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.DTOs.Repair;
using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.Interfaces;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers.Store;

[Authorize(Roles = "StoreOwner,StoreStaff")]
[Route("manage/[action]")]
public class ManageController : Controller
{
    private readonly IStoreService _storeService;
    private readonly IDashboardService _dashboardService;
    private readonly IOrderService _orderService;
    private readonly IRepairService _repairService;
    private readonly ITechnicianService _technicianService;
    private readonly IProductService _productService;
    private readonly IFileStorageService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageController(
        IStoreService storeService,
        IDashboardService dashboardService,
        IOrderService orderService,
        IRepairService repairService,
        ITechnicianService technicianService,
        IProductService productService,
        IFileStorageService fileService,
        UserManager<ApplicationUser> userManager)
    {
        _storeService = storeService;
        _dashboardService = dashboardService;
        _orderService = orderService;
        _repairService = repairService;
        _technicianService = technicianService;
        _productService = productService;
        _fileService = fileService;
        _userManager = userManager;
    }

    private async Task<int> GetStoreIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.StoreId ?? 0;
    }

    // GET /manage/dashboard
    public async Task<IActionResult> Dashboard()
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0)
        {
            TempData["Error"] = "Tài khoản của bạn chưa được liên kết với cửa hàng nào. Vui lòng liên hệ Admin để được cấp quyền.";
            return View("NoStore");
        }

        var dashboard = await _dashboardService.GetStoreDashboardAsync(storeId);
        return View(dashboard);
    }

    // GET /manage/profile
    public async Task<IActionResult> Profile()
    {
        var storeId = await GetStoreIdAsync();
        var store = await _storeService.GetStoreByIdAsync(storeId);
        if (store == null) return NotFound();
        return View(store);
    }

    // POST /manage/profile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(StoreUpdateDto dto)
    {
        var storeId = await GetStoreIdAsync();
        if (!ModelState.IsValid) return View(dto);

        var result = await _storeService.UpdateStoreProfileAsync(storeId, dto);
        if (result.Success)
            TempData["Success"] = "Đã cập nhật thông tin cửa hàng";
        else
            TempData["Error"] = string.Join(", ", result.Errors);

        return RedirectToAction(nameof(Profile));
    }

    // POST /manage/upload-logo
    [HttpPost]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        var storeId = await GetStoreIdAsync();
        if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file");
        if (!_fileService.IsValidImageType(file.ContentType)) return BadRequest("Định dạng ảnh không hợp lệ");
        if (!_fileService.IsValidFileSize(file.Length)) return BadRequest("Kích thước ảnh vượt quá 5MB");

        await using var stream = file.OpenReadStream();
        var url = await _fileService.SaveFileAsync(stream, file.FileName, $"stores/{storeId}/logo");
        await _storeService.UpdateLogoAsync(storeId, url);

        return Json(new { success = true, url });
    }

    // POST /manage/upload-cover
    [HttpPost]
    public async Task<IActionResult> UploadCover(IFormFile file)
    {
        var storeId = await GetStoreIdAsync();
        if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file");
        if (!_fileService.IsValidImageType(file.ContentType)) return BadRequest("Định dạng ảnh không hợp lệ");

        await using var stream = file.OpenReadStream();
        var url = await _fileService.SaveFileAsync(stream, file.FileName, $"stores/{storeId}/cover");
        await _storeService.UpdateCoverAsync(storeId, url);

        return Json(new { success = true, url });
    }

    // GET /manage/service-areas
    [HttpGet("/manage/service-areas")]
    [HttpGet("/manage/serviceareas")]
    public async Task<IActionResult> ServiceAreas()
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        var areas = await _storeService.GetServiceAreasAsync(storeId);
        ViewBag.StoreId = storeId;
        return View(areas);
    }

    // POST /manage/service-areas/add
    [HttpPost("/manage/service-areas/add")]
    [HttpPost("/manage/addservicearea")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddServiceArea(StoreServiceAreaCreateDto dto)
    {
        var storeId = await GetStoreIdAsync();
        dto.StoreId = storeId;
        var result = await _storeService.AddServiceAreaAsync(dto);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã thêm khu vực" : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(ServiceAreas));
    }

    // POST /manage/service-areas/remove/{id}
    [HttpPost("/manage/service-areas/remove/{id:int}")]
    [HttpPost("/manage/removeservicearea/{id:int}")]
    public async Task<IActionResult> RemoveServiceArea(int id)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _storeService.RemoveServiceAreaAsync(id, storeId);
        return Json(new { success = result.Success });
    }

    // GET /manage/technicians
    [HttpGet("/manage/technicians")]
    public async Task<IActionResult> Technicians()
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        var techs = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.StoreId = storeId;
        return View(techs);
    }

    // POST /manage/technicians/add
    [HttpPost("/manage/technicians/add")]
    [HttpPost("/manage/addtechnician")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTechnician(TechnicianCreateDto dto)
    {
        var storeId = await GetStoreIdAsync();
        dto.StoreId = storeId;
        var result = await _technicianService.CreateTechnicianAsync(dto);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã thêm kỹ thuật viên" : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Technicians));
    }

    // POST /manage/technicians/{id}/delete
    [HttpPost("/manage/technicians/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTechnician(int id)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _technicianService.DeleteTechnicianAsync(id, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa kỹ thuật viên" : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Technicians));
    }


    // ─── Orders ──────────────────────────────────────────────────────────────────
    // GET /manage/orders
    public async Task<IActionResult> Orders([FromQuery] OrderFilterDto filter)
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        var result = await _orderService.GetOrdersAsync(storeId, filter);
        ViewBag.Filter = filter;
        return View(result);
    }

    // GET /manage/order-detail/{id}
    [HttpGet("/manage/order-detail/{id:int}")]
    public async Task<IActionResult> OrderDetail(int id)
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        var order = await _orderService.GetOrderByIdAsync(id, storeId);
        if (order == null) return NotFound();
        var techs = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Technicians = techs;
        return View(order);
    }

    // POST /manage/order-detail/{id}/update-status
    [HttpPost("/manage/order-detail/{id:int}/update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromForm] OrderUpdateStatusDto dto)
    {
        var storeId = await GetStoreIdAsync();
        dto.OrderId = id;
        var result = await _orderService.UpdateOrderStatusAsync(dto, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật trạng thái đơn hàng." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(OrderDetail), new { id });
    }

    // POST /manage/order-detail/{id}/schedule-install
    [HttpPost("/manage/order-detail/{id:int}/schedule-install")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleInstall(int id, [FromForm] InstallationScheduleDto dto)
    {
        var storeId = await GetStoreIdAsync();
        dto.OrderId = id;
        var result = await _orderService.ScheduleInstallationAsync(dto, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã lên lịch lắp đặt." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(OrderDetail), new { id });
    }

    // ─── Installations ────────────────────────────────────────────────────────────
    // GET /manage/installations
    public async Task<IActionResult> Installations([FromQuery] InstallationFilterDto filter)
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        var result = await _orderService.GetInstallationsAsync(storeId, filter);
        var techs = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Filter = filter;
        ViewBag.Technicians = techs;
        return View(result);
    }

    // GET /manage/installation-detail/{id}
    [HttpGet("/manage/installation-detail/{id:int}")]
    public async Task<IActionResult> InstallationDetail(int id)
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        var inst = await _orderService.GetInstallationByIdAsync(id, storeId);
        if (inst == null) return NotFound();
        var techs = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Technicians = techs;
        return View(inst);
    }

    // POST /manage/installation-detail/{id}/update-status
    [HttpPost("/manage/installation-detail/{id:int}/update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateInstallationStatus(int id, [FromForm] DiemLanh.Domain.Enums.InstallationStatus status, [FromForm] string? note)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _orderService.UpdateInstallationStatusAsync(id, status, note, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật trạng thái lịch lắp đặt." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(InstallationDetail), new { id });
    }

    // ─── Repairs ─────────────────────────────────────────────────────────────────
    // GET /manage/repairs
    public async Task<IActionResult> Repairs([FromQuery] RepairFilterDto filter)
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        var result = await _repairService.GetRepairRequestsAsync(storeId, filter);
        var techs = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Filter = filter;
        ViewBag.Technicians = techs;
        return View(result);
    }

    // GET /manage/repair-detail/{id}
    [HttpGet("/manage/repair-detail/{id:int}")]
    public async Task<IActionResult> RepairDetail(int id)
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        var repair = await _repairService.GetRepairRequestByIdAsync(id, storeId);
        if (repair == null) return NotFound();
        var techs = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Technicians = techs;
        return View(repair);
    }

    // POST /manage/repair-detail/{id}/update-status
    [HttpPost("/manage/repair-detail/{id:int}/update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRepairStatus(int id, [FromForm] RepairStatusUpdateDto dto)
    {
        var storeId = await GetStoreIdAsync();
        dto.RepairId = id;
        var result = await _repairService.UpdateRepairStatusAsync(dto, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật trạng thái." : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(RepairDetail), new { id });
    }

    // ─── Statistics ───────────────────────────────────────────────────────────────
    // GET /manage/statistics
    public async Task<IActionResult> Statistics(int year = 0)
    {
        var storeId = await GetStoreIdAsync();
        if (storeId == 0) return View("NoStore");
        if (year == 0) year = DateTime.Now.Year;
        var stats = await _dashboardService.GetStoreStatisticsAsync(storeId, year);
        ViewBag.Year = year;
        return View(stats);
    }
}
