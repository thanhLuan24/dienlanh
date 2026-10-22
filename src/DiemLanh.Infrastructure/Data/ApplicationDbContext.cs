using DiemLanh.Domain.Entities.Product;
using DiemLanh.Domain.Entities.Store;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using DiemLanh.Domain.Common;

namespace DiemLanh.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Store
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreAddress> StoreAddresses => Set<StoreAddress>();
    public DbSet<StoreServiceArea> StoreServiceAreas => Set<StoreServiceArea>();
    public DbSet<StoreBrand> StoreBrands => Set<StoreBrand>();
    public DbSet<StoreImage> StoreImages => Set<StoreImage>();
    public DbSet<StoreWorkingHour> StoreWorkingHours => Set<StoreWorkingHour>();
    public DbSet<StoreReview> StoreReviews => Set<StoreReview>();
    public DbSet<StoreTechnician> StoreTechnicians => Set<StoreTechnician>();
    public DbSet<StoreOrder> StoreOrders => Set<StoreOrder>();
    public DbSet<StoreOrderItem> StoreOrderItems => Set<StoreOrderItem>();
    public DbSet<StoreInstallation> StoreInstallations => Set<StoreInstallation>();
    public DbSet<StoreRepairRequest> StoreRepairRequests => Set<StoreRepairRequest>();

    // Product
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductBrand> ProductBrands => Set<ProductBrand>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ProductPromotion> ProductPromotions => Set<ProductPromotion>();
    public DbSet<ProductVideo> ProductVideos => Set<ProductVideo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables to Vietnamese-friendly names
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");

        // Global Soft Delete Query Filters
        builder.Entity<Store>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<StoreTechnician>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<StoreReview>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<StoreOrder>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<StoreRepairRequest>().HasQueryFilter(e => !e.IsDeleted);

        // ===================== STORE CONFIGURATION =====================
        builder.Entity<Store>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(250);
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.AverageRating).HasPrecision(3, 2);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.OwnerId);
            e.HasIndex(x => x.Status);

            // 1-1 Address
            e.HasOne(x => x.Address)
             .WithOne(x => x.Store)
             .HasForeignKey<StoreAddress>(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);

            // 1-N
            e.HasMany(x => x.ServiceAreas)
             .WithOne(x => x.Store)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Brands)
             .WithOne(x => x.Store)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Images)
             .WithOne(x => x.Store)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.WorkingHours)
             .WithOne(x => x.Store)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Reviews)
             .WithOne(x => x.Store)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Technicians)
             .WithOne(x => x.Store)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Orders)
             .WithOne(x => x.Store)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.RepairRequests)
             .WithOne(x => x.Store)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StoreAddress>(e =>
        {
            e.Property(x => x.Street).HasMaxLength(300);
            e.Property(x => x.Province).HasMaxLength(100);
            e.Property(x => x.District).HasMaxLength(100);
            e.Property(x => x.Ward).HasMaxLength(100);
        });

        builder.Entity<StoreServiceArea>(e =>
        {
            e.HasIndex(x => new { x.StoreId, x.Province, x.District });
        });

        // ===================== ORDER CONFIGURATION =====================
        builder.Entity<StoreOrder>(e =>
        {
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.Property(x => x.OrderCode).HasMaxLength(50);
            e.HasIndex(x => x.OrderCode).IsUnique();
            e.HasIndex(x => new { x.StoreId, x.Status });
            e.HasIndex(x => x.CustomerId);

            e.HasMany(x => x.Items)
             .WithOne(x => x.Order)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Installation)
             .WithOne(x => x.Order)
             .HasForeignKey<StoreInstallation>(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Review)
             .WithOne(x => x.Order)
             .HasForeignKey<StoreReview>(x => x.OrderId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StoreOrderItem>(e =>
        {
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        // ===================== INSTALLATION CONFIGURATION =====================
        builder.Entity<StoreInstallation>(e =>
        {
            e.HasIndex(x => new { x.StoreId, x.ScheduledDate });
            e.HasIndex(x => x.TechnicianId);

            e.HasOne(x => x.Technician)
             .WithMany(x => x.Installations)
             .HasForeignKey(x => x.TechnicianId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(x => x.Store)
             .WithMany()
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ===================== REPAIR CONFIGURATION =====================
        builder.Entity<StoreRepairRequest>(e =>
        {
            e.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            e.Property(x => x.ActualCost).HasPrecision(18, 2);
            e.HasIndex(x => new { x.StoreId, x.Status });

            e.HasOne(x => x.Technician)
             .WithMany(x => x.RepairRequests)
             .HasForeignKey(x => x.TechnicianId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ===================== PRODUCT CONFIGURATION =====================
        builder.Entity<ProductBrand>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.IsFeatured);
        });

        builder.Entity<ProductAttribute>(e =>
        {
            e.HasIndex(x => new { x.CategoryId, x.Code });
            e.HasOne(x => x.Category)
             .WithMany(x => x.CategoryAttributes)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductAttributeValue>(e =>
        {
            e.HasIndex(x => new { x.ProductId, x.AttributeId });
            e.HasOne(x => x.Product)
             .WithMany(x => x.AttributeValues)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Attribute)
             .WithMany(x => x.AttributeValues)
             .HasForeignKey(x => x.AttributeId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductReview>(e =>
        {
            e.HasIndex(x => new { x.ProductId, x.Rating });
            e.HasOne(x => x.Product)
             .WithMany(x => x.Reviews)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Store)
             .WithMany()
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductPromotion>(e =>
        {
            e.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Product)
             .WithMany(x => x.Promotions)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductVideo>(e =>
        {
            e.HasOne(x => x.Product)
             .WithMany(x => x.Videos)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Product>(e =>
        {
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.SalePrice).HasPrecision(18, 2);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.StoreId);
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.BrandId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.CapacityBtu);
            e.HasIndex(x => x.IsInverter);
            e.HasIndex(x => x.AverageRating);

            e.HasOne(x => x.Store)
             .WithMany()
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Category)
             .WithMany(x => x.Products)
             .HasForeignKey(x => x.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.BrandEntity)
             .WithMany(x => x.Products)
             .HasForeignKey(x => x.BrandId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(x => x.Images)
             .WithOne(x => x.Product)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    break;
            }
        }
    }
}
