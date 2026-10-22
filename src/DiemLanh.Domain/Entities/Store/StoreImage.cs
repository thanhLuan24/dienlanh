using DiemLanh.Domain.Common;
using DiemLanh.Domain.Enums;

namespace DiemLanh.Domain.Entities.Store;

public class StoreImage : BaseEntity
{
    public int StoreId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int SortOrder { get; set; } = 0;
    public ImageType Type { get; set; } = ImageType.Gallery;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Store Store { get; set; } = null!;
}
