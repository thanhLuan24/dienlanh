using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Store;

public class StoreBrand : BaseEntity
{
    public int StoreId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Store Store { get; set; } = null!;
}
