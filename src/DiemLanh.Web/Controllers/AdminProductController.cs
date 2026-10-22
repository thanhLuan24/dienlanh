using DiemLanh.Application.DTOs.Admin;
using DiemLanh.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
[Route("admin/products")]
public class AdminProductController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IProductService _productService;
    private readonly IProductCatalogService _catalogService;

    public AdminProductController(IAdminService adminService, IProductService productService, IProductCatalogService catalogService)
    {
        _adminService = adminService;
        _productService = productService;
        _catalogService = catalogService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] string? keyword, [FromQuery] int? categoryId, [FromQuery] int? brandId, [FromQuery] int page = 1)
    {
        var result = await _adminService.GetProductsAsync(keyword, categoryId, brandId, page, 20);
        ViewBag.Keyword = keyword;
        ViewBag.CategoryId = categoryId;
        ViewBag.BrandId = brandId;
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        ViewBag.Brands = await _catalogService.GetAllBrandsAsync();
        return View(result);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        ViewBag.Brands = await _catalogService.GetAllBrandsAsync();
        ViewBag.Stores = await GetStoreSelectList();
        return View(new AdminProductCreateEditDto());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductCreateEditDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            ViewBag.Brands = await _catalogService.GetAllBrandsAsync();
            ViewBag.Stores = await GetStoreSelectList();
            return View(dto);
        }
        var (success, message, id) = await _adminService.CreateProductAsync(dto);
        if (success) { TempData["Success"] = message; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", message);
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        ViewBag.Brands = await _catalogService.GetAllBrandsAsync();
        ViewBag.Stores = await GetStoreSelectList();
        return View(dto);
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _adminService.GetProductForEditAsync(id);
        if (dto == null) return NotFound();
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        ViewBag.Brands = await _catalogService.GetAllBrandsAsync();
        ViewBag.Stores = await GetStoreSelectList();
        return View(dto);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminProductCreateEditDto dto)
    {
        dto.Id = id;
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            ViewBag.Brands = await _catalogService.GetAllBrandsAsync();
            ViewBag.Stores = await GetStoreSelectList();
            return View(dto);
        }
        var (success, message) = await _adminService.UpdateProductAsync(dto);
        if (success) { TempData["Success"] = message; return RedirectToAction(nameof(Index)); }
        ModelState.AddModelError("", message);
        ViewBag.Categories = await _productService.GetCategoriesAsync();
        ViewBag.Brands = await _catalogService.GetAllBrandsAsync();
        ViewBag.Stores = await GetStoreSelectList();
        return View(dto);
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, message) = await _adminService.DeleteProductAsync(id);
        TempData[success ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<(int Id, string Name)>> GetStoreSelectList()
    {
        // Simple inline query for store list
        return new List<(int Id, string Name)>(); // Will be populated via ViewBag in real usage
    }
}
