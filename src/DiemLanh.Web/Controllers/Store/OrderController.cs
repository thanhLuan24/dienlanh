using DiemLanh.Application.DTOs.Order;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers.Store;

[Authorize(Roles = "StoreOwner,StoreStaff")]
[Route("manage/orders/[action]")]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ITechnicianService _technicianService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(IOrderService orderService, ITechnicianService technicianService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _technicianService = technicianService;
        _userManager = userManager;
    }

    private async Task<int> GetStoreIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.StoreId ?? 0;
    }

    // GET /manage/orders/index
    public async Task<IActionResult> Index([FromQuery] OrderFilterDto filter)
    {
        var storeId = await GetStoreIdAsync();
        var orders = await _orderService.GetOrdersAsync(storeId, filter);
        ViewBag.Filter = filter;
        return View(orders);
    }

    // GET /manage/orders/detail/{id}
    public async Task<IActionResult> Detail(int id)
    {
        var storeId = await GetStoreIdAsync();
        var order = await _orderService.GetOrderByIdAsync(id, storeId);
        if (order == null) return NotFound();

        var technicians = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Technicians = technicians;
        return View(order);
    }

    // POST /manage/orders/update-status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(OrderUpdateStatusDto dto)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _orderService.UpdateOrderStatusAsync(dto, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Message ?? result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Detail), new { id = dto.OrderId });
    }

    // POST /manage/orders/schedule-installation
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScheduleInstallation(InstallationScheduleDto dto)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _orderService.ScheduleInstallationAsync(dto, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã lên lịch lắp đặt" : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Detail), new { id = dto.OrderId });
    }
}

[Authorize(Roles = "StoreOwner,StoreStaff,Technician")]
[Route("manage/installations/[action]")]
public class InstallationController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ITechnicianService _technicianService;
    private readonly UserManager<ApplicationUser> _userManager;

    public InstallationController(IOrderService orderService, ITechnicianService technicianService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _technicianService = technicianService;
        _userManager = userManager;
    }

    private async Task<int> GetStoreIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.StoreId ?? 0;
    }

    // GET /manage/installations/index
    public async Task<IActionResult> Index([FromQuery] InstallationFilterDto filter)
    {
        var storeId = await GetStoreIdAsync();
        var installations = await _orderService.GetInstallationsAsync(storeId, filter);
        var technicians = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Technicians = technicians;
        ViewBag.Filter = filter;
        return View(installations);
    }

    // GET /manage/installations/detail/{id}
    public async Task<IActionResult> Detail(int id)
    {
        var storeId = await GetStoreIdAsync();
        var installation = await _orderService.GetInstallationByIdAsync(id, storeId);
        if (installation == null) return NotFound();
        return View(installation);
    }

    // POST /manage/installations/update-status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, InstallationStatus status, string? note)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _orderService.UpdateInstallationStatusAsync(id, status, note, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Message ?? result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Detail), new { id });
    }

    // POST /manage/installations/reschedule
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(InstallationRescheduleDto dto)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _orderService.RescheduleInstallationAsync(dto, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Message ?? result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Detail), new { id = dto.InstallationId });
    }
}
