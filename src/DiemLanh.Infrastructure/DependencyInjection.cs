using DiemLanh.Application.Interfaces;
using DiemLanh.Application.Mappings;
using DiemLanh.Domain.Interfaces;
using DiemLanh.Infrastructure.Data;
using DiemLanh.Infrastructure.Repositories;
using DiemLanh.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiemLanh.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core + SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // AutoMapper
        services.AddAutoMapper(typeof(StoreMappingProfile).Assembly);

        // Repository + UoW
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IRepairService, RepairService>();
        services.AddScoped<IDashboardService, DashboardService>();
        // IFileStorageService registered in Web layer (needs IWebHostEnvironment)
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<ITechnicianService, TechnicianService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
