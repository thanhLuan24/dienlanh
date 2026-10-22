using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Admin;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Entities.Product;
using DiemLanh.Domain.Entities.Store;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Data;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiemLanh.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<AdminService> logger)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    // ─── Overview ──────────────────────────────────────────────────────────────
    public async Task<AdminOverviewDto> GetOverviewAsync()
    {
        var today = DateTime.UtcNow.Date;

        var totalUsers = await _userManager.Users.CountAsync();
        var newUsersToday = await _userManager.Users.CountAsync(u => u.CreatedAt >= today);
        var totalStores = await _db.Stores.CountAsync();
        var pendingStores = await _db.Stores.CountAsync(s => s.Status == StoreStatus.Pending);
        var totalProducts = await _db.Products.CountAsync();
        var totalOrders = await _db.StoreOrders.CountAsync();
        var newOrdersToday = await _db.StoreOrders.CountAsync(o => o.CreatedAt >= today);
        var totalRevenue = await _db.StoreOrders
            .Where(o => o.Status == OrderStatus.Completed)
            .SumAsync(o => o.TotalAmount);

        var topViewed = await _db.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.ViewCount)
            .Take(8)
            .Select(p => new AdminTopProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.MainImageUrl,
                Brand = p.Brand,
                CategoryName = p.Category.Name,
                ViewCount = p.ViewCount,
                SoldCount = p.SoldCount,
                Price = p.SalePrice ?? p.Price
            }).ToListAsync();

        var topSold = await _db.Products
            .Include(p => p.Category)
            .OrderByDescending(p => p.SoldCount)
            .Take(8)
            .Select(p => new AdminTopProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.MainImageUrl,
                Brand = p.Brand,
                CategoryName = p.Category.Name,
                ViewCount = p.ViewCount,
                SoldCount = p.SoldCount,
                Price = p.SalePrice ?? p.Price
            }).ToListAsync();

        var recentUsers = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentUserDtos = new List<AdminRecentUserDto>();
        foreach (var u in recentUsers)
        {
            var roles = await _userManager.GetRolesAsync(u);
            recentUserDtos.Add(new AdminRecentUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                Role = roles.FirstOrDefault() ?? "Customer",
                CreatedAt = u.CreatedAt,
                IsActive = u.IsActive
            });
        }

        return new AdminOverviewDto
        {
            TotalUsers = totalUsers,
            TotalStores = totalStores,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            NewUsersToday = newUsersToday,
            NewOrdersToday = newOrdersToday,
            PendingStores = pendingStores,
            TopViewedProducts = topViewed,
            TopSoldProducts = topSold,
            RecentUsers = recentUserDtos
        };
    }

    // ─── Products ──────────────────────────────────────────────────────────────
    public async Task<PagedResult<AdminProductListDto>> GetProductsAsync(string? keyword, int? categoryId, int? brandId, int page = 1, int pageSize = 20)
    {
        var query = _db.Products
            .IgnoreQueryFilters()
            .Include(p => p.Category)
            .Include(p => p.Store)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(kw) || p.Sku.ToLower().Contains(kw) || (p.Brand != null && p.Brand.ToLower().Contains(kw)));
        }
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (brandId.HasValue) query = query.Where(p => p.BrandId == brandId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Sku = p.Sku,
                Brand = p.Brand,
                CategoryName = p.Category.Name,
                StoreName = p.Store.Name,
                Price = p.Price,
                SalePrice = p.SalePrice,
                Stock = p.Stock,
                ViewCount = p.ViewCount,
                SoldCount = p.SoldCount,
                AverageRating = p.AverageRating,
                IsFeatured = p.IsFeatured,
                IsHidden = p.IsHidden,
                MainImageUrl = p.MainImageUrl,
                Status = p.IsDeleted ? "Đã xóa" : (p.IsHidden ? "Ẩn" : p.Status == ProductStatus.Active ? "Đang bán" : "Ngừng bán"),
                CreatedAt = p.CreatedAt
            }).ToListAsync();

        return PagedResult<AdminProductListDto>.Create(items, total, page, pageSize);
    }

    public async Task<AdminProductCreateEditDto?> GetProductForEditAsync(int id)
    {
        var p = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return null;
        return new AdminProductCreateEditDto
        {
            Id = p.Id, StoreId = p.StoreId, CategoryId = p.CategoryId, BrandId = p.BrandId,
            Name = p.Name, SubTitle = p.SubTitle, Sku = p.Sku, Model = p.Model, Brand = p.Brand,
            ShortDescription = p.ShortDescription, Description = p.Description,
            Price = p.Price, SalePrice = p.SalePrice, Stock = p.Stock, WarrantyMonths = p.WarrantyMonths,
            MainImageUrl = p.MainImageUrl, IsInverter = p.IsInverter, CapacityBtu = p.CapacityBtu,
            RoomAreaCode = p.RoomAreaCode, GasType = p.GasType, IsFeatured = p.IsFeatured, IsHidden = p.IsHidden
        };
    }

    public async Task<(bool Success, string Message, int Id)> CreateProductAsync(AdminProductCreateEditDto dto)
    {
        try
        {
            var slug = GenerateSlug(dto.Name);
            var counter = 1;
            var baseSlug = slug;
            while (await _db.Products.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug))
                slug = $"{baseSlug}-{counter++}";

            var product = new Product
            {
                StoreId = dto.StoreId, CategoryId = dto.CategoryId, BrandId = dto.BrandId,
                Name = dto.Name, SubTitle = dto.SubTitle, Slug = slug, Sku = dto.Sku,
                Model = dto.Model, Brand = dto.Brand, ShortDescription = dto.ShortDescription,
                Description = dto.Description, Price = dto.Price, SalePrice = dto.SalePrice,
                Stock = dto.Stock, WarrantyMonths = dto.WarrantyMonths, MainImageUrl = dto.MainImageUrl,
                IsInverter = dto.IsInverter, CapacityBtu = dto.CapacityBtu, RoomAreaCode = dto.RoomAreaCode,
                GasType = dto.GasType, IsFeatured = dto.IsFeatured, IsHidden = dto.IsHidden,
                Status = ProductStatus.Active
            };
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            return (true, "Tạo sản phẩm thành công.", product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return (false, $"Lỗi: {ex.Message}", 0);
        }
    }

    public async Task<(bool Success, string Message)> UpdateProductAsync(AdminProductCreateEditDto dto)
    {
        try
        {
            var product = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == dto.Id);
            if (product == null) return (false, "Không tìm thấy sản phẩm.");

            product.StoreId = dto.StoreId; product.CategoryId = dto.CategoryId; product.BrandId = dto.BrandId;
            product.Name = dto.Name; product.SubTitle = dto.SubTitle; product.Sku = dto.Sku;
            product.Model = dto.Model; product.Brand = dto.Brand; product.ShortDescription = dto.ShortDescription;
            product.Description = dto.Description; product.Price = dto.Price; product.SalePrice = dto.SalePrice;
            product.Stock = dto.Stock; product.WarrantyMonths = dto.WarrantyMonths; product.MainImageUrl = dto.MainImageUrl;
            product.IsInverter = dto.IsInverter; product.CapacityBtu = dto.CapacityBtu; product.RoomAreaCode = dto.RoomAreaCode;
            product.GasType = dto.GasType; product.IsFeatured = dto.IsFeatured; product.IsHidden = dto.IsHidden;

            await _db.SaveChangesAsync();
            return (true, "Cập nhật thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", dto.Id);
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteProductAsync(int id)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [ProductReviews] WHERE [ProductId] = {0}", id);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [ProductPromotions] WHERE [ProductId] = {0}", id);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [ProductImages] WHERE [ProductId] = {0}", id);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [Products] WHERE [Id] = {0}", id);
            return (true, "Đã xóa sản phẩm.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    // ─── Stores ────────────────────────────────────────────────────────────────
    public async Task<PagedResult<AdminStoreListDto>> GetStoresAsync(string? keyword, string? status, int page = 1, int pageSize = 20)
    {
        var query = _db.Stores.IgnoreQueryFilters().Include(s => s.Address).AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(kw) || (s.Email != null && s.Email.ToLower().Contains(kw)));
        }
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<StoreStatus>(status, out var st))
            query = query.Where(s => s.Status == st);

        var total = await query.CountAsync();
        var stores = await query.OrderByDescending(s => s.JoinedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = new List<AdminStoreListDto>();
        foreach (var s in stores)
        {
            var productCount = await _db.Products.IgnoreQueryFilters().CountAsync(p => p.StoreId == s.Id);
            var ownerName = "";
            if (!string.IsNullOrEmpty(s.OwnerId))
            {
                var owner = await _userManager.FindByIdAsync(s.OwnerId);
                ownerName = owner?.FullName ?? owner?.Email ?? "";
            }
            items.Add(new AdminStoreListDto
            {
                Id = s.Id, Name = s.Name, Slug = s.Slug, LogoUrl = s.LogoUrl,
                PhoneNumber = s.PhoneNumber, Email = s.Email,
                Province = s.Address?.Province ?? "",
                Status = GetStoreStatusName(s.Status), StatusColor = GetStoreStatusColor(s.Status),
                OwnerId = s.OwnerId, OwnerName = ownerName, ProductCount = productCount,
                AverageRating = s.AverageRating, JoinedAt = s.JoinedAt
            });
        }
        return PagedResult<AdminStoreListDto>.Create(items, total, page, pageSize);
    }

    public async Task<(bool Success, string Message)> CreateStoreAsync(AdminStoreCreateDto dto)
    {
        try
        {
            var slug = GenerateSlug(dto.Name);
            var counter = 1; var baseSlug = slug;
            while (await _db.Stores.IgnoreQueryFilters().AnyAsync(s => s.Slug == slug))
                slug = $"{baseSlug}-{counter++}";

            var store = new Store
            {
                Name = dto.Name, Slug = slug, PhoneNumber = dto.PhoneNumber, Email = dto.Email,
                Description = dto.Description, LogoUrl = dto.LogoUrl, OwnerId = dto.OwnerId,
                Status = StoreStatus.Active, WarrantyPolicy = dto.WarrantyPolicy, ReturnPolicy = dto.ReturnPolicy
            };
            _db.Stores.Add(store);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(dto.Province))
            {
                _db.StoreAddresses.Add(new StoreAddress
                {
                    StoreId = store.Id, Province = dto.Province ?? "",
                    District = dto.District ?? "", Street = dto.Street ?? ""
                });
                await _db.SaveChangesAsync();
            }
            return (true, "Tạo cửa hàng thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating store");
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteStoreAsync(int id)
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [Products] WHERE [StoreId] = {0}", id);
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM [Stores] WHERE [Id] = {0}", id);
            return (true, "Đã xóa cửa hàng.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting store {Id}", id);
            return (false, $"Lỗi: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ToggleStoreStatusAsync(int id)
    {
        var store = await _db.Stores.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == id);
        if (store == null) return (false, "Không tìm thấy cửa hàng.");
        store.Status = store.Status == StoreStatus.Active ? StoreStatus.Suspended : StoreStatus.Active;
        await _db.SaveChangesAsync();
        return (true, store.Status == StoreStatus.Active ? "Đã kích hoạt cửa hàng." : "Đã tạm ngưng cửa hàng.");
    }

    // ─── Users ─────────────────────────────────────────────────────────────────
    public async Task<PagedResult<AdminUserListDto>> GetUsersAsync(string? keyword, string? role, int page = 1, int pageSize = 20)
    {
        IQueryable<ApplicationUser> query = _userManager.Users;

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(u => u.Email!.ToLower().Contains(kw) || u.FullName.ToLower().Contains(kw));
        }

        var total = await query.CountAsync();
        var users = await query.OrderByDescending(u => u.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = new List<AdminUserListDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var userRole = roles.FirstOrDefault() ?? "Customer";
            if (!string.IsNullOrEmpty(role) && userRole != role) continue;

            string? storeName = null;
            if (u.StoreId.HasValue)
                storeName = (await _db.Stores.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == u.StoreId))?.Name;

            items.Add(new AdminUserListDto
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email ?? "",
                AvatarUrl = u.AvatarUrl, Role = userRole, IsActive = u.IsActive,
                StoreId = u.StoreId, StoreName = storeName, CreatedAt = u.CreatedAt, LastLoginAt = u.LastLoginAt
            });
        }
        return PagedResult<AdminUserListDto>.Create(items, total, page, pageSize);
    }

    public async Task<(bool Success, string Message)> ToggleUserStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "Không tìm thấy người dùng.");
        user.IsActive = !user.IsActive;
        if (!user.IsActive) await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        else await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.UpdateAsync(user);
        return (true, user.IsActive ? "Đã kích hoạt tài khoản." : "Đã khóa tài khoản.");
    }

    public async Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string newRole, int? storeId = null)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "Không tìm thấy người dùng.");

        // Remove all existing roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return (false, "Lỗi khi xóa vai trò cũ: " + string.Join(", ", removeResult.Errors.Select(e => e.Description)));
        }

        // Add new role
        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
            return (false, "Lỗi khi gán vai trò mới: " + string.Join(", ", addResult.Errors.Select(e => e.Description)));

        // Re-fetch user to get updated ConcurrencyStamp before updating StoreId
        var freshUser = await _userManager.FindByIdAsync(userId);
        if (freshUser == null) return (false, "Không tìm thấy người dùng sau khi đổi vai trò.");

        if (newRole == "StoreOwner" || newRole == "StoreStaff" || newRole == "Technician")
            freshUser.StoreId = storeId;
        else
            freshUser.StoreId = null;

        var updateResult = await _userManager.UpdateAsync(freshUser);
        if (!updateResult.Succeeded)
            return (false, "Đã đổi vai trò nhưng lỗi lưu cửa hàng: " + string.Join(", ", updateResult.Errors.Select(e => e.Description)));

        return (true, $"Đã đổi vai trò thành {newRole}" + (storeId.HasValue ? " và liên kết cửa hàng." : "."));
    }

    public async Task<List<StoreSimpleDto>> GetActiveStoresSimpleAsync()
    {
        return await _db.Stores
            .Where(s => s.Status == StoreStatus.Active && !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new StoreSimpleDto
            {
                Id = s.Id,
                Name = s.Name
            }).ToListAsync();
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────
    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace("đ", "d").Replace("ă", "a").Replace("â", "a").Replace("ê", "e")
            .Replace("ô", "o").Replace("ơ", "o").Replace("ư", "u").Replace("ị", "i")
            .Replace("ọ", "o").Replace("ụ", "u").Replace("ạ", "a").Replace("ế", "e")
            .Replace("ề", "e").Replace("ổ", "o").Replace("ố", "o").Replace("ộ", "o")
            .Replace("ợ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ứ", "u")
            .Replace("ừ", "u").Replace("ự", "u").Replace("ẫ", "a").Replace("ấ", "a")
            .Replace("ầ", "a").Replace("ắ", "a").Replace("ặ", "a").Replace("ặ", "a")
            .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a")
            .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e")
            .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i")
            .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o")
            .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u")
            .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');
        if (slug.Length > 120) slug = slug[..120].TrimEnd('-');
        return slug;
    }

    private static string GetStoreStatusName(StoreStatus s) => s switch
    {
        StoreStatus.Pending => "Chờ duyệt",
        StoreStatus.Active => "Đang hoạt động",
        StoreStatus.Inactive => "Tạm ngưng",
        StoreStatus.Suspended => "Đình chỉ",
        StoreStatus.Rejected => "Từ chối",
        _ => "Không xác định"
    };

    private static string GetStoreStatusColor(StoreStatus s) => s switch
    {
        StoreStatus.Pending => "warning",
        StoreStatus.Active => "success",
        StoreStatus.Inactive => "danger",
        StoreStatus.Suspended => "danger",
        StoreStatus.Rejected => "danger",
        _ => "secondary"
    };
}
