using AutoMapper;
using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Dashboard;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiemLanh.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApplicationDbContext db, IMapper mapper, ILogger<DashboardService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<StoreDashboardDto> GetStoreDashboardAsync(int storeId)
    {
        var store = await _db.Stores.FindAsync(storeId);
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var orders = await _db.StoreOrders.Include(o => o.Items).Where(o => o.StoreId == storeId).ToListAsync();
        var installations = await _db.StoreInstallations.Include(i => i.Order).Include(i => i.Technician).Where(i => i.StoreId == storeId).ToListAsync();
        var repairs = await _db.StoreRepairRequests.Include(r => r.Store).Include(r => r.Technician).Where(r => r.StoreId == storeId).ToListAsync();
        var reviews = await _db.StoreReviews.Where(r => r.StoreId == storeId).ToListAsync();
        var products = await _db.Products.Where(p => p.StoreId == storeId).ToListAsync();

        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var todayRevenue = completedOrders.Where(o => o.CreatedAt.Date == today).Sum(o => o.TotalAmount);
        var monthRevenue = completedOrders.Where(o => o.CreatedAt >= monthStart).Sum(o => o.TotalAmount);
        var totalRevenue = completedOrders.Sum(o => o.TotalAmount);

        // Revenue last 7 days
        var revenueLast7 = Enumerable.Range(0, 7)
            .Select(i => today.AddDays(-i))
            .Select(date => new RevenueChartPoint
            {
                Date = date.ToString("dd/MM"),
                Revenue = completedOrders.Where(o => o.CreatedAt.Date == date).Sum(o => o.TotalAmount),
                OrderCount = completedOrders.Count(o => o.CreatedAt.Date == date)
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Top products
        var topProducts = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName, i.ProductImageUrl })
            .Select(g => new TopProductDto
            {
                ProductId = g.Key.ProductId ?? 0,
                ProductName = g.Key.ProductName,
                ImageUrl = g.Key.ProductImageUrl,
                SoldCount = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(x => x.SoldCount)
            .Take(5)
            .ToList();

        // Status counts
        var statusCounts = orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusCountDto
            {
                Status = GetStatusName(g.Key),
                Count = g.Count(),
                Color = GetStatusColor(g.Key)
            }).ToList();

        // Recent orders
        var recentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(5).Select(o => new RecentOrderDto
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
            CustomerName = o.CustomerName,
            TotalAmount = o.TotalAmount,
            Status = GetStatusName(o.Status),
            StatusColor = GetStatusColor(o.Status),
            CreatedAt = o.CreatedAt
        }).ToList();

        return new StoreDashboardDto
        {
            StoreId = storeId,
            StoreName = store?.Name ?? "",
            TotalOrders = orders.Count,
            TodayOrders = orders.Count(o => o.CreatedAt.Date == today),
            PendingOrders = orders.Count(o => o.Status == OrderStatus.PendingConfirmation),
            ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Preparing),
            TotalRevenue = totalRevenue,
            MonthRevenue = monthRevenue,
            TodayRevenue = todayRevenue,
            TodayInstallations = installations.Count(i => i.ScheduledDate.Date == today),
            PendingInstallations = installations.Count(i => i.Status == InstallationStatus.Pending || i.Status == InstallationStatus.Scheduled),
            TodayRepairs = repairs.Count(r => r.ScheduledAt.HasValue && r.ScheduledAt.Value.Date == today),
            PendingRepairs = repairs.Count(r => r.Status == RepairStatus.PendingReceive || r.Status == RepairStatus.Checking),
            NewReviews = reviews.Count(r => r.CreatedAt.Date == today),
            AverageRating = reviews.Any() ? reviews.Average(r => r.AverageRating) : 0,
            TotalReviews = reviews.Count,
            TotalProducts = products.Count,
            LowStockProducts = products.Count(p => p.Stock <= 5 && p.Stock > 0),
            RevenueLast7Days = revenueLast7,
            OrderStatusCounts = statusCounts,
            TopProducts = topProducts,
            RecentOrders = recentOrders,
            UpcomingInstallations = _mapper.Map<List<UpcomingInstallationDto>>(installations.Where(i => i.ScheduledDate >= today && i.Status != InstallationStatus.Cancelled).OrderBy(i => i.ScheduledDate).ThenBy(i => i.ScheduledTime).Take(5).ToList()),
            RecentRepairs = repairs.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new RecentRepairDto
            {
                Id = r.Id,
                CustomerName = r.CustomerName,
                DeviceType = r.DeviceType,
                Brand = r.Brand,
                Status = GetRepairStatusName(r.Status),
                StatusColor = GetRepairStatusColor(r.Status),
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    public async Task<StoreStatisticsDto> GetStoreStatisticsAsync(int storeId, int year, int? month = null)
    {
        var orders = await _db.StoreOrders.Include(o => o.Items).Where(o => o.StoreId == storeId && o.CreatedAt.Year == year).ToListAsync();

        if (month.HasValue) orders = orders.Where(o => o.CreatedAt.Month == month.Value).ToList();

        var monthlyRevenue = Enumerable.Range(1, 12).Select(m => new MonthlyRevenueDto
        {
            Month = m,
            MonthName = $"Tháng {m}",
            Revenue = orders.Where(o => o.CreatedAt.Month == m && o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
            OrderCount = orders.Count(o => o.CreatedAt.Month == m)
        }).ToList();

        var topProducts = orders.SelectMany(o => o.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName, i.ProductImageUrl })
            .Select(g => new TopProductDto { ProductId = g.Key.ProductId ?? 0, ProductName = g.Key.ProductName, ImageUrl = g.Key.ProductImageUrl, SoldCount = g.Sum(i => i.Quantity), Revenue = g.Sum(i => i.TotalPrice) })
            .OrderByDescending(x => x.Revenue).Take(10).ToList();

        var completed = orders.Count(o => o.Status == OrderStatus.Completed);

        return new StoreStatisticsDto
        {
            StoreId = storeId,
            Year = year,
            Month = month,
            MonthlyRevenue = monthlyRevenue,
            TopProducts = topProducts,
            TotalOrders = orders.Count,
            CompletedOrders = completed,
            CancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount),
            OrderCompletionRate = orders.Any() ? Math.Round((double)completed / orders.Count * 100, 1) : 0
        };
    }

    private static string GetStatusName(OrderStatus s) => s switch
    {
        OrderStatus.PendingConfirmation => "Chờ xác nhận", OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Preparing => "Đang chuẩn bị", OrderStatus.Shipping => "Đang giao",
        OrderStatus.Delivered => "Đã giao", OrderStatus.Installed => "Đã lắp đặt",
        OrderStatus.Completed => "Hoàn thành", OrderStatus.Cancelled => "Đã hủy",
        OrderStatus.Rejected => "Từ chối", _ => "Không xác định"
    };

    private static string GetStatusColor(OrderStatus s) => s switch
    {
        OrderStatus.PendingConfirmation => "warning", OrderStatus.Confirmed => "info",
        OrderStatus.Preparing => "primary", OrderStatus.Shipping => "info",
        OrderStatus.Delivered => "success", OrderStatus.Completed => "success",
        OrderStatus.Cancelled => "danger", OrderStatus.Rejected => "danger", _ => "secondary"
    };

    private static string GetRepairStatusName(RepairStatus s) => s switch
    {
        RepairStatus.PendingReceive => "Chờ tiếp nhận", RepairStatus.Checking => "Đang kiểm tra",
        RepairStatus.Scheduled => "Đã hẹn lịch", RepairStatus.Repairing => "Đang sửa",
        RepairStatus.Completed => "Hoàn thành", RepairStatus.CannotRepair => "Không thể sửa",
        _ => "Không xác định"
    };

    private static string GetRepairStatusColor(RepairStatus s) => s switch
    {
        RepairStatus.PendingReceive => "warning", RepairStatus.Checking => "info",
        RepairStatus.Scheduled => "primary", RepairStatus.Repairing => "info",
        RepairStatus.Completed => "success", RepairStatus.CannotRepair => "danger", _ => "secondary"
    };
}
