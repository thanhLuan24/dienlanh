using DiemLanh.Application.DTOs.Product;
using DiemLanh.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public class ProductsApiController : ControllerBase
{
    private readonly IProductCatalogService _catalogService;

    public ProductsApiController(IProductCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Search and Filter Catalog Products with faceted filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductCatalogFilterDto filter)
    {
        var result = await _catalogService.GetCatalogProductsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Get full Product details by Slug or ID
    /// </summary>
    [HttpGet("{idOrSlug}")]
    public async Task<IActionResult> GetProductDetail(string idOrSlug)
    {
        if (int.TryParse(idOrSlug, out var id))
        {
            var p = await _catalogService.GetProductDetailByIdAsync(id);
            if (p == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });
            return Ok(p);
        }
        else
        {
            var p = await _catalogService.GetProductDetailBySlugAsync(idOrSlug);
            if (p == null) return NotFound(new { message = "Không tìm thấy sản phẩm." });
            return Ok(p);
        }
    }

    /// <summary>
    /// Autocomplete query search for instantaneous UI lookup
    /// </summary>
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete([FromQuery] string q)
    {
        var results = await _catalogService.AutocompleteSearchAsync(q);
        return Ok(results);
    }

    /// <summary>
    /// Compare up to 4 products by IDs
    /// </summary>
    [HttpGet("compare")]
    public async Task<IActionResult> Compare([FromQuery] string ids)
    {
        var idList = ids?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => int.TryParse(x, out var i) ? i : 0)
                         .Where(x => x > 0).ToList() ?? new List<int>();

        var result = await _catalogService.CompareProductsAsync(idList);
        return Ok(result);
    }

    /// <summary>
    /// RAG AI System Prompt Context Endpoint for specific product
    /// </summary>
    [HttpGet("{id:int}/ai-context")]
    public async Task<IActionResult> GetAiContext(int id)
    {
        var context = await _catalogService.GetAiProductContextAsync(id);
        return Ok(context);
    }
}
