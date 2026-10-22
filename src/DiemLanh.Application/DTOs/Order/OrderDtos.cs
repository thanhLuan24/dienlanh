using DiemLanh.Domain.Enums;

namespace DiemLanh.Application.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        OrderStatus.PendingConfirmation => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Preparing => "Đang chuẩn bị",
        OrderStatus.Shipping => "Đang giao",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.Installed => "Đã lắp đặt",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        OrderStatus.Rejected => "Bị từ chối",
        _ => "Không xác định"
    };
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Note { get; set; }
    public string? StoreNote { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public InstallationDto? Installation { get; set; }
}

public class OrderItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int? WarrantyMonths { get; set; }
}

public class OrderListDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderUpdateStatusDto
{
    public int OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
}

public class OrderFilterDto
{
    public string? Keyword { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public bool SortDesc { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// DTOs for Installation
public class InstallationDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public DateTime ScheduledDate { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public string InstallationAddress { get; set; } = string.Empty;
    public InstallationStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        InstallationStatus.Pending => "Chờ lên lịch",
        InstallationStatus.Scheduled => "Đã lên lịch",
        InstallationStatus.InProgress => "Đang thực hiện",
        InstallationStatus.Completed => "Hoàn thành",
        InstallationStatus.Cancelled => "Hủy",
        InstallationStatus.Rescheduled => "Đổi lịch",
        _ => "Không xác định"
    };
    public string? Note { get; set; }
    public string? TechnicianNote { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InstallationScheduleDto
{
    public int OrderId { get; set; }
    public int? TechnicianId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public string InstallationAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class InstallationRescheduleDto
{
    public int InstallationId { get; set; }
    public DateTime NewScheduledDate { get; set; }
    public TimeOnly NewScheduledTime { get; set; }
    public int? TechnicianId { get; set; }
    public string? RescheduleReason { get; set; }
}

public class InstallationFilterDto
{
    public string? Keyword { get; set; }
    public InstallationStatus? Status { get; set; }
    public int? TechnicianId { get; set; }
    public DateTime? Date { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ─── Customer Checkout ────────────────────────────────────────────────────────
public class OrderPlaceDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class CustomerOrderItemDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSlug { get; set; }
    public string? ProductImageUrl { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public int? WarrantyMonths { get; set; }
}

public class CustomerOrderSummaryDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        OrderStatus.PendingConfirmation => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Preparing => "Đang chuẩn bị",
        OrderStatus.Shipping => "Đang giao",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.Installed => "Đã lắp đặt",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        OrderStatus.Rejected => "Bị từ chối",
        _ => "Không xác định"
    };
    public int ItemCount { get; set; }
    public string? FirstProductName { get; set; }
    public string? FirstProductImage { get; set; }
    public int? FirstProductId { get; set; }
    public string? FirstProductSlug { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CustomerOrderItemDto> Items { get; set; } = new();
}
