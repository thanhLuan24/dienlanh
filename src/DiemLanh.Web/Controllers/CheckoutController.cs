using DiemLanh.Application.DTOs.Order;
using DiemLanh.Application.Interfaces;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

[Route("checkout")]
public class CheckoutController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IProductCatalogService _catalogService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CheckoutController(
        IOrderService orderService,
        IProductCatalogService catalogService,
        UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _catalogService = catalogService;
        _userManager = userManager;
    }

    // GET /checkout?productId=X&quantity=1
    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> Index(int productId, int quantity = 1)
    {
        if (productId <= 0) return RedirectToAction("Index", "Home");
        if (quantity < 1) quantity = 1;

        var product = await _catalogService.GetProductDetailByIdAsync(productId);
        if (product == null) return NotFound();

        // Pre-fill from logged-in user
        var user = await _userManager.GetUserAsync(User);

        var dto = new OrderPlaceDto
        {
            ProductId = productId,
            Quantity = quantity,
            CustomerName = user?.FullName ?? "",
            CustomerPhone = user?.PhoneNumber ?? "",
            CustomerEmail = user?.Email ?? ""
        };

        ViewBag.Product = product;
        ViewBag.Quantity = quantity;
        return View(dto);
    }

    // POST /checkout/place
    [HttpPost("place")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(OrderPlaceDto dto)
    {
        if (!ModelState.IsValid)
        {
            var product = await _catalogService.GetProductDetailByIdAsync(dto.ProductId);
            ViewBag.Product = product;
            ViewBag.Quantity = dto.Quantity;
            return View("Index", dto);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var result = await _orderService.PlaceOrderAsync(dto, user.Id);
        if (!result.Success)
        {
            var product = await _catalogService.GetProductDetailByIdAsync(dto.ProductId);
            ViewBag.Product = product;
            ViewBag.Quantity = dto.Quantity;
            ModelState.AddModelError("", result.Message ?? result.Errors.FirstOrDefault() ?? "Đặt hàng thất bại.");
            return View("Index", dto);
        }

        TempData["OrderCode"] = result.Data!.OrderCode;
        TempData["OrderId"] = result.Data!.Id;
        return RedirectToAction(nameof(Success), new { code = result.Data!.OrderCode });
    }

    // GET /checkout/success?code=CC-XXXXXXXX
    [HttpGet("success")]
    [Authorize]
    public async Task<IActionResult> Success(string code)
    {
        if (string.IsNullOrEmpty(code)) return RedirectToAction("Index", "Home");

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var orders = await _orderService.GetCustomerOrdersAsync(user.Id);
        var order = orders.FirstOrDefault(o => o.OrderCode == code);

        if (order == null) return RedirectToAction("Index", "Home");

        return View(order);
    }
}
