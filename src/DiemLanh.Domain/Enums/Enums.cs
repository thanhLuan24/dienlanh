namespace DiemLanh.Domain.Enums;

public enum StoreStatus
{
    Pending = 0,      // Chờ phê duyệt
    Active = 1,       // Đang hoạt động
    Inactive = 2,     // Tạm ngưng
    Suspended = 3,    // Bị đình chỉ
    Rejected = 4      // Bị từ chối
}

public enum OrderStatus
{
    PendingConfirmation = 0,  // Chờ xác nhận
    Confirmed = 1,            // Đã xác nhận
    Preparing = 2,            // Đang chuẩn bị
    Shipping = 3,             // Đang giao
    Delivered = 4,            // Đã giao
    Installed = 5,            // Đã lắp đặt
    Completed = 6,            // Hoàn thành
    Cancelled = 7,            // Hủy
    Rejected = 8              // Từ chối
}

public enum InstallationStatus
{
    Pending = 0,      // Chờ lên lịch
    Scheduled = 1,   // Đã lên lịch
    InProgress = 2,  // Đang thực hiện
    Completed = 3,   // Hoàn thành
    Cancelled = 4,   // Hủy
    Rescheduled = 5  // Đổi lịch
}

public enum RepairStatus
{
    PendingReceive = 0,  // Chờ tiếp nhận
    Checking = 1,        // Đang kiểm tra
    Scheduled = 2,       // Đã hẹn lịch
    Repairing = 3,       // Đang sửa
    Completed = 4,       // Hoàn thành
    CannotRepair = 5     // Không thể sửa
}

public enum ProductStatus
{
    Active = 0,      // Đang bán
    Hidden = 1,      // Ẩn
    OutOfStock = 2,  // Hết hàng
    Discontinued = 3 // Ngừng kinh doanh
}

public enum ImageType
{
    Logo = 0,
    Cover = 1,
    Gallery = 2
}

public enum DayOfWeekVN
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}
