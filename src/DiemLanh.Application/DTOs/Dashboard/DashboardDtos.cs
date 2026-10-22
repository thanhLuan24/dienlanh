namespace DiemLanh.Application.DTOs.Dashboard;

public class StoreDashboardDto
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;

    // KPI Cards
    public int TotalOrders { get; set; }
    public int TodayOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal TodayRevenue { get; set; }

    // Installations
    public int TodayInstallations { get; set; }
    public int PendingInstallations { get; set; }

    // Repairs
    public int TodayRepairs { get; set; }
    public int PendingRepairs { get; set; }

    // Reviews
    public int NewReviews { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }

    // Products
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }

    // Charts Data
    public List<RevenueChartPoint> RevenueLast7Days { get; set; } = new();
    public List<OrderStatusCountDto> OrderStatusCounts { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();

    // Recent Data
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
    public List<UpcomingInstallationDto> UpcomingInstallations { get; set; } = new();
    public List<RecentRepairDto> RecentRepairs { get; set; } = new();
}

public class RevenueChartPoint
{
    public string Date { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class OrderStatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int SoldCount { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentOrderDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpcomingInstallationDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public TimeOnly ScheduledTime { get; set; }
    public string? TechnicianName { get; set; }
    public string InstallationAddress { get; set; } = string.Empty;
}

public class RecentRepairDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class StoreStatisticsDto
{
    public int StoreId { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public double OrderCompletionRate { get; set; }
}

public class MonthlyRevenueDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
