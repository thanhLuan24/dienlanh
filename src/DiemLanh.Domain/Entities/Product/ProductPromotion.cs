using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Product;

public class ProductPromotion : BaseEntity
{
    public int ProductId { get; set; }
    public string Title { get; set; } = string.Empty; // e.g. Tặng gói bảo dưỡng 1 năm + Miễn phí lắp đặt
    public string? Description { get; set; }
    public decimal? DiscountAmount { get; set; }
    public double? DiscountPercent { get; set; }
    public string? GiftItemName { get; set; }
    public bool FreeInstallation { get; set; } = true;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Product Product { get; set; } = null!;
}

public class ProductVideo : BaseEntity
{
    public int ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty; // YouTube embed or direct video URL
    public string? ThumbnailUrl { get; set; }
    public string VideoType { get; set; } = "Overview"; // Overview, Review, Unboxing, InstallationGuide
    public int SortOrder { get; set; } = 0;

    // Navigation
    public Product Product { get; set; } = null!;
}
