using DiemLanh.Application.DTOs.Repair;
using DiemLanh.Application.Interfaces;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers.Store;

[Authorize(Roles = "StoreOwner,StoreStaff")]
[Route("manage/repairs/[action]")]
public class RepairController : Controller
{
    private readonly IRepairService _repairService;
    private readonly ITechnicianService _technicianService;
    private readonly IFileStorageService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RepairController(IRepairService repairService, ITechnicianService technicianService, IFileStorageService fileService, UserManager<ApplicationUser> userManager)
    {
        _repairService = repairService;
        _technicianService = technicianService;
        _fileService = fileService;
        _userManager = userManager;
    }

    private async Task<int> GetStoreIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.StoreId ?? 0;
    }

    // GET /manage/repairs/index
    public async Task<IActionResult> Index([FromQuery] RepairFilterDto filter)
    {
        var storeId = await GetStoreIdAsync();
        var repairs = await _repairService.GetRepairRequestsAsync(storeId, filter);
        var technicians = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Technicians = technicians;
        ViewBag.Filter = filter;
        return View(repairs);
    }

    // GET /manage/repairs/detail/{id}
    public async Task<IActionResult> Detail(int id)
    {
        var storeId = await GetStoreIdAsync();
        var repair = await _repairService.GetRepairRequestByIdAsync(id, storeId);
        if (repair == null) return NotFound();

        var technicians = await _technicianService.GetTechniciansAsync(storeId);
        ViewBag.Technicians = technicians;
        return View(repair);
    }

    // POST /manage/repairs/update-status
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(RepairStatusUpdateDto dto)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _repairService.UpdateRepairStatusAsync(dto, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Message ?? result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Detail), new { id = dto.RepairId });
    }

    // POST /manage/repairs/upload-images
    [HttpPost]
    public async Task<IActionResult> UploadImages(int repairId, IFormFileCollection files)
    {
        var storeId = await GetStoreIdAsync();
        var urls = new List<string>();

        foreach (var file in files)
        {
            if (!_fileService.IsValidImageType(file.ContentType)) continue;
            await using var stream = file.OpenReadStream();
            var url = await _fileService.SaveFileAsync(stream, file.FileName, $"repairs/{repairId}");
            urls.Add(url);
        }

        await _repairService.AddRepairImagesAsync(repairId, urls, storeId);
        return Json(new { success = true, urls });
    }
}
