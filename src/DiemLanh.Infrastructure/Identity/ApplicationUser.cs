using Microsoft.AspNetCore.Identity;

namespace DiemLanh.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int? StoreId { get; set; } // For Store Owner/Staff
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }

    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string StoreOwner = "StoreOwner";
    public const string StoreStaff = "StoreStaff";
    public const string Technician = "Technician";
    public const string Customer = "Customer";

    public static readonly string[] AllRoles =
    [
        SuperAdmin, StoreOwner, StoreStaff, Technician, Customer
    ];
}
