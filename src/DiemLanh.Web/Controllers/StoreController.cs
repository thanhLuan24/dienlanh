using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

[Route("stores")]
public class StoreController : Controller
{
    private readonly IStoreService _storeService;
    private readonly IProductService _productService;
    private readonly ILogger<StoreController> _logger;

    public StoreController(IStoreService storeService, IProductService productService, ILogger<StoreController> logger)
    {
        _storeService = storeService;
        _productService = productService;
        _logger = logger;
    }

    // GET /stores
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] StoreFilterDto filter)
    {
        var result = await _storeService.GetStoresAsync(filter);
        var categories = await _productService.GetCategoriesAsync();
        ViewBag.Categories = categories;
        ViewBag.Filter = filter;
        return View(result);
    }

    // GET /stores/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        var store = await _storeService.GetStoreBySlugAsync(slug);
        if (store == null) return NotFound();

        // Get products for this store
        var products = await _productService.GetProductsAsync(store.Id, new Application.DTOs.Product.ProductFilterDto
        {
            PageSize = 8, SortBy = "sold", SortDesc = true
        });
        ViewBag.Products = products;

        return View(store);
    }
}
