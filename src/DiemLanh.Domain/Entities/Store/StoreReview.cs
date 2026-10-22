using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Store;

public class StoreReview : AuditableEntity
{
    public int StoreId { get; set; }
    public int? OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty; // FK to ApplicationUser
    public string CustomerName { get; set; } = string.Empty;

    // Ratings 1-5
    public int ProductRating { get; set; }
    public int ServiceRating { get; set; }
    public int InstallationRating { get; set; }
    public int TechnicianRating { get; set; }
    public int ResponseTimeRating { get; set; }

    public double AverageRating =>
        (ProductRating + ServiceRating + InstallationRating + TechnicianRating + ResponseTimeRating) / 5.0;

    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; } = false;
    public bool IsHidden { get; set; } = false;

    // Navigation
    public Store Store { get; set; } = null!;
    public StoreOrder? Order { get; set; }
}
