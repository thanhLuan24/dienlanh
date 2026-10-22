using DiemLanh.Domain.Enums;

namespace DiemLanh.Application.DTOs.Repair;

public class RepairRequestDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public string? VideoUrl { get; set; }
    public RepairStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        RepairStatus.PendingReceive => "Chờ tiếp nhận",
        RepairStatus.Checking => "Đang kiểm tra",
        RepairStatus.Scheduled => "Đã hẹn lịch",
        RepairStatus.Repairing => "Đang sửa",
        RepairStatus.Completed => "Hoàn thành",
        RepairStatus.CannotRepair => "Không thể sửa",
        _ => "Không xác định"
    };
    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? DiagnosisNote { get; set; }
    public string? StoreNote { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RepairRequestCreateDto
{
    public int StoreId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? Source { get; set; } = "website";
}

public class RepairStatusUpdateDto
{
    public int RepairId { get; set; }
    public RepairStatus NewStatus { get; set; }
    public int? TechnicianId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? DiagnosisNote { get; set; }
    public string? StoreNote { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
}

public class RepairFilterDto
{
    public string? Keyword { get; set; }
    public RepairStatus? Status { get; set; }
    public string? DeviceType { get; set; }
    public int? TechnicianId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
