using DiemLanh.Application.Interfaces;
using DiemLanh.Infrastructure.Identity;
using DiemLanh.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiemLanh.Web.Controllers;

[Route("auth")]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IOrderService _orderService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger,
        IOrderService orderService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _orderService = orderService;
    }

    // GET /auth/login
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST /auth/login
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }
            _logger.LogInformation("User {Email} logged in.", model.Email);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Redirect admins to dashboard
            if (user != null && await _userManager.IsInRoleAsync(user, AppRoles.SuperAdmin))
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }
        if (result.IsLockedOut)
            ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng thử lại sau.");
        else
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");

        return View(model);
    }

    // GET /auth/register
    [HttpGet("register")]
    public IActionResult Register()
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Home");
        return View();
    }

    // POST /auth/register
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, AppRoles.Customer);
            await _signInManager.SignInAsync(user, isPersistent: false);
            _logger.LogInformation("New user {Email} registered.", model.Email);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);
        return View(model);
    }

    // GET /auth/orders
    [HttpGet("orders")]
    [Authorize]
    public async Task<IActionResult> Orders([FromQuery] string tab = "active")
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var orders = await _orderService.GetCustomerOrdersAsync(user.Id);
        ViewBag.ActiveTab = tab.ToLower() == "completed" ? "completed" : "active";
        return View(orders);
    }

    // GET /auth/orders/{id:int}
    [HttpGet("orders/{id:int}")]
    [Authorize]
    public async Task<IActionResult> OrderDetail(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var order = await _orderService.GetCustomerOrderByIdAsync(id, user.Id);
        if (order == null) return NotFound();

        return View(order);
    }

    // POST /auth/orders/{id:int}/cancel
    [HttpPost("orders/{id:int}/cancel")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var result = await _orderService.CancelCustomerOrderAsync(id, user.Id);
        if (result.Success)
            TempData["Success"] = result.Message ?? "Đã hủy đơn hàng thành công.";
        else
            TempData["Error"] = result.Errors.FirstOrDefault() ?? result.Message ?? "Không thể hủy đơn hàng.";

        return RedirectToAction(nameof(Orders));
    }

    // POST /auth/logout
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
