using DiemLanh.Domain.Common;
using DiemLanh.Domain.Enums;

namespace DiemLanh.Domain.Entities.Store;

public class StoreInstallation : AuditableEntity
{
    public int OrderId { get; set; }
    public int StoreId { get; set; }
    public int? TechnicianId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public string InstallationAddress { get; set; } = string.Empty;
    public InstallationStatus Status { get; set; } = InstallationStatus.Pending;
    public string? Note { get; set; }
    public string? TechnicianNote { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RescheduledAt { get; set; }
    public string? RescheduleReason { get; set; }

    // Navigation
    public StoreOrder Order { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public StoreTechnician? Technician { get; set; }
}
