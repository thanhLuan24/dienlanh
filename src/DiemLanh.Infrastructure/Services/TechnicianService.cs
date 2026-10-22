using AutoMapper;
using DiemLanh.Application.Common;
using DiemLanh.Application.Interfaces;
using DiemLanh.Domain.Entities.Store;
using DiemLanh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiemLanh.Infrastructure.Services;

public class TechnicianService : ITechnicianService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<TechnicianService> _logger;

    public TechnicianService(ApplicationDbContext db, IMapper mapper, ILogger<TechnicianService> logger)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TechnicianDto>> GetTechniciansAsync(int storeId)
    {
        var techs = await _db.StoreTechnicians
            .Include(t => t.Store)
            .Where(t => t.StoreId == storeId && !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync();
        var dtos = _mapper.Map<List<TechnicianDto>>(techs);
        foreach (var d in dtos)
        {
            var tech = techs.FirstOrDefault(t => t.Id == d.Id);
            if (tech?.Store != null) d.StoreName = tech.Store.Name;
        }
        return dtos;
    }

    public async Task<IEnumerable<TechnicianDto>> GetAllTechniciansAsync(int? storeId)
    {
        var query = _db.StoreTechnicians
            .Include(t => t.Store)
            .Where(t => !t.IsDeleted)
            .AsQueryable();

        if (storeId.HasValue && storeId.Value > 0)
            query = query.Where(t => t.StoreId == storeId.Value);

        var techs = await query.OrderBy(t => t.Name).ToListAsync();
        var dtos = _mapper.Map<List<TechnicianDto>>(techs);
        foreach (var d in dtos)
        {
            var tech = techs.FirstOrDefault(t => t.Id == d.Id);
            if (tech?.Store != null) d.StoreName = tech.Store.Name;
        }
        return dtos;
    }

    public async Task<ServiceResult<TechnicianDto>> CreateTechnicianAsync(TechnicianCreateDto dto)
    {
        var tech = _mapper.Map<StoreTechnician>(dto);
        _db.StoreTechnicians.Add(tech);
        await _db.SaveChangesAsync();
        return ServiceResult<TechnicianDto>.Ok(_mapper.Map<TechnicianDto>(tech));
    }

    public async Task<ServiceResult<TechnicianDto>> UpdateTechnicianAsync(TechnicianUpdateDto dto, int storeId)
    {
        var tech = await _db.StoreTechnicians.FirstOrDefaultAsync(t => t.Id == dto.Id && t.StoreId == storeId);
        if (tech == null) return ServiceResult<TechnicianDto>.Fail("Không tìm thấy kỹ thuật viên");

        _mapper.Map(dto, tech);
        await _db.SaveChangesAsync();
        return ServiceResult<TechnicianDto>.Ok(_mapper.Map<TechnicianDto>(tech));
    }

    public async Task<ServiceResult> DeleteTechnicianAsync(int id, int storeId)
    {
        var tech = await _db.StoreTechnicians.FirstOrDefaultAsync(t => t.Id == id && t.StoreId == storeId);
        if (tech == null) return ServiceResult.Fail("Không tìm thấy kỹ thuật viên");

        tech.IsDeleted = true;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AdminDeleteTechnicianAsync(int id)
    {
        var tech = await _db.StoreTechnicians.FirstOrDefaultAsync(t => t.Id == id);
        if (tech == null) return ServiceResult.Fail("Không tìm thấy kỹ thuật viên");

        tech.IsDeleted = true;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
