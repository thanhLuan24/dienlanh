using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Store;

public class StoreAddress : BaseEntity
{
    public int StoreId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string District { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string FullAddress => $"{Street}, {Ward}, {District}, {Province}".Replace(", ,", ",");

    // Navigation
    public Store Store { get; set; } = null!;
}
