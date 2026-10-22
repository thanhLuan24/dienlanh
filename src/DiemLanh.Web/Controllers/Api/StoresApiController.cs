using DiemLanh.Application.DTOs.Repair;
using DiemLanh.Application.DTOs.Store;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StoresApiController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly IOrderService _orderService;
    private readonly IRepairService _repairService;
    private readonly IDashboardService _dashboardService;
    private readonly IProductService _productService;
    private readonly IFileStorageService _fileService;

    public StoresApiController(
        IStoreService storeService,
        IOrderService orderService,
        IRepairService repairService,
        IDashboardService dashboardService,
        IProductService productService,
        IFileStorageService fileService)
    {
        _storeService = storeService;
        _orderService = orderService;
        _repairService = repairService;
        _dashboardService = dashboardService;
        _productService = productService;
        _fileService = fileService;
    }

    /// <summary>GET /api/stores – Public store listing with filters (Chatbot-compatible)</summary>
    [HttpGet]
    public async Task<IActionResult> GetStores([FromQuery] StoreFilterDto filter)
    {
        var result = await _storeService.GetStoresAsync(filter);
        return Ok(result);
    }

    /// <summary>GET /api/stores/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetStore(int id)
    {
        var store = await _storeService.GetStoreByIdAsync(id);
        return store == null ? NotFound() : Ok(store);
    }

    /// <summary>GET /api/stores/by-province/{province} – For chatbot to find nearby stores</summary>
    [HttpGet("by-province/{province}")]
    public async Task<IActionResult> GetByProvince(string province)
    {
        var stores = await _storeService.GetStoresByProvinceAsync(province);
        return Ok(stores);
    }

    /// <summary>GET /api/stores/nearby – GPS-based store discovery</summary>
    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double radius = 20)
    {
        var stores = await _storeService.GetNearbyStoresAsync(lat, lon, radius);
        return Ok(stores);
    }

    /// <summary>POST /api/stores – Create store (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateStore([FromBody] StoreCreateDto dto)
    {
        var result = await _storeService.CreateStoreAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetStore), new { id = result.Data!.Id }, result.Data) : BadRequest(result.Errors);
    }

    /// <summary>PUT /api/stores/{id}/status – Update store status (Admin only)</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StoreStatus status)
    {
        var result = await _storeService.UpdateStoreStatusAsync(id, status);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }

    /// <summary>GET /api/stores/{id}/service-areas</summary>
    [HttpGet("{id:int}/service-areas")]
    public async Task<IActionResult> GetServiceAreas(int id)
    {
        var areas = await _storeService.GetServiceAreasAsync(id);
        return Ok(areas);
    }

    /// <summary>POST /api/stores/{id}/service-areas</summary>
    [HttpPost("{id:int}/service-areas")]
    [Authorize(Roles = "SuperAdmin,StoreOwner")]
    public async Task<IActionResult> AddServiceArea(int id, [FromBody] StoreServiceAreaCreateDto dto)
    {
        dto.StoreId = id;
        var result = await _storeService.AddServiceAreaAsync(dto);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    /// <summary>DELETE /api/stores/{id}/service-areas/{areaId}</summary>
    [HttpDelete("{id:int}/service-areas/{areaId:int}")]
    [Authorize(Roles = "SuperAdmin,StoreOwner")]
    public async Task<IActionResult> RemoveServiceArea(int id, int areaId)
    {
        var result = await _storeService.RemoveServiceAreaAsync(areaId, id);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }

    /// <summary>GET /api/stores/{id}/dashboard</summary>
    [HttpGet("{id:int}/dashboard")]
    [Authorize(Roles = "SuperAdmin,StoreOwner,StoreStaff")]
    public async Task<IActionResult> GetDashboard(int id)
    {
        var data = await _dashboardService.GetStoreDashboardAsync(id);
        return Ok(data);
    }

    /// <summary>GET /api/stores/{id}/stats</summary>
    [HttpGet("{id:int}/stats")]
    [Authorize(Roles = "SuperAdmin,StoreOwner")]
    public async Task<IActionResult> GetStats(int id, [FromQuery] int year = 0, [FromQuery] int? month = null)
    {
        if (year == 0) year = DateTime.Now.Year;
        var stats = await _dashboardService.GetStoreStatisticsAsync(id, year, month);
        return Ok(stats);
    }

    /// <summary>GET /api/stores/{id}/orders</summary>
    [HttpGet("{id:int}/orders")]
    [Authorize(Roles = "SuperAdmin,StoreOwner,StoreStaff")]
    public async Task<IActionResult> GetOrders(int id, [FromQuery] Application.DTOs.Order.OrderFilterDto filter)
    {
        var orders = await _orderService.GetOrdersAsync(id, filter);
        return Ok(orders);
    }

    /// <summary>GET /api/stores/{id}/installations</summary>
    [HttpGet("{id:int}/installations")]
    [Authorize(Roles = "SuperAdmin,StoreOwner,StoreStaff,Technician")]
    public async Task<IActionResult> GetInstallations(int id, [FromQuery] Application.DTOs.Order.InstallationFilterDto filter)
    {
        var items = await _orderService.GetInstallationsAsync(id, filter);
        return Ok(items);
    }

    /// <summary>GET /api/stores/{id}/repairs</summary>
    [HttpGet("{id:int}/repairs")]
    [Authorize(Roles = "SuperAdmin,StoreOwner,StoreStaff")]
    public async Task<IActionResult> GetRepairs(int id, [FromQuery] Application.DTOs.Repair.RepairFilterDto filter)
    {
        var items = await _repairService.GetRepairRequestsAsync(id, filter);
        return Ok(items);
    }

    /// <summary>POST /api/repairs – Create repair request (public, from chatbot or website)</summary>
    [HttpPost("/api/repairs")]
    public async Task<IActionResult> CreateRepair([FromBody] RepairRequestCreateDto dto)
    {
        var customerId = User.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous" : "anonymous";
        var result = await _repairService.CreateRepairRequestAsync(dto, customerId);
        return result.Success ? Ok(result.Data) : BadRequest(result.Errors);
    }

    /// <summary>PUT /api/repairs/{id}/status</summary>
    [HttpPut("/api/repairs/{id:int}/status")]
    [Authorize(Roles = "SuperAdmin,StoreOwner,StoreStaff")]
    public async Task<IActionResult> UpdateRepairStatus(int id, [FromBody] RepairStatusUpdateDto dto, [FromQuery] int storeId)
    {
        dto.RepairId = id;
        var result = await _repairService.UpdateRepairStatusAsync(dto, storeId);
        return result.Success ? Ok() : BadRequest(result.Errors);
    }

    /// <summary>POST /api/upload/image</summary>
    [HttpPost("/api/upload/image")]
    [Authorize]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "general")
    {
        if (file == null || file.Length == 0) return BadRequest("No file provided");
        if (!_fileService.IsValidImageType(file.ContentType)) return BadRequest("Invalid image type");
        if (!_fileService.IsValidFileSize(file.Length)) return BadRequest("File too large (max 5MB)");

        await using var stream = file.OpenReadStream();
        var url = await _fileService.SaveFileAsync(stream, file.FileName, folder);
        return Ok(new { url });
    }
}
