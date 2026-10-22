using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.Interfaces;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers.Store;

[Authorize(Roles = "StoreOwner,StoreStaff")]
[Route("manage/products/[action]")]
public class ProductManageController : Controller
{
    private readonly IProductService _productService;
    private readonly IFileStorageService _fileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductManageController(IProductService productService, IFileStorageService fileService, UserManager<ApplicationUser> userManager)
    {
        _productService = productService;
        _fileService = fileService;
        _userManager = userManager;
    }

    private async Task<int> GetStoreIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.StoreId ?? 0;
    }

    // GET /manage/products
    [HttpGet("/manage/products")]
    [HttpGet("/manage/products/index")]
    public async Task<IActionResult> Index([FromQuery] ProductFilterDto filter)
    {
        var storeId = await GetStoreIdAsync();
        var products = await _productService.GetProductsAsync(storeId, filter);
        var categories = await _productService.GetCategoriesAsync();
        ViewBag.Categories = categories;
        ViewBag.Filter = filter;
        return View(products);
    }

    // GET /manage/products/create
    [HttpGet("/manage/products/create")]
    public async Task<IActionResult> Create()
    {
        var storeId = await GetStoreIdAsync();
        var categories = await _productService.GetCategoriesAsync();
        ViewBag.Categories = categories;
        ViewBag.StoreId = storeId;
        return View(new ProductCreateDto { StoreId = storeId });
    }

    // POST /manage/products/create
    [HttpPost("/manage/products/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        var storeId = await GetStoreIdAsync();
        dto.StoreId = storeId;
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(dto);
        }

        var result = await _productService.CreateProductAsync(dto);
        if (result.Success)
        {
            TempData["Success"] = "Đã thêm sản phẩm thành công";
            return RedirectToAction(nameof(Edit), new { id = result.Data!.Id });
        }
        TempData["Error"] = result.Errors.FirstOrDefault();
        return View(dto);
    }

    // GET /manage/products/edit/{id}
    [HttpGet("/manage/products/edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var storeId = await GetStoreIdAsync();
        var product = await _productService.GetProductByIdAsync(id, storeId);
        if (product == null) return NotFound();

        var categories = await _productService.GetCategoriesAsync();
        ViewBag.Categories = categories;

        var dto = new ProductUpdateDto
        {
            Id = product.Id,
            StoreId = storeId,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Description = product.Description,
            Brand = product.Brand,
            Model = product.Model,
            Price = product.Price,
            SalePrice = product.SalePrice,
            Stock = product.Stock,
            WarrantyMonths = product.WarrantyMonths,
            IsFeatured = product.IsFeatured,
            IsHidden = product.IsHidden,
            Status = product.Status,
            Specifications = product.Specifications
        };
        ViewBag.Product = product;
        return View(dto);
    }

    // POST /manage/products/edit/{id}
    [HttpPost("/manage/products/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductUpdateDto dto)
    {
        var storeId = await GetStoreIdAsync();
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(dto);
        }

        var result = await _productService.UpdateProductAsync(dto, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã cập nhật sản phẩm" : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Edit), new { id = dto.Id });
    }

    // POST /manage/products/toggle-hidden/{id}
    [HttpPost("/manage/products/toggle-hidden/{id:int}")]
    public async Task<IActionResult> ToggleHidden(int id)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _productService.ToggleHiddenAsync(id, storeId);
        return Json(new { success = result.Success, message = result.Message });
    }

    // POST /manage/products/delete/{id}
    [HttpPost("/manage/products/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _productService.DeleteProductAsync(id, storeId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa sản phẩm" : result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(Index));
    }

    // POST /manage/products/upload-images
    [HttpPost("/manage/products/upload-images")]
    public async Task<IActionResult> UploadImages(int productId, IFormFileCollection files)
    {
        var storeId = await GetStoreIdAsync();
        var urls = new List<string>();

        foreach (var file in files)
        {
            if (!_fileService.IsValidImageType(file.ContentType)) continue;
            if (!_fileService.IsValidFileSize(file.Length)) continue;

            await using var stream = file.OpenReadStream();
            var url = await _fileService.SaveFileAsync(stream, file.FileName, $"products/{productId}");
            urls.Add(url);
        }

        if (urls.Any())
            await _productService.AddProductImagesAsync(productId, urls, storeId);

        return Json(new { success = true, urls });
    }

    // POST /manage/products/remove-image
    [HttpPost("/manage/products/remove-image")]
    public async Task<IActionResult> RemoveImage(int imageId)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _productService.RemoveProductImageAsync(imageId, storeId);
        return Json(new { success = result.Success });
    }

    // POST /manage/products/update-stock
    [HttpPost("/manage/products/update-stock")]
    public async Task<IActionResult> UpdateStock(int id, int stock)
    {
        var storeId = await GetStoreIdAsync();
        var result = await _productService.UpdateStockAsync(id, stock, storeId);
        return Json(new { success = result.Success });
    }
}
