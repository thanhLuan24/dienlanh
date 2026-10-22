using DiemLanh.Domain.Common;
using DiemLanh.Domain.Enums;

namespace DiemLanh.Domain.Entities.Store;

public class StoreRepairRequest : AuditableEntity
{
    public int StoreId { get; set; }
    public string CustomerId { get; set; } = string.Empty; // FK to ApplicationUser
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;   // Điều hòa, Tủ lạnh...
    public string Brand { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string Symptoms { get; set; } = string.Empty;
    public string? ImageUrlsJson { get; set; }   // JSON array of URLs
    public string? VideoUrl { get; set; }
    public RepairStatus Status { get; set; } = RepairStatus.PendingReceive;
    public int? TechnicianId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? DiagnosisNote { get; set; }
    public string? StoreNote { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Source { get; set; } // "website", "chatbot", "phone"

    // Navigation
    public Store Store { get; set; } = null!;
    public StoreTechnician? Technician { get; set; }
}
