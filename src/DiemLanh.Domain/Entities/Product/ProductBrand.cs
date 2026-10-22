using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Product;

public class ProductBrand : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string? Country { get; set; } // Nhật Bản, Hàn Quốc, Việt Nam, Thái Lan...
    public bool IsFeatured { get; set; } = false;

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
