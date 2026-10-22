using DiemLanh.Domain.Common;
using DiemLanh.Domain.Enums;

namespace DiemLanh.Domain.Entities.Store;

public class StoreOrder : AuditableEntity
{
    public int StoreId { get; set; }
    public string CustomerId { get; set; } = string.Empty; // FK to ApplicationUser
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.PendingConfirmation;
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Note { get; set; }
    public string? StoreNote { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<StoreOrderItem> Items { get; set; } = new List<StoreOrderItem>();
    public StoreInstallation? Installation { get; set; }
    public StoreReview? Review { get; set; }
}
