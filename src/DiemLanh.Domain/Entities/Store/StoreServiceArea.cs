using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Store;

public class StoreServiceArea : BaseEntity
{
    public int StoreId { get; set; }
    public string Province { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? Ward { get; set; }
    public double? CenterLatitude { get; set; }
    public double? CenterLongitude { get; set; }
    public double? RadiusKm { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Store Store { get; set; } = null!;
}
