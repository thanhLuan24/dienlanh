using AutoMapper;
using DiemLanh.Application.Common;
using DiemLanh.Application.DTOs.Repair;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Entities.Store;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DiemLanh.Infrastructure.Services;

public class RepairService : IRepairService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<RepairService> _logger;

    public RepairService(ApplicationDbContext db, IMapper mapper, ILogger<RepairService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<RepairRequestDto>> GetRepairRequestsAsync(int storeId, RepairFilterDto filter)
    {
        var query = _db.StoreRepairRequests
            .Include(r => r.Technician)
            .Include(r => r.Store)
            .Where(r => r.StoreId == storeId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(r => r.CustomerName.Contains(filter.Keyword) ||
                                     r.CustomerPhone.Contains(filter.Keyword) ||
                                     r.DeviceType.Contains(filter.Keyword) ||
                                     r.Brand.Contains(filter.Keyword));

        if (filter.Status.HasValue) query = query.Where(r => r.Status == filter.Status.Value);
        if (!string.IsNullOrWhiteSpace(filter.DeviceType)) query = query.Where(r => r.DeviceType == filter.DeviceType);
        if (filter.TechnicianId.HasValue) query = query.Where(r => r.TechnicianId == filter.TechnicianId.Value);
        if (filter.FromDate.HasValue) query = query.Where(r => r.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(r => r.CreatedAt <= filter.ToDate.Value.AddDays(1));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(r => r.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return PagedResult<RepairRequestDto>.Create(_mapper.Map<List<RepairRequestDto>>(items), total, filter.Page, filter.PageSize);
    }

    public async Task<PagedResult<RepairRequestDto>> GetAllRepairRequestsAsync(int? storeId, RepairFilterDto filter)
    {
        var query = _db.StoreRepairRequests
            .Include(r => r.Technician)
            .Include(r => r.Store)
            .AsQueryable();

        if (storeId.HasValue && storeId.Value > 0)
            query = query.Where(r => r.StoreId == storeId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(r => r.CustomerName.Contains(filter.Keyword) ||
                                     r.CustomerPhone.Contains(filter.Keyword) ||
                                     r.DeviceType.Contains(filter.Keyword) ||
                                     r.Brand.Contains(filter.Keyword));

        if (filter.Status.HasValue) query = query.Where(r => r.Status == filter.Status.Value);
        if (!string.IsNullOrWhiteSpace(filter.DeviceType)) query = query.Where(r => r.DeviceType == filter.DeviceType);
        if (filter.TechnicianId.HasValue) query = query.Where(r => r.TechnicianId == filter.TechnicianId.Value);
        if (filter.FromDate.HasValue) query = query.Where(r => r.CreatedAt >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(r => r.CreatedAt <= filter.ToDate.Value.AddDays(1));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(r => r.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

        return PagedResult<RepairRequestDto>.Create(_mapper.Map<List<RepairRequestDto>>(items), total, filter.Page, filter.PageSize);
    }

    public async Task<RepairRequestDto?> GetRepairRequestByIdAsync(int id, int storeId)
    {
        var item = await _db.StoreRepairRequests
            .Include(r => r.Store)
            .Include(r => r.Technician)
            .FirstOrDefaultAsync(r => r.Id == id && r.StoreId == storeId);
        return item == null ? null : _mapper.Map<RepairRequestDto>(item);
    }

    public async Task<RepairRequestDto?> GetRepairRequestByIdAsync(int id)
    {
        var item = await _db.StoreRepairRequests
            .Include(r => r.Store)
            .Include(r => r.Technician)
            .FirstOrDefaultAsync(r => r.Id == id);
        return item == null ? null : _mapper.Map<RepairRequestDto>(item);
    }

    public async Task<ServiceResult<RepairRequestDto>> CreateRepairRequestAsync(RepairRequestCreateDto dto, string customerId)
    {
        var store = await _db.Stores.FindAsync(dto.StoreId);
        if (store == null) return ServiceResult<RepairRequestDto>.Fail("Không tìm thấy cửa hàng");

        var repair = _mapper.Map<StoreRepairRequest>(dto);
        repair.CustomerId = customerId;
        repair.ImageUrlsJson = "[]";

        _db.StoreRepairRequests.Add(repair);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Repair request created for store {StoreId} by customer {CustomerId}", dto.StoreId, customerId);
        return ServiceResult<RepairRequestDto>.Ok(_mapper.Map<RepairRequestDto>(repair));
    }

    public async Task<ServiceResult> UpdateRepairStatusAsync(RepairStatusUpdateDto dto, int storeId)
    {
        var repair = await _db.StoreRepairRequests.FirstOrDefaultAsync(r => r.Id == dto.RepairId && r.StoreId == storeId);
        if (repair == null) return ServiceResult.Fail("Không tìm thấy yêu cầu sửa chữa");

        repair.Status = dto.NewStatus;
        if (dto.TechnicianId.HasValue) repair.TechnicianId = dto.TechnicianId;
        if (dto.ScheduledAt.HasValue) repair.ScheduledAt = dto.ScheduledAt;
        if (!string.IsNullOrEmpty(dto.DiagnosisNote)) repair.DiagnosisNote = dto.DiagnosisNote;
        if (!string.IsNullOrEmpty(dto.StoreNote)) repair.StoreNote = dto.StoreNote;
        if (dto.EstimatedCost.HasValue) repair.EstimatedCost = dto.EstimatedCost;
        if (dto.ActualCost.HasValue) repair.ActualCost = dto.ActualCost;
        if (dto.NewStatus == RepairStatus.Completed) repair.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Đã cập nhật trạng thái yêu cầu sửa chữa");
    }

    public async Task<ServiceResult> AdminUpdateRepairStatusAsync(RepairStatusUpdateDto dto)
    {
        var repair = await _db.StoreRepairRequests.FirstOrDefaultAsync(r => r.Id == dto.RepairId);
        if (repair == null) return ServiceResult.Fail("Không tìm thấy yêu cầu sửa chữa");

        repair.Status = dto.NewStatus;
        if (dto.TechnicianId.HasValue) repair.TechnicianId = dto.TechnicianId;
        if (dto.ScheduledAt.HasValue) repair.ScheduledAt = dto.ScheduledAt;
        if (!string.IsNullOrEmpty(dto.DiagnosisNote)) repair.DiagnosisNote = dto.DiagnosisNote;
        if (!string.IsNullOrEmpty(dto.StoreNote)) repair.StoreNote = dto.StoreNote;
        if (dto.EstimatedCost.HasValue) repair.EstimatedCost = dto.EstimatedCost;
        if (dto.ActualCost.HasValue) repair.ActualCost = dto.ActualCost;
        if (dto.NewStatus == RepairStatus.Completed) repair.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Đã cập nhật trạng thái yêu cầu sửa chữa");
    }

    public async Task<ServiceResult> AddRepairImagesAsync(int repairId, List<string> imageUrls, int storeId)
    {
        var repair = await _db.StoreRepairRequests.FirstOrDefaultAsync(r => r.Id == repairId && r.StoreId == storeId);
        if (repair == null) return ServiceResult.Fail("Không tìm thấy yêu cầu sửa chữa");

        var existing = string.IsNullOrEmpty(repair.ImageUrlsJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(repair.ImageUrlsJson) ?? new List<string>();

        existing.AddRange(imageUrls);
        repair.ImageUrlsJson = JsonSerializer.Serialize(existing);

        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<RepairRequestDto>> GetTodayRepairsAsync(int storeId)
    {
        var today = DateTime.UtcNow.Date;
        var items = await _db.StoreRepairRequests
            .Include(r => r.Store)
            .Include(r => r.Technician)
            .Where(r => r.StoreId == storeId && r.ScheduledAt.HasValue && r.ScheduledAt.Value.Date == today)
            .OrderBy(r => r.ScheduledAt)
            .ToListAsync();
        return _mapper.Map<List<RepairRequestDto>>(items);
    }

    public async Task<List<RepairRequestDto>> GetCustomerRepairsAsync(string customerId)
    {
        var items = await _db.StoreRepairRequests
            .Include(r => r.Store)
            .Include(r => r.Technician)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return _mapper.Map<List<RepairRequestDto>>(items);
    }

    public async Task<RepairRequestDto?> GetRepairRequestByIdForCustomerAsync(int id, string customerId)
    {
        var item = await _db.StoreRepairRequests
            .Include(r => r.Store)
            .Include(r => r.Technician)
            .FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == customerId);
        return item == null ? null : _mapper.Map<RepairRequestDto>(item);
    }

    public async Task<ServiceResult> CancelCustomerRepairAsync(int id, string customerId)
    {
        var item = await _db.StoreRepairRequests.FirstOrDefaultAsync(r => r.Id == id && r.CustomerId == customerId);
        if (item == null) return ServiceResult.Fail("Không tìm thấy yêu cầu sửa chữa");

        if (item.Status != RepairStatus.PendingReceive)
            return ServiceResult.Fail("Chỉ có thể hủy yêu cầu khi đang chờ tiếp nhận");

        item.Status = RepairStatus.CannotRepair; // Or add a Cancelled status, but currently there is only Closed/CannotRepair in logic. Wait, let's use CannotRepair and Note.
        item.DiagnosisNote = "Khách hàng tự hủy qua website";
        item.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Đã hủy yêu cầu sửa chữa thành công");
    }
}
