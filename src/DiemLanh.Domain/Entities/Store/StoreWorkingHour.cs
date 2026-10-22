using DiemLanh.Domain.Common;

namespace DiemLanh.Domain.Entities.Store;

public class StoreWorkingHour : BaseEntity
{
    public int StoreId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsClosed { get; set; } = false;

    // Navigation
    public Store Store { get; set; } = null!;
}
