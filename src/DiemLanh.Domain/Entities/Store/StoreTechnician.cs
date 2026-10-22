using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Store;

public class StoreTechnician : AuditableEntity
{
    public int StoreId { get; set; }
    public string? UserId { get; set; } // FK to ApplicationUser (nullable if not in system)
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Specialty { get; set; } // Điều hòa, Tủ lạnh, etc.
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Store Store { get; set; } = null!;
    public ICollection<StoreInstallation> Installations { get; set; } = new List<StoreInstallation>();
    public ICollection<StoreRepairRequest> RepairRequests { get; set; } = new List<StoreRepairRequest>();
}
