using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Store;

public class StoreOrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int? ProductId { get; set; } // nullable: product may be deleted
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public string? ProductSnapshot { get; set; } // JSON snapshot at time of order
    public int? WarrantyMonths { get; set; }

    // Navigation
    public StoreOrder Order { get; set; } = null!;
}
