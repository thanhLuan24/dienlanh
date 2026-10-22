using AutoMapper;
using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Dashboard;
using DiemLanh.Application.DTOs.Order;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Entities.Store;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiemLanh.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ApplicationDbContext db, IMapper mapper, ILogger<OrderService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<OrderListDto>> GetOrdersAsync(int storeId, OrderFilterDto filter)
    {
        var query = _db.StoreOrders
            .Include(o => o.Items)
            .Where(o => o.StoreId == storeId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(o => o.OrderCode.Contains(filter.Keyword) ||
                                     o.CustomerName.Contains(filter.Keyword) ||
                                     o.CustomerPhone.Contains(filter.Keyword));

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value.AddDays(1));

        query = filter.SortDesc
            ? query.OrderByDescending(o => o.CreatedAt)
            : query.OrderBy(o => o.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        var dtos = items.Select(o => new OrderListDto
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
            CustomerName = o.CustomerName,
            CustomerPhone = o.CustomerPhone,
            Status = o.Status,
            StatusDisplay = GetStatusDisplay(o.Status),
            TotalAmount = o.TotalAmount,
            ItemCount = o.Items.Count,
            CreatedAt = o.CreatedAt
        }).ToList();

        return PagedResult<OrderListDto>.Create(dtos, total, filter.Page, filter.PageSize);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, int storeId)
    {
        var order = await _db.StoreOrders
            .Include(o => o.Items)
            .Include(o => o.Installation).ThenInclude(i => i!.Technician)
            .Include(o => o.Review)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.StoreId == storeId);

        if (order == null) return null;

        var dto = _mapper.Map<OrderDto>(order);
        dto.StoreName = (await _db.Stores.FindAsync(storeId))?.Name ?? "";
        return dto;
    }

    public async Task<ServiceResult> UpdateOrderStatusAsync(OrderUpdateStatusDto dto, int storeId)
    {
        var order = await _db.StoreOrders.FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.StoreId == storeId);
        if (order == null) return ServiceResult.Fail("Không tìm thấy đơn hàng");

        order.Status = dto.NewStatus;
        if (!string.IsNullOrEmpty(dto.Note)) order.StoreNote = dto.Note;
        if (!string.IsNullOrEmpty(dto.CancelReason)) order.CancelReason = dto.CancelReason;
        if (dto.NewStatus == OrderStatus.Confirmed) order.ConfirmedAt = DateTime.UtcNow;
        if (dto.NewStatus == OrderStatus.Cancelled) order.CancelledAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Order {OrderId} status updated to {Status}", dto.OrderId, dto.NewStatus);
        return ServiceResult.Ok("Đã cập nhật trạng thái đơn hàng");
    }

    public async Task<ServiceResult<InstallationDto>> ScheduleInstallationAsync(InstallationScheduleDto dto, int storeId)
    {
        var order = await _db.StoreOrders.FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.StoreId == storeId);
        if (order == null) return ServiceResult<InstallationDto>.Fail("Không tìm thấy đơn hàng");

        var existing = await _db.StoreInstallations.FirstOrDefaultAsync(i => i.OrderId == dto.OrderId);
        if (existing != null) return ServiceResult<InstallationDto>.Fail("Đơn hàng này đã có lịch lắp đặt");

        var installation = new StoreInstallation
        {
            OrderId = dto.OrderId,
            StoreId = storeId,
            TechnicianId = dto.TechnicianId,
            ScheduledDate = dto.ScheduledDate,
            ScheduledTime = dto.ScheduledTime,
            InstallationAddress = dto.InstallationAddress,
            Note = dto.Note,
            Status = InstallationStatus.Scheduled
        };

        _db.StoreInstallations.Add(installation);
        await _db.SaveChangesAsync();

        return ServiceResult<InstallationDto>.Ok(_mapper.Map<InstallationDto>(installation));
    }

    public async Task<ServiceResult> RescheduleInstallationAsync(InstallationRescheduleDto dto, int storeId)
    {
        var installation = await _db.StoreInstallations
            .FirstOrDefaultAsync(i => i.Id == dto.InstallationId && i.StoreId == storeId);
        if (installation == null) return ServiceResult.Fail("Không tìm thấy lịch lắp đặt");

        installation.ScheduledDate = dto.NewScheduledDate;
        installation.ScheduledTime = dto.NewScheduledTime;
        installation.TechnicianId = dto.TechnicianId;
        installation.RescheduleReason = dto.RescheduleReason;
        installation.RescheduledAt = DateTime.UtcNow;
        installation.Status = InstallationStatus.Rescheduled;

        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Đã đổi lịch lắp đặt");
    }

    public async Task<PagedResult<InstallationDto>> GetInstallationsAsync(int storeId, InstallationFilterDto filter)
    {
        var query = _db.StoreInstallations
            .Include(i => i.Order)
            .Include(i => i.Technician)
            .Where(i => i.StoreId == storeId)
            .AsQueryable();

        if (filter.Status.HasValue) query = query.Where(i => i.Status == filter.Status.Value);
        if (filter.TechnicianId.HasValue) query = query.Where(i => i.TechnicianId == filter.TechnicianId.Value);
        if (filter.Date.HasValue) query = query.Where(i => i.ScheduledDate.Date == filter.Date.Value.Date);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(i => i.Order.CustomerName.Contains(filter.Keyword) ||
                                     i.Order.OrderCode.Contains(filter.Keyword) ||
                                     i.Order.CustomerPhone.Contains(filter.Keyword));

        var total = await query.CountAsync();
        var items = await query.OrderBy(i => i.ScheduledDate).ThenBy(i => i.ScheduledTime)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return PagedResult<InstallationDto>.Create(_mapper.Map<List<InstallationDto>>(items), total, filter.Page, filter.PageSize);
    }

    public async Task<PagedResult<InstallationDto>> GetAllInstallationsAsync(int? storeId, InstallationFilterDto filter)
    {
        var query = _db.StoreInstallations
            .Include(i => i.Order)
            .Include(i => i.Technician)
            .Include(i => i.Store)
            .AsQueryable();

        if (storeId.HasValue && storeId.Value > 0)
            query = query.Where(i => i.StoreId == storeId.Value);

        if (filter.Status.HasValue) query = query.Where(i => i.Status == filter.Status.Value);
        if (filter.TechnicianId.HasValue) query = query.Where(i => i.TechnicianId == filter.TechnicianId.Value);
        if (filter.Date.HasValue) query = query.Where(i => i.ScheduledDate.Date == filter.Date.Value.Date);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(i => i.Order.CustomerName.Contains(filter.Keyword) ||
                                     i.Order.OrderCode.Contains(filter.Keyword) ||
                                     i.Order.CustomerPhone.Contains(filter.Keyword));

        var total = await query.CountAsync();
        var items = await query.OrderBy(i => i.ScheduledDate).ThenBy(i => i.ScheduledTime)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return PagedResult<InstallationDto>.Create(_mapper.Map<List<InstallationDto>>(items), total, filter.Page, filter.PageSize);
    }

    public async Task<InstallationDto?> GetInstallationByIdAsync(int installationId, int storeId)
    {
        var item = await _db.StoreInstallations
            .Include(i => i.Order)
            .Include(i => i.Technician)
            .FirstOrDefaultAsync(i => i.Id == installationId && i.StoreId == storeId);
        return item == null ? null : _mapper.Map<InstallationDto>(item);
    }

    public async Task<InstallationDto?> GetInstallationByIdAsync(int installationId)
    {
        var item = await _db.StoreInstallations
            .Include(i => i.Order)
            .Include(i => i.Technician)
            .Include(i => i.Store)
            .FirstOrDefaultAsync(i => i.Id == installationId);
        return item == null ? null : _mapper.Map<InstallationDto>(item);
    }

    public async Task<ServiceResult> UpdateInstallationStatusAsync(int installationId, InstallationStatus status, string? note, int storeId)
    {
        var item = await _db.StoreInstallations.FirstOrDefaultAsync(i => i.Id == installationId && i.StoreId == storeId);
        if (item == null) return ServiceResult.Fail("Không tìm thấy lịch lắp đặt");

        item.Status = status;
        if (!string.IsNullOrEmpty(note)) item.TechnicianNote = note;
        if (status == InstallationStatus.Completed) item.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AdminUpdateInstallationStatusAsync(int installationId, InstallationStatus status, string? note)
    {
        var item = await _db.StoreInstallations.FirstOrDefaultAsync(i => i.Id == installationId);
        if (item == null) return ServiceResult.Fail("Không tìm thấy lịch lắp đặt");

        item.Status = status;
        if (!string.IsNullOrEmpty(note)) item.TechnicianNote = note;
        if (status == InstallationStatus.Completed) item.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<UpcomingInstallationDto>> GetTodayInstallationsAsync(int storeId)
    {
        var today = DateTime.UtcNow.Date;
        var items = await _db.StoreInstallations
            .Include(i => i.Order)
            .Include(i => i.Technician)
            .Where(i => i.StoreId == storeId &&
                        i.ScheduledDate.Date == today &&
                        i.Status != InstallationStatus.Cancelled)
            .OrderBy(i => i.ScheduledTime)
            .ToListAsync();

        return _mapper.Map<List<UpcomingInstallationDto>>(items);
    }

    // ─── Customer checkout ───────────────────────────────────────────────────

    public async Task<ServiceResult<OrderDto>> PlaceOrderAsync(OrderPlaceDto dto, string customerId)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId && !p.IsDeleted);

        if (product == null)
            return ServiceResult<OrderDto>.Fail("Sản phẩm không tồn tại hoặc đã bị xóa.");

        if (product.Stock < dto.Quantity)
            return ServiceResult<OrderDto>.Fail("Sản phẩm hiện không đủ hàng.");

        var effectivePrice = product.SalePrice ?? product.Price;
        var totalAmount = effectivePrice * dto.Quantity;

        // Generate unique order code: CC-YYYYMMDD-XXXXXX
        var orderCode = $"CC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var order = new StoreOrder
        {
            StoreId = product.StoreId,
            CustomerId = customerId,
            CustomerName = dto.CustomerName.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            CustomerEmail = dto.CustomerEmail.Trim(),
            OrderCode = orderCode,
            Status = OrderStatus.PendingConfirmation,
            TotalAmount = totalAmount,
            ShippingAddress = dto.ShippingAddress.Trim(),
            Note = dto.Note?.Trim(),
            Items = new List<StoreOrderItem>
            {
                new()
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.MainImageUrl,
                    Quantity = dto.Quantity,
                    UnitPrice = effectivePrice,
                    WarrantyMonths = product.WarrantyMonths
                }
            }
        };

        _db.StoreOrders.Add(order);

        // Increment sold count on product
        product.SoldCount += dto.Quantity;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Order {Code} placed by customer {CId} for product {PId}", orderCode, customerId, product.Id);

        var resultDto = new OrderDto
        {
            Id = order.Id,
            StoreId = order.StoreId,
            StoreName = (await _db.Stores.FindAsync(order.StoreId))?.Name ?? "",
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerEmail = order.CustomerEmail,
            OrderCode = order.OrderCode,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Note = order.Note,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                ProductImageUrl = i.ProductImageUrl,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                WarrantyMonths = i.WarrantyMonths
            }).ToList()
        };

        return ServiceResult<OrderDto>.Ok(resultDto);
    }

    public async Task<List<CustomerOrderSummaryDto>> GetCustomerOrdersAsync(string customerId)
    {
        var orders = await _db.StoreOrders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var storeIds = orders.Select(o => o.StoreId).Distinct().ToList();
        var stores = await _db.Stores.Where(s => storeIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name);

        var productIds = orders.SelectMany(o => o.Items).Where(i => i.ProductId.HasValue).Select(i => i.ProductId!.Value).Distinct().ToList();
        var productsById = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Slug);

        var productNames = orders.SelectMany(o => o.Items).Select(i => i.ProductName).Distinct().ToList();
        var productsByName = await _db.Products.Where(p => productNames.Contains(p.Name)).ToDictionaryAsync(p => p.Name, p => p.Slug);

        string? resolveSlug(int? pId, string pName)
        {
            if (pId.HasValue && productsById.TryGetValue(pId.Value, out var slugById)) return slugById;
            if (!string.IsNullOrEmpty(pName) && productsByName.TryGetValue(pName, out var slugByName)) return slugByName;
            return null;
        }

        return orders.Select(o =>
        {
            var firstItem = o.Items.FirstOrDefault();
            var firstSlug = firstItem != null ? resolveSlug(firstItem.ProductId, firstItem.ProductName) : null;

            return new CustomerOrderSummaryDto
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                StoreId = o.StoreId,
                StoreName = stores.GetValueOrDefault(o.StoreId, ""),
                CustomerName = o.CustomerName,
                CustomerPhone = o.CustomerPhone,
                CustomerEmail = o.CustomerEmail,
                ShippingAddress = o.ShippingAddress,
                Note = o.Note,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ItemCount = o.Items.Count,
                FirstProductName = firstItem?.ProductName,
                FirstProductImage = firstItem?.ProductImageUrl,
                FirstProductId = firstItem?.ProductId,
                FirstProductSlug = firstSlug,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(i => new CustomerOrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductSlug = resolveSlug(i.ProductId, i.ProductName),
                    ProductImageUrl = i.ProductImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    WarrantyMonths = i.WarrantyMonths
                }).ToList()
            };
        }).ToList();
    }

    public async Task<CustomerOrderSummaryDto?> GetCustomerOrderByIdAsync(int orderId, string customerId)
    {
        var o = await _db.StoreOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

        if (o == null) return null;

        var store = await _db.Stores.FindAsync(o.StoreId);
        var productIds = o.Items.Where(i => i.ProductId.HasValue).Select(i => i.ProductId!.Value).Distinct().ToList();
        var productsById = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Slug);

        var productNames = o.Items.Select(i => i.ProductName).Distinct().ToList();
        var productsByName = await _db.Products.Where(p => productNames.Contains(p.Name)).ToDictionaryAsync(p => p.Name, p => p.Slug);

        string? resolveSlug(int? pId, string pName)
        {
            if (pId.HasValue && productsById.TryGetValue(pId.Value, out var slugById)) return slugById;
            if (!string.IsNullOrEmpty(pName) && productsByName.TryGetValue(pName, out var slugByName)) return slugByName;
            return null;
        }

        var firstItem = o.Items.FirstOrDefault();
        var firstSlug = firstItem != null ? resolveSlug(firstItem.ProductId, firstItem.ProductName) : null;

        return new CustomerOrderSummaryDto
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
            StoreId = o.StoreId,
            StoreName = store?.Name ?? "",
            CustomerName = o.CustomerName,
            CustomerPhone = o.CustomerPhone,
            CustomerEmail = o.CustomerEmail,
            ShippingAddress = o.ShippingAddress,
            Note = o.Note,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            ItemCount = o.Items.Count,
            FirstProductName = firstItem?.ProductName,
            FirstProductImage = firstItem?.ProductImageUrl,
            FirstProductId = firstItem?.ProductId,
            FirstProductSlug = firstSlug,
            CreatedAt = o.CreatedAt,
            Items = o.Items.Select(i => new CustomerOrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductSlug = resolveSlug(i.ProductId, i.ProductName),
                ProductImageUrl = i.ProductImageUrl,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                WarrantyMonths = i.WarrantyMonths
            }).ToList()
        };
    }

    public async Task<ServiceResult> CancelCustomerOrderAsync(int orderId, string customerId)
    {
        var order = await _db.StoreOrders.FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);
        if (order == null) return ServiceResult.Fail("Không tìm thấy đơn hàng");
        
        if (order.Status != OrderStatus.PendingConfirmation)
            return ServiceResult.Fail("Chỉ có thể hủy đơn hàng khi đang chờ xác nhận");

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancelReason = "Khách hàng tự hủy qua website";

        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Đã hủy đơn hàng thành công");
    }

    // ─── Admin-side ──────────────────────────────────────────────────────────

    public async Task<PagedResult<OrderListDto>> GetAllOrdersAsync(OrderFilterDto filter)
    {
        var query = _db.StoreOrders
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(o => o.OrderCode.Contains(filter.Keyword) ||
                                     o.CustomerName.Contains(filter.Keyword) ||
                                     o.CustomerPhone.Contains(filter.Keyword));

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value.AddDays(1));

        query = filter.SortDesc
            ? query.OrderByDescending(o => o.CreatedAt)
            : query.OrderBy(o => o.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        var dtos = items.Select(o => new OrderListDto
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
            CustomerName = o.CustomerName,
            CustomerPhone = o.CustomerPhone,
            Status = o.Status,
            StatusDisplay = GetStatusDisplay(o.Status),
            TotalAmount = o.TotalAmount,
            ItemCount = o.Items.Count,
            CreatedAt = o.CreatedAt
        }).ToList();

        return PagedResult<OrderListDto>.Create(dtos, total, filter.Page, filter.PageSize);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _db.StoreOrders
            .Include(o => o.Items)
            .Include(o => o.Installation).ThenInclude(i => i!.Technician)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        var dto = _mapper.Map<OrderDto>(order);
        dto.StoreName = (await _db.Stores.FindAsync(order.StoreId))?.Name ?? "";
        return dto;
    }

    public async Task<ServiceResult> AdminUpdateOrderStatusAsync(OrderUpdateStatusDto dto)
    {
        var order = await _db.StoreOrders.FindAsync(dto.OrderId);
        if (order == null) return ServiceResult.Fail("Không tìm thấy đơn hàng");

        order.Status = dto.NewStatus;
        if (!string.IsNullOrEmpty(dto.Note)) order.StoreNote = dto.Note;
        if (!string.IsNullOrEmpty(dto.CancelReason)) order.CancelReason = dto.CancelReason;
        if (dto.NewStatus == OrderStatus.Confirmed) order.ConfirmedAt = DateTime.UtcNow;
        if (dto.NewStatus == OrderStatus.Cancelled) order.CancelledAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Đã cập nhật trạng thái đơn hàng");
    }

    private static string GetStatusDisplay(OrderStatus status) => status switch
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
}
