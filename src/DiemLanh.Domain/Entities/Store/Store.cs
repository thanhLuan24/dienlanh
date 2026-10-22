using DiemLanh.Domain.Common;
using DiemLanh.Domain.Enums;

namespace DiemLanh.Domain.Entities.Store;

public class Store : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Description { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? FanpageUrl { get; set; }
    public string? GoogleMapsUrl { get; set; }
    public TimeOnly OpenTime { get; set; } = new TimeOnly(8, 0);
    public TimeOnly CloseTime { get; set; } = new TimeOnly(21, 0);
    public StoreStatus Status { get; set; } = StoreStatus.Pending;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public string? OwnerId { get; set; } // FK to ApplicationUser
    public string? WarrantyPolicy { get; set; }
    public string? ReturnPolicy { get; set; }
    public string? BusinessDescription { get; set; }
    public double? AverageRating { get; set; }
    public int TotalReviews { get; set; } = 0;

    // Navigation Properties
    public StoreAddress? Address { get; set; }
    public ICollection<StoreServiceArea> ServiceAreas { get; set; } = new List<StoreServiceArea>();
    public ICollection<StoreBrand> Brands { get; set; } = new List<StoreBrand>();
    public ICollection<StoreImage> Images { get; set; } = new List<StoreImage>();
    public ICollection<StoreWorkingHour> WorkingHours { get; set; } = new List<StoreWorkingHour>();
    public ICollection<StoreReview> Reviews { get; set; } = new List<StoreReview>();
    public ICollection<StoreTechnician> Technicians { get; set; } = new List<StoreTechnician>();
    public ICollection<StoreOrder> Orders { get; set; } = new List<StoreOrder>();
    public ICollection<StoreRepairRequest> RepairRequests { get; set; } = new List<StoreRepairRequest>();
}
