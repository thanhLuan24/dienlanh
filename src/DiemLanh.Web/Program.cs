using DiemLanh.Application.Interfaces;
using DiemLanh.Infrastructure;
using DiemLanh.Infrastructure.Data;
using DiemLanh.Infrastructure.Identity;
using DiemLanh.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES ==========
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // For Identity UI

// Infrastructure (EF Core, AutoMapper, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Identity (configured here because AddIdentity needs ASP.NET Core framework)
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<DiemLanh.Infrastructure.Data.ApplicationDbContext>()
.AddDefaultTokenProviders();

// File Storage (registered here because it needs IWebHostEnvironment)
builder.Services.AddScoped<IFileStorageService>(sp =>
{
    var env = sp.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
    var logger = sp.GetRequiredService<ILogger<LocalFileStorageService>>();
    return new LocalFileStorageService(logger, env.WebRootPath);
});

// API Explorer (Swagger-compatible)
builder.Services.AddEndpointsApiExplorer();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie auth config
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ========== PIPELINE ==========
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Admin routes
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "admin_products",
    pattern: "admin/products/{action=Index}/{id?}",
    defaults: new { controller = "AdminProduct" });

app.MapControllerRoute(
    name: "admin_stores",
    pattern: "admin/stores/{action=Index}/{id?}",
    defaults: new { controller = "AdminStore" });

app.MapControllerRoute(
    name: "admin_users",
    pattern: "admin/users/{action=Index}/{id?}",
    defaults: new { controller = "AdminUser" });

// Store manage routes
app.MapControllerRoute(
    name: "store_manage",
    pattern: "manage/{action=Dashboard}/{id?}",
    defaults: new { controller = "Manage" });

app.MapControllerRoute(
    name: "store_orders",
    pattern: "manage/orders/{action=Index}/{id?}",
    defaults: new { controller = "Order", area = "" });

app.MapControllerRoute(
    name: "store_installations",
    pattern: "manage/installations/{action=Index}/{id?}",
    defaults: new { controller = "Installation" });

app.MapControllerRoute(
    name: "store_repairs",
    pattern: "manage/repairs/{action=Index}/{id?}",
    defaults: new { controller = "Repair" });

app.MapControllerRoute(
    name: "store_products",
    pattern: "manage/products/{action=Index}/{id?}",
    defaults: new { controller = "ProductManage" });

app.MapControllerRoute(
    name: "stores",
    pattern: "stores/{action=Index}/{slug?}",
    defaults: new { controller = "Store" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapControllers(); // For API

// ========== DATABASE SEED ==========
await DatabaseSeeder.SeedAsync(app.Services);

app.Run();
