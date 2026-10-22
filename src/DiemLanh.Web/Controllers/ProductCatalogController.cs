using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

[Route("products")]
public class ProductCatalogController : Controller
{
    private readonly IProductCatalogService _catalogService;
    private readonly IProductService _productService;
    private readonly ILogger<ProductCatalogController> _logger;
    private readonly IFileStorageService _fileStorageService;

    public ProductCatalogController(
        IProductCatalogService catalogService, 
        IProductService productService,
        ILogger<ProductCatalogController> logger,
        IFileStorageService fileStorageService)
    {
        _catalogService = catalogService;
        _productService = productService;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    // GET: /products or /products?CategorySlug=dieu-hoa
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ProductCatalogFilterDto filter)
    {
        var result = await _catalogService.GetCatalogProductsAsync(filter);
        var categories = await _productService.GetCategoriesAsync();
        var brands = await _catalogService.GetAllBrandsAsync();

        ViewBag.Filter = filter;
        ViewBag.Categories = categories;
        ViewBag.Brands = brands;
        return View(result);
    }

    // GET: /products/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var product = await _catalogService.GetProductDetailBySlugAsync(slug);
        if (product == null)
        {
            return NotFound();
        }

        var relatedProducts = await _catalogService.GetRelatedProductsAsync(product.Id, 6);
        var sameBrandProducts = product.BrandId.HasValue 
            ? await _catalogService.GetSameBrandProductsAsync(product.BrandId.Value, product.Id, 6) 
            : new List<ProductCardDto>();

        ViewBag.RelatedProducts = relatedProducts;
        ViewBag.SameBrandProducts = sameBrandProducts;
        return View(product);
    }

    // GET: /products/compare?ids=1,2,3
    [HttpGet("compare")]
    public async Task<IActionResult> Compare([FromQuery] string? ids)
    {
        var productIds = new List<int>();
        if (!string.IsNullOrEmpty(ids))
        {
            productIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => int.TryParse(x.Trim(), out var id) ? id : 0)
                            .Where(x => x > 0)
                            .Distinct()
                            .Take(4)
                            .ToList();
        }

        var compareData = await _catalogService.CompareProductsAsync(productIds);
        return View(compareData);
    }

    // GET: /products/quick-view/{id}
    [HttpGet("quick-view/{id:int}")]
    public async Task<IActionResult> QuickView(int id)
    {
        var product = await _catalogService.GetQuickViewAsync(id);
        if (product == null) return NotFound();
        return PartialView("_QuickViewModal", product);
    }

    // GET: /products/autocomplete?q=daikin
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string q)
    {
        var results = await _catalogService.AutocompleteSearchAsync(q);
        return Json(results);
    }

    // POST: /products/reviews
    [HttpPost("reviews")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview([FromForm] ProductReviewCreateDto dto, List<IFormFile>? Images)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        if (Images != null && Images.Count > 0)
        {
            foreach (var file in Images.Take(3))
            {
                if (file.Length > 0 && _fileStorageService.IsValidImageType(file.ContentType))
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var savedPath = await _fileStorageService.SaveFileAsync(file.OpenReadStream(), fileName, "reviews");
                    dto.ImageUrls.Add(_fileStorageService.GetPublicUrl(savedPath));
                }
            }
        }

        var customerName = User.Identity?.IsAuthenticated == true ? User.Identity.Name! : "Khách hàng";
        var customerId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "guest" : "guest";

        var result = await _catalogService.AddProductReviewAsync(customerId, customerName, dto);
        return Json(new { success = result.Success, message = result.Message, data = result.Data });
    }

    // GET: /products/ai-context/{id}
    [HttpGet("ai-context/{id:int}")]
    public async Task<IActionResult> GetAiContext(int id)
    {
        var context = await _catalogService.GetAiProductContextAsync(id);
        return Json(context);
    }
}
