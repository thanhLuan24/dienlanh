using System.Diagnostics;
using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DiemLanh.Web.Models;

namespace DiemLanh.Web.Controllers;

public class HomeController : Controller
{
    private readonly IStoreService _storeService;
    private readonly IProductService _productService;
    private readonly IProductCatalogService _catalogService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IStoreService storeService, 
        IProductService productService, 
        IProductCatalogService catalogService,
        ILogger<HomeController> logger)
    {
        _storeService = storeService;
        _productService = productService;
        _catalogService = catalogService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var storesResult = await _storeService.GetStoresAsync(new StoreFilterDto { PageSize = 6, SortBy = "rating" });
        var categories = await _productService.GetCategoriesAsync();
        var productsResult = await _catalogService.GetCatalogProductsAsync(new ProductCatalogFilterDto { PageSize = 6, SortBy = "best_seller" });

        ViewBag.FeaturedStores = storesResult.Items;
        ViewBag.Categories = categories;
        ViewBag.FeaturedProducts = productsResult.Items;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
