using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Product;

public class ProductReview : AuditableEntity
{
    public int ProductId { get; set; }
    public int StoreId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAvatarUrl { get; set; }
    public int Rating { get; set; } // 1 to 5 stars
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string? Pros { get; set; } // Ưu điểm
    public string? Cons { get; set; } // Nhược điểm
    public string? ImageUrlsJson { get; set; } // JSON array of string image URLs
    public string? VideoUrl { get; set; }
    public bool IsVerifiedPurchase { get; set; } = true;
    public bool IsApproved { get; set; } = true;
    
    // Store Response
    public string? StoreResponseNote { get; set; }
    public DateTime? StoreResponseAt { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public Store.Store Store { get; set; } = null!;
}
