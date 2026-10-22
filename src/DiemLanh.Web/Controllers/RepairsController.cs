using DiemLanh.Application.DTOs.Repair;
using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.Interfaces;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

public class RepairsController : Controller
{
    private readonly IRepairService _repairService;
    private readonly IStoreService _storeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public RepairsController(IRepairService repairService, IStoreService storeService, UserManager<ApplicationUser> userManager, IFileStorageService fileStorageService)
    {
        _repairService = repairService;
        _storeService = storeService;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
    }

    // GET /repairs/book?storeId=X
    [HttpGet("repairs/book")]
    public async Task<IActionResult> Book([FromQuery] int? storeId)
    {
        var storesResult = await _storeService.GetStoresAsync(new StoreFilterDto { PageSize = 50, Status = Domain.Enums.StoreStatus.Active });
        ViewBag.Stores = storesResult.Items;
        ViewBag.SelectedStoreId = storeId;

        var user = await _userManager.GetUserAsync(User);
        var dto = new RepairRequestCreateDto();
        if (user != null)
        {
            dto.CustomerName = user.FullName ?? "";
            dto.CustomerPhone = user.PhoneNumber ?? "";
        }
        if (storeId.HasValue)
        {
            dto.StoreId = storeId.Value;
        }

        return View(dto);
    }

    // POST /repairs/book
    [HttpPost("repairs/book")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(RepairRequestCreateDto dto, List<IFormFile>? Images)
    {
        var user = await _userManager.GetUserAsync(User);
        var customerId = user?.Id ?? "anonymous";

        if (!ModelState.IsValid || dto.StoreId == 0)
        {
            if (dto.StoreId == 0) ModelState.AddModelError("StoreId", "Vui lòng chọn cửa hàng sửa chữa.");
            var storesResult = await _storeService.GetStoresAsync(new StoreFilterDto { PageSize = 50, Status = Domain.Enums.StoreStatus.Active });
            ViewBag.Stores = storesResult.Items;
            ViewBag.SelectedStoreId = dto.StoreId;
            return View(dto);
        }

        var result = await _repairService.CreateRepairRequestAsync(dto, customerId);
        if (result.Success)
        {
            // Process Image uploads if any
            if (Images != null && Images.Count > 0)
            {
                var imageUrls = new List<string>();
                foreach (var file in Images.Take(3)) // Max 3 images
                {
                    if (file.Length > 0 && _fileStorageService.IsValidImageType(file.ContentType))
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var savedPath = await _fileStorageService.SaveFileAsync(file.OpenReadStream(), fileName, "repairs");
                        imageUrls.Add(_fileStorageService.GetPublicUrl(savedPath));
                    }
                }

                if (imageUrls.Count > 0)
                {
                    await _repairService.AddRepairImagesAsync(result.Data!.Id, imageUrls, dto.StoreId);
                }
            }

            TempData["Success"] = $"Đặt lịch sửa chữa #{result.Data!.Id} thành công! Cửa hàng sẽ liên hệ lại với bạn trong thời gian sớm nhất.";
            if (user != null)
                return RedirectToAction(nameof(CustomerRepairDetail), new { id = result.Data.Id });
            else
                return RedirectToAction("Index", "Home");
        }

        TempData["Error"] = result.Errors.FirstOrDefault() ?? "Không thể đặt lịch sửa chữa.";
        var stores = await _storeService.GetStoresAsync(new StoreFilterDto { PageSize = 50, Status = Domain.Enums.StoreStatus.Active });
        ViewBag.Stores = stores.Items;
        ViewBag.SelectedStoreId = dto.StoreId;
        return View(dto);
    }

    // GET /auth/repairs
    [HttpGet("auth/repairs")]
    [Authorize]
    public async Task<IActionResult> CustomerRepairs()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var list = await _repairService.GetCustomerRepairsAsync(user.Id);
        return View("~/Views/Account/Repairs.cshtml", list);
    }

    // GET /auth/repairs/{id:int}
    [HttpGet("auth/repairs/{id:int}")]
    [Authorize]
    public async Task<IActionResult> CustomerRepairDetail(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var repair = await _repairService.GetRepairRequestByIdForCustomerAsync(id, user.Id);
        if (repair == null) return NotFound();

        return View("~/Views/Account/RepairDetail.cshtml", repair);
    }
    // POST /auth/repairs/{id:int}/cancel
    [HttpPost("auth/repairs/{id:int}/cancel")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelCustomerRepair(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var result = await _repairService.CancelCustomerRepairAsync(id, user.Id);
        if (result.Success)
            TempData["Success"] = result.Message ?? "Đã hủy yêu cầu sửa chữa.";
        else
            TempData["Error"] = result.Errors.FirstOrDefault() ?? result.Message ?? "Không thể hủy yêu cầu sửa chữa.";

        return RedirectToAction(nameof(CustomerRepairs));
    }
}
