using DiemLanh.Domain.Entities.Product;
using DiemLanh.Domain.Entities.Store;
using DiemLanh.Domain.Enums;
using DiemLanh.Infrastructure.Data;
using DiemLanh.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiemLanh.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            // Seed Roles
            foreach (var roleName in AppRoles.AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole(roleName));
                    logger.LogInformation("Created role: {Role}", roleName);
                }
            }

            // Seed Super Admin (primary)
            const string adminEmail = "admin@diemlanhvn.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Super Admin",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, AppRoles.SuperAdmin);
                    logger.LogInformation("Super Admin created: {Email}", adminEmail);
                }
            }

            // Seed Super Admin (alias – password: Admin@123)
            const string adminEmail2 = "admin@coolconnect.vn";
            if (await userManager.FindByEmailAsync(adminEmail2) == null)
            {
                var admin2 = new ApplicationUser
                {
                    UserName = adminEmail2,
                    Email = adminEmail2,
                    FullName = "CoolConnect Admin",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var r2 = await userManager.CreateAsync(admin2, "Admin@123");
                if (r2.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin2, AppRoles.SuperAdmin);
                    logger.LogInformation("CoolConnect Admin created: {Email}", adminEmail2);
                }
            }

            // Seed Product Categories
            if (!await context.ProductCategories.AnyAsync())
            {
                var categories = new List<ProductCategory>
                {
                    new() { Name = "Điều hòa", Slug = "dieu-hoa", IconClass = "bi bi-wind", SortOrder = 1 },
                    new() { Name = "Tủ lạnh", Slug = "tu-lanh", IconClass = "bi bi-snow2", SortOrder = 2 },
                    new() { Name = "Máy giặt", Slug = "may-giat", IconClass = "bi bi-droplet", SortOrder = 3 },
                    new() { Name = "Máy lọc nước", Slug = "may-loc-nuoc", IconClass = "bi bi-water", SortOrder = 4 },
                    new() { Name = "Bình nóng lạnh", Slug = "binh-nong-lanh", IconClass = "bi bi-thermometer-half", SortOrder = 5 },
                    new() { Name = "Máy sấy", Slug = "may-say", IconClass = "bi bi-sun", SortOrder = 6 },
                    new() { Name = "Quạt điều hòa", Slug = "quat-dieu-hoa", IconClass = "bi bi-fan", SortOrder = 7 },
                    new() { Name = "Máy hút mùi", Slug = "may-hut-mui", IconClass = "bi bi-layers", SortOrder = 8 },
                    new() { Name = "Thiết bị điện lạnh khác", Slug = "khac", IconClass = "bi bi-plug", SortOrder = 9 },
                };
                await context.ProductCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
                logger.LogInformation("Product categories seeded");
            }

            // Seed Demo Store
            if (!await context.Stores.AnyAsync())
            {
                // Create store owner
                const string ownerEmail = "store1@demo.com";
                ApplicationUser? owner = await userManager.FindByEmailAsync(ownerEmail);
                if (owner == null)
                {
                    owner = new ApplicationUser
                    {
                        UserName = ownerEmail,
                        Email = ownerEmail,
                        FullName = "Nguyễn Văn An",
                        EmailConfirmed = true,
                        IsActive = true
                    };
                    await userManager.CreateAsync(owner, "Store@123456");
                    await userManager.AddToRoleAsync(owner, AppRoles.StoreOwner);
                }

                var store = new Store
                {
                    Name = "Điện Lạnh An Phát",
                    Slug = "dien-lanh-an-phat",
                    Description = "Cửa hàng điện lạnh uy tín tại Thái Nguyên, chuyên cung cấp và lắp đặt điều hòa, tủ lạnh, máy giặt các hãng uy tín.",
                    PhoneNumber = "0912345678",
                    Email = "anphat@diemlanhvn.com",
                    GoogleMapsUrl = "https://maps.google.com",
                    OpenTime = new TimeOnly(8, 0),
                    CloseTime = new TimeOnly(21, 0),
                    Status = StoreStatus.Active,
                    JoinedAt = DateTime.UtcNow,
                    OwnerId = owner.Id,
                    WarrantyPolicy = "Bảo hành theo hãng sản xuất, hỗ trợ bảo hành tại nhà.",
                    ReturnPolicy = "Đổi trả trong 7 ngày nếu sản phẩm lỗi do nhà sản xuất.",
                    AverageRating = 4.5,
                    TotalReviews = 0,
                    LogoUrl = "/images/store-logo.png",
                    CoverImageUrl = "/images/store-banner.png"
                };

                store.Address = new StoreAddress
                {
                    Street = "123 Đường Hoàng Văn Thụ",
                    Ward = "Phường Trưng Vương",
                    District = "Thành phố Thái Nguyên",
                    Province = "Thái Nguyên",
                    Latitude = 21.5928,
                    Longitude = 105.8478
                };

                store.ServiceAreas = new List<StoreServiceArea>
                {
                    new() { Province = "Thái Nguyên", District = "Thành phố Thái Nguyên", IsActive = true },
                    new() { Province = "Thái Nguyên", District = "Sông Công", IsActive = true },
                    new() { Province = "Thái Nguyên", District = "Phổ Yên", IsActive = true }
                };

                store.Brands = new List<StoreBrand>
                {
                    new() { BrandName = "Daikin" },
                    new() { BrandName = "LG" },
                    new() { BrandName = "Samsung" },
                    new() { BrandName = "Panasonic" },
                    new() { BrandName = "Mitsubishi" }
                };

                store.WorkingHours = Enumerable.Range(1, 7).Select(d => new StoreWorkingHour
                {
                    DayOfWeek = (DayOfWeek)(d % 7),
                    OpenTime = new TimeOnly(8, 0),
                    CloseTime = new TimeOnly(21, 0),
                    IsClosed = false
                }).ToList();

                store.Technicians = new List<StoreTechnician>
                {
                    new() { Name = "Trần Văn Bình", Phone = "0987654321", Specialty = "Điều hòa", IsActive = true },
                    new() { Name = "Lê Thị Cúc", Phone = "0976543210", Specialty = "Tủ lạnh, Máy giặt", IsActive = true }
                };

                context.Stores.Add(store);
                await context.SaveChangesAsync();

                // Update owner's store ID
                owner.StoreId = store.Id;
                await userManager.UpdateAsync(owner);

                logger.LogInformation("Demo store seeded: {StoreName}", store.Name);
            }

            // Ensure existing store has logo and banner set
            var apStore = await context.Stores.FirstOrDefaultAsync(s => s.Slug == "dien-lanh-an-phat");
            if (apStore != null)
            {
                bool modified = false;
                if (string.IsNullOrEmpty(apStore.LogoUrl) || apStore.LogoUrl != "/images/store-logo.png")
                {
                    apStore.LogoUrl = "/images/store-logo.png";
                    modified = true;
                }
                if (string.IsNullOrEmpty(apStore.CoverImageUrl) || apStore.CoverImageUrl != "/images/store-banner.png")
                {
                    apStore.CoverImageUrl = "/images/store-banner.png";
                    modified = true;
                }
                if (modified)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Updated logo/cover for existing store: {StoreName}", apStore.Name);
                }
            }

            // Seed Product Brands
            if (!await context.ProductBrands.AnyAsync())
            {
                var brands = new List<ProductBrand>
                {
                    new() { Name = "Daikin", Slug = "daikin", Country = "Nhật Bản", IsFeatured = true },
                    new() { Name = "Panasonic", Slug = "panasonic", Country = "Nhật Bản", IsFeatured = true },
                    new() { Name = "LG", Slug = "lg", Country = "Hàn Quốc", IsFeatured = true },
                    new() { Name = "Samsung", Slug = "samsung", Country = "Hàn Quốc", IsFeatured = true },
                    new() { Name = "Casper", Slug = "casper", Country = "Thái Lan", IsFeatured = true },
                    new() { Name = "Toshiba", Slug = "toshiba", Country = "Nhật Bản", IsFeatured = false },
                    new() { Name = "Electrolux", Slug = "electrolux", Country = "Thụy Điển", IsFeatured = false },
                    new() { Name = "Sharp", Slug = "sharp", Country = "Nhật Bản", IsFeatured = false },
                };
                await context.ProductBrands.AddRangeAsync(brands);
                await context.SaveChangesAsync();
                logger.LogInformation("Product brands seeded");
            }

            // Seed Sample Products (always reset to clean 3x3 grid)
            if (true)
            {
                // Clear existing promotions, reviews, and all products using raw SQL to bypass soft-delete logic
                await context.Database.ExecuteSqlRawAsync("DELETE FROM ProductPromotions");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM ProductReviews");
                await context.Database.ExecuteSqlRawAsync("DELETE FROM Products");

                var demoStore = await context.Stores.FirstOrDefaultAsync();
                var acCategory = await context.ProductCategories.FirstOrDefaultAsync(c => c.Slug == "dieu-hoa");
                var wmCategory = await context.ProductCategories.FirstOrDefaultAsync(c => c.Slug == "may-giat");
                var daikinBrand = await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "daikin");
                var lgBrand = await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "lg");
                var panasonicBrand = await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "panasonic");
                var toshibaBrand = await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "toshiba");
                var electroluxBrand = await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "electrolux");
                var samsungBrand = await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "samsung");

                if (demoStore != null && acCategory != null)
                {
                    var productsToSeed = new List<Product>
                    {
                        // --- WASHING MACHINES (MÁY GIẶT) ---
                        // 1. LG AI DD Inverter 10 kg
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = wmCategory?.Id ?? acCategory.Id,
                            BrandId = lgBrand?.Id,
                            Brand = "LG",
                            Name = "Máy Giặt LG AI DD Inverter 10 kg FV1410S4B",
                            Slug = "may-giat-lg-ai-dd-inverter-10-kg-fv1410s4b",
                            Sku = "LG-FV1410S4B",
                            Model = "FV1410S4B",
                            SubTitle = "Truyền động trực tiếp AI DD bảo vệ sợi vải, giặt hơi nước Steam diệt khuẩn 99.9%",
                            Description = "Máy giặt LG AI DD Inverter 10 kg FV1410S4B thiết kế cửa ngang sang trọng màu đen nhám. Công nghệ AI DD nhận biết khối lượng và độ mềm sợi vải để chọn chuyển động giặt tối ưu nhất, cùng giặt hơi nước Steam loại bỏ 99.9% vi khuẩn và nếp nhăn.",
                            ShortDescription = "Máy giặt lồng ngang LG 10 kg Inverter truyền động trực tiếp AI DD",
                            Price = 11490000,
                            SalePrice = 8990000,
                            Stock = 18,
                            WarrantyMonths = 24,
                            IsInverter = true,
                            AverageRating = 4.9,
                            ReviewCount = 34,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 120,
                            MainImageUrl = "/images/lg-wm.png",
                            HighlightsJson = "[\"Động cơ truyền động trực tiếp AI DD thông minh\",\"Giặt hơi nước Steam diệt khuẩn & ngừa dị ứng 99.9%\",\"Công nghệ Inverter tiết kiệm điện êm ái\",\"Khối lượng giặt 10kg phù hợp gia đình 5-7 người\"]",
                            SpecificationsJson = "{\"Loại máy giặt\":\"Cửa trước (Lồng ngang)\",\"Khối lượng giặt\":\"10.0 kg\",\"Công nghệ Inverter\":\"Có (Truyền động trực tiếp AI DD)\",\"Tốc độ quay vắt\":\"1400 vòng/phút\",\"Chương trình giặt\":\"14 chương trình\",\"Thời gian bảo hành\":\"24 tháng chính hãng\",\"Nơi sản xuất\":\"Việt Nam / Thái Lan\"}"
                        },

                        // 2. Toshiba Inverter 10.5 kg AW-DUK1150HV
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = wmCategory?.Id ?? acCategory.Id,
                            BrandId = toshibaBrand?.Id,
                            Brand = "Toshiba",
                            Name = "Máy Giặt Inverter Toshiba 10.5 kg AW-DUK1150HV",
                            Slug = "may-giat-inverter-toshiba-10-5-kg-aw-duk1150hv",
                            Sku = "TOSHIBA-DUK1150",
                            Model = "AW-DUK1150HV",
                            SubTitle = "Siêu bọt khí Nano UFB đánh tan vết bẩn, sóng nước Greatwaves mạnh mẽ",
                            Description = "Máy giặt Toshiba 10.5 kg AW-DUK1150HV là dòng máy giặt cửa trên hiện đại với công nghệ siêu bọt khí Nano UFB độc quyền giúp các bong bóng xà phòng siêu nhỏ thấm sâu vào từng sợi vải, đánh bay các vết bẩn cứng đầu mà vẫn tiết kiệm điện nước tối đa.",
                            ShortDescription = "Máy giặt lồng đứng Toshiba 10.5 kg Inverter bọt khí Nano UFB",
                            Price = 9890000,
                            SalePrice = 7690000,
                            Stock = 22,
                            WarrantyMonths = 24,
                            IsInverter = true,
                            AverageRating = 4.8,
                            ReviewCount = 27,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 98,
                            MainImageUrl = "/images/toshiba-wm.png",
                            HighlightsJson = "[\"Công nghệ bọt khí siêu mịn Nano UFB\",\"Sóng nước Greatwaves bảo vệ quần áo\",\"Động cơ Origin Inverter vận hành êm ái\",\"Nắp kính cường lực tràn viền đóng chậm an toàn\"]",
                            SpecificationsJson = "{\"Loại máy giặt\":\"Cửa trên (Lồng đứng)\",\"Khối lượng giặt\":\"10.5 kg\",\"Công nghệ Inverter\":\"Origin Inverter truyền động trực tiếp\",\"Tốc độ quay vắt\":\"700 vòng/phút\",\"Chương trình giặt\":\"8 chương trình\",\"Thời gian bảo hành\":\"24 tháng chính hãng\",\"Nơi sản xuất\":\"Thái Lan\"}"
                        },

                        // 3. Electrolux EcoInverter 9 kg EWF9024P5SB
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = wmCategory?.Id ?? acCategory.Id,
                            BrandId = electroluxBrand?.Id,
                            Brand = "Electrolux",
                            Name = "Máy Giặt Electrolux EcoInverter 9 kg EWF9024P5SB",
                            Slug = "may-giat-electrolux-ecoinverter-9-kg-ewf9024p5sb",
                            Sku = "ELECTROLUX-EWF9024",
                            Model = "EWF9024P5SB",
                            SubTitle = "Giặt hơi nước HygienicCare diệt khuẩn, công nghệ UltraMix hòa tan nước giặt",
                            Description = "Máy giặt Electrolux EcoInverter 9 kg EWF9024P5SB sở hữu thiết kế Châu Âu sang trọng màu xám đen Onyx. Công nghệ UltraMix giúp hòa tan hoàn toàn chất tẩy rửa trước khi giặt, kết hợp công nghệ HygienicCare giặt bằng hơi nước diệt khuẩn đến 99.9%.",
                            ShortDescription = "Máy giặt lồng ngang Electrolux 9 kg EcoInverter giặt hơi nước",
                            Price = 12490000,
                            SalePrice = 9490000,
                            Stock = 15,
                            WarrantyMonths = 24,
                            IsInverter = true,
                            AverageRating = 4.9,
                            ReviewCount = 41,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 135,
                            MainImageUrl = "/images/electrolux-wm.png",
                            HighlightsJson = "[\"Công nghệ UltraMix hòa tan chất tẩy rửa chống cặn\",\"Giặt hơi nước HygienicCare diệt 99.9% vi khuẩn\",\"Động cơ EcoInverter êm ái tiết kiệm 50% điện\",\"Chế độ giặt nhanh 15 phút vô cùng tiện lợi\"]",
                            SpecificationsJson = "{\"Loại máy giặt\":\"Cửa trước (Lồng ngang)\",\"Khối lượng giặt\":\"9.0 kg\",\"Công nghệ Inverter\":\"EcoInverter\",\"Tốc độ quay vắt\":\"1200 vòng/phút\",\"Chương trình giặt\":\"15 chương trình\",\"Thời gian bảo hành\":\"24 tháng chính hãng\",\"Nơi sản xuất\":\"Thái Lan\"}"
                        },

                        // 4. Samsung AI EcoBubble 9 kg WW90T554DAX/SV
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = wmCategory?.Id ?? acCategory.Id,
                            BrandId = samsungBrand?.Id,
                            Brand = "Samsung",
                            Name = "Máy Giặt Samsung AI EcoBubble Inverter 9 kg WW90T554DAX/SV",
                            Slug = "may-giat-samsung-ai-ecobubble-inverter-9-kg-ww90t554dax-sv",
                            Sku = "SAMSUNG-WW90T554",
                            Model = "WW90T554DAX/SV",
                            SubTitle = "Bảng điều khiển AI Control thông minh, bong bóng siêu mịn EcoBubble thẩm thấu gấp 40 lần",
                            Description = "Máy giặt Samsung WW90T554DAX/SV với công nghệ EcoBubble tạo bong bóng xà phòng xốp mịn thấm sâu sợi vải gấp 40 lần, đánh bật vết bẩn hiệu quả ngay cả trong nước lạnh. Bảng điều khiển AI Control tự động ghi nhớ thói quen sử dụng của bạn.",
                            ShortDescription = "Máy giặt lồng ngang Samsung 9 kg AI EcoBubble Inverter",
                            Price = 11990000,
                            SalePrice = 8790000,
                            Stock = 20,
                            WarrantyMonths = 24,
                            IsInverter = true,
                            AverageRating = 4.8,
                            ReviewCount = 29,
                            Status = ProductStatus.Active,
                            IsFeatured = false,
                            SoldCount = 88,
                            MainImageUrl = "/images/samsung-wm.png",
                            HighlightsJson = "[\"Bảng điều khiển AI Control thông minh tự đề xuất chế độ\",\"Giặt EcoBubble đánh bật vết bẩn trong nước lạnh\",\"Giặt hơi nước Hygiene Steam diệt khuẩn 99.9%\",\"Động cơ Digital Inverter bảo hành 20 năm\"]",
                            SpecificationsJson = "{\"Loại máy giặt\":\"Cửa trước (Lồng ngang)\",\"Khối lượng giặt\":\"9.0 kg\",\"Công nghệ Inverter\":\"Digital Inverter\",\"Tốc độ quay vắt\":\"1400 vòng/phút\",\"Chương trình giặt\":\"22 chương trình\",\"Thời gian bảo hành\":\"24 tháng (Động cơ 20 năm)\",\"Nơi sản xuất\":\"Việt Nam\"}"
                        },

                        // 5. Panasonic Inverter 9.5 kg NA-FD95V1BRV
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = wmCategory?.Id ?? acCategory.Id,
                            BrandId = panasonicBrand?.Id,
                            Brand = "Panasonic",
                            Name = "Máy Giặt Panasonic Inverter 9.5 kg NA-FD95V1BRV",
                            Slug = "may-giat-panasonic-inverter-9-5-kg-na-fd95v1brv",
                            Sku = "PANASONIC-FD95V1",
                            Model = "NA-FD95V1BRV",
                            SubTitle = "Giặt nước nóng StainMaster+ diệt 99.99% vi khuẩn, bọt mịn ActiveFoam",
                            Description = "Máy giặt Panasonic 9.5 kg NA-FD95V1BRV tích hợp công nghệ StainMaster+ giặt nước nóng diệt khuẩn 99.99% và đánh tan vết mỡ dầu cặn bẩn. Hệ thống ActiveFoam biến nước giặt thành vô số bọt siêu mịn phủ đều toàn bộ lồng giặt.",
                            ShortDescription = "Máy giặt lồng đứng Panasonic 9.5 kg Inverter giặt nước nóng StainMaster+",
                            Price = 10990000,
                            SalePrice = 8490000,
                            Stock = 16,
                            WarrantyMonths = 24,
                            IsInverter = true,
                            AverageRating = 4.9,
                            ReviewCount = 23,
                            Status = ProductStatus.Active,
                            IsFeatured = false,
                            SoldCount = 76,
                            MainImageUrl = "/images/panasonic-ac.png",
                            HighlightsJson = "[\"Công nghệ StainMaster+ giặt nước nóng diệt khuẩn 99.99%\",\"Hệ thống ActiveFoam tạo bọt xà phòng siêu mịn\",\"Động cơ TD Inverter kết hợp Econavi tiết kiệm điện\",\"Xoáy nước Water Bazooka mạnh mẽ cuộn trôi vết bẩn\"]",
                            SpecificationsJson = "{\"Loại máy giặt\":\"Cửa trên (Lồng đứng)\",\"Khối lượng giặt\":\"9.5 kg\",\"Công nghệ Inverter\":\"TD Inverter & Econavi\",\"Tốc độ quay vắt\":\"700 vòng/phút\",\"Chương trình giặt\":\"10 chương trình\",\"Thời gian bảo hành\":\"24 tháng chính hãng\",\"Nơi sản xuất\":\"Việt Nam\"}"
                        },
                        // --- DAIKIN PRODUCTS ---
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = daikinBrand?.Id,
                            Brand = "Daikin",
                            Name = "Điều Hòa Inverter Daikin 1HP FTKB25WAVMV",
                            Slug = "dieu-hoa-inverter-daikin-1hp-ftkb25wavmv",
                            Sku = "DAIKIN-FTKB25",
                            Model = "FTKB25WAVMV",
                            SubTitle = "Tiết kiệm điện Inverter, Phin lọc Enzyme Blue diệt khuẩn 99.9%",
                            Description = "Điều hòa Daikin Inverter 1 HP FTKB25WAVMV trang bị công nghệ Inverter giúp tiết kiệm điện năng tối đa, phin lọc Enzyme Blue tích hợp lọc PM2.5 diệt khuẩn hiệu quả.",
                            ShortDescription = "Điều hòa Inverter 9.000 BTU phù hợp phòng dưới 15m²",
                            Price = 10990000,
                            SalePrice = 8490000,
                            Stock = 20,
                            WarrantyMonths = 12,
                            IsInverter = true,
                            CapacityBtu = 9000,
                            RoomAreaCode = "UNDER_15",
                            GasType = "R32",
                            AverageRating = 4.8,
                            ReviewCount = 15,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 95,
                            MainImageUrl = "/images/daikin-ac.png",
                            HighlightsJson = "[\"Công nghệ Inverter tiết kiệm điện\",\"Phin lọc Enzyme Blue + PM2.5\",\"Chế độ Econo tiết kiệm điện năng\",\"Luồng gió Coanda dễ chịu không gây buốt\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"9.000 BTU (1.0 HP)\",\"Công nghệ Inverter\":\"Có (Biến tần Inverter)\",\"Môi chất làm lạnh\":\"Gas R32 thân thiện môi trường\",\"Điện năng tiêu thụ\":\"0.8 kW/h\",\"Diện tích phòng phù hợp\":\"Dưới 15 m²\",\"Thời gian bảo hành\":\"12 tháng chính hãng\",\"Nơi sản xuất\":\"Việt Nam\"}"
                        },
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = daikinBrand?.Id,
                            Brand = "Daikin",
                            Name = "Điều Hòa Inverter Daikin 1.5HP FTKB35WAVMV",
                            Slug = "dieu-hoa-inverter-daikin-1-5hp-ftkb35wavmv",
                            Sku = "DAIKIN-FTKB35",
                            Model = "FTKB35WAVMV",
                            SubTitle = "Tiết kiệm điện Inverter, Phin lọc Enzyme Blue diệt khuẩn 99.9%",
                            Description = "Điều hòa Daikin Inverter 1.5 HP FTKB35WAVMV trang bị công nghệ Inverter giúp tiết kiệm điện năng tối đa, phin lọc Enzyme Blue tích hợp lọc PM2.5 diệt khuẩn hiệu quả.",
                            ShortDescription = "Điều hòa Inverter 12.000 BTU phù hợp phòng 15 - 20m²",
                            Price = 12990000,
                            SalePrice = 9990000,
                            Stock = 15,
                            WarrantyMonths = 12,
                            IsInverter = true,
                            CapacityBtu = 12000,
                            RoomAreaCode = "FROM_15_TO_20",
                            GasType = "R32",
                            AverageRating = 4.9,
                            ReviewCount = 28,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 142,
                            MainImageUrl = "/images/daikin-ac.png",
                            HighlightsJson = "[\"Công nghệ Inverter tiết kiệm điện\",\"Phin lọc Enzyme Blue + PM2.5\",\"Chế độ Econo tiết kiệm điện năng\",\"Luồng gió Coanda dễ chịu không gây buốt\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"12.000 BTU (1.5 HP)\",\"Công nghệ Inverter\":\"Có (Biến tần Inverter)\",\"Môi chất làm lạnh\":\"Gas R32 thân thiện môi trường\",\"Điện năng tiêu thụ\":\"1.2 kW/h\",\"Diện tích phòng phù hợp\":\"15 - 20 m²\",\"Thời gian bảo hành\":\"12 tháng chính hãng\",\"Nơi sản xuất\":\"Việt Nam\"}"
                        },
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = daikinBrand?.Id,
                            Brand = "Daikin",
                            Name = "Điều Hòa Inverter Daikin 2HP FTKB50WAVMV",
                            Slug = "dieu-hoa-inverter-daikin-2hp-ftkb50wavmv",
                            Sku = "DAIKIN-FTKB50",
                            Model = "FTKB50WAVMV",
                            SubTitle = "Tiết kiệm điện Inverter, Phin lọc Enzyme Blue diệt khuẩn 99.9%",
                            Description = "Điều hòa Daikin Inverter 2 HP FTKB50WAVMV trang bị công nghệ Inverter giúp tiết kiệm điện năng tối đa, phin lọc Enzyme Blue tích hợp lọc PM2.5 diệt khuẩn hiệu quả.",
                            ShortDescription = "Điều hòa Inverter 18.000 BTU phù hợp phòng 20 - 30m²",
                            Price = 19990000,
                            SalePrice = 15990000,
                            Stock = 10,
                            WarrantyMonths = 12,
                            IsInverter = true,
                            CapacityBtu = 18000,
                            RoomAreaCode = "FROM_20_TO_30",
                            GasType = "R32",
                            AverageRating = 4.7,
                            ReviewCount = 18,
                            Status = ProductStatus.Active,
                            IsFeatured = false,
                            SoldCount = 65,
                            MainImageUrl = "/images/daikin-ac.png",
                            HighlightsJson = "[\"Công nghệ Inverter tiết kiệm điện\",\"Phin lọc Enzyme Blue + PM2.5\",\"Chế độ Econo tiết kiệm điện năng\",\"Luồng gió Coanda dễ chịu không gây buốt\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"18.000 BTU (2.0 HP)\",\"Công nghệ Inverter\":\"Có (Biến tần Inverter)\",\"Môi chất làm lạnh\":\"Gas R32 thân thiện môi trường\",\"Điện năng tiêu thụ\":\"1.8 kW/h\",\"Diện tích phòng phù hợp\":\"20 - 30 m²\",\"Thời gian bảo hành\":\"12 tháng chính hãng\",\"Nơi sản xuất\":\"Việt Nam\"}"
                        },

                        // --- PANASONIC PRODUCTS ---
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = (await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "panasonic"))?.Id,
                            Brand = "Panasonic",
                            Name = "Điều Hòa Inverter Panasonic 1HP RU9AKH-8",
                            Slug = "dieu-hoa-inverter-panasonic-1hp-ru9akh-8",
                            Sku = "PANASONIC-RU9",
                            Model = "RU9AKH-8",
                            SubTitle = "Công nghệ nanoe-G lọc bụi mịn, ECO tích hợp A.I tiết kiệm điện",
                            Description = "Điều hòa Panasonic Inverter 1 HP RU9AKH-8 sở hữu công nghệ nanoe-G lọc sạch bụi bẩn và bụi mịn PM2.5 bảo vệ sức khỏe gia đình, cùng chế độ Eco tích hợp A.I giúp cân bằng giữa tiết kiệm điện và làm lạnh dễ chịu.",
                            ShortDescription = "Điều hòa Inverter Panasonic 9.000 BTU phù hợp phòng dưới 15m²",
                            Price = 11490000,
                            SalePrice = 9290000,
                            Stock = 18,
                            WarrantyMonths = 12,
                            IsInverter = true,
                            CapacityBtu = 9000,
                            RoomAreaCode = "UNDER_15",
                            GasType = "R32",
                            AverageRating = 4.9,
                            ReviewCount = 22,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 115,
                            MainImageUrl = "/images/panasonic-ac.png",
                            HighlightsJson = "[\"Công nghệ nanoe-G lọc bụi mịn PM2.5\",\"Chế độ ECO tích hợp A.I thông minh\",\"Làm lạnh nhanh Powerful lập tức\",\"Dàn tản nhiệt chống ăn mòn Blue Fin\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"9.000 BTU (1.0 HP)\",\"Công nghệ Inverter\":\"Có Inverter\",\"Môi chất làm lạnh\":\"Gas R32\",\"Điện năng tiêu thụ\":\"0.75 kW/h\",\"Diện tích phòng phù hợp\":\"Dưới 15 m²\",\"Thời gian bảo hành\":\"12 tháng chính hãng\",\"Nơi sản xuất\":\"Việt Nam\"}"
                        },
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = (await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "panasonic"))?.Id,
                            Brand = "Panasonic",
                            Name = "Điều Hòa Inverter Panasonic 1.5HP RU12AKH-8",
                            Slug = "dieu-hoa-inverter-panasonic-1-5hp-ru12akh-8",
                            Sku = "PANASONIC-RU12",
                            Model = "RU12AKH-8",
                            SubTitle = "Công nghệ nanoe-G lọc bụi mịn, ECO tích hợp A.I tiết kiệm điện",
                            Description = "Điều hòa Panasonic Inverter 1.5 HP RU12AKH-8 sở hữu công nghệ nanoe-G lọc sạch bụi bẩn và bụi mịn PM2.5 bảo vệ sức khỏe gia đình, cùng chế độ Eco tích hợp A.I giúp cân bằng giữa tiết kiệm điện và làm lạnh dễ chịu.",
                            ShortDescription = "Điều hòa Inverter Panasonic 12.000 BTU phù hợp phòng 15 - 20m²",
                            Price = 13990000,
                            SalePrice = 11290000,
                            Stock = 14,
                            WarrantyMonths = 12,
                            IsInverter = true,
                            CapacityBtu = 12000,
                            RoomAreaCode = "FROM_15_TO_20",
                            GasType = "R32",
                            AverageRating = 4.8,
                            ReviewCount = 31,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 154,
                            MainImageUrl = "/images/panasonic-ac.png",
                            HighlightsJson = "[\"Công nghệ nanoe-G lọc bụi mịn PM2.5\",\"Chế độ ECO tích hợp A.I thông minh\",\"Làm lạnh nhanh Powerful lập tức\",\"Dàn tản nhiệt chống ăn mòn Blue Fin\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"12.000 BTU (1.5 HP)\",\"Công nghệ Inverter\":\"Có Inverter\",\"Môi chất làm lạnh\":\"Gas R32\",\"Điện năng tiêu thụ\":\"1.15 kW/h\",\"Diện tích phòng phù hợp\":\"15 - 20 m²\",\"Thời gian bảo hành\":\"12 tháng chính hãng\",\"Nơi sản xuất\":\"Việt Nam\"}"
                        },
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = (await context.ProductBrands.FirstOrDefaultAsync(b => b.Slug == "panasonic"))?.Id,
                            Brand = "Panasonic",
                            Name = "Điều Hòa Inverter Panasonic 2HP RU18AKH-8",
                            Slug = "dieu-hoa-inverter-panasonic-2hp-ru18akh-8",
                            Sku = "PANASONIC-RU18",
                            Model = "RU18AKH-8",
                            SubTitle = "Công nghệ nanoe-G lọc bụi mịn, ECO tích hợp A.I tiết kiệm điện",
                            Description = "Điều hòa Panasonic Inverter 2 HP RU18AKH-8 sở hữu công nghệ nanoe-G lọc sạch bụi bẩn và bụi mịn PM2.5 bảo vệ sức khỏe gia đình, cùng chế độ Eco tích hợp A.I giúp cân bằng giữa tiết kiệm điện và làm lạnh dễ chịu.",
                            ShortDescription = "Điều hòa Inverter Panasonic 18.000 BTU phù hợp phòng 20 - 30m²",
                            Price = 21990000,
                            SalePrice = 17990000,
                            Stock = 8,
                            WarrantyMonths = 12,
                            IsInverter = true,
                            CapacityBtu = 18000,
                            RoomAreaCode = "FROM_20_TO_30",
                            GasType = "R32",
                            AverageRating = 4.7,
                            ReviewCount = 16,
                            Status = ProductStatus.Active,
                            IsFeatured = false,
                            SoldCount = 52,
                            MainImageUrl = "/images/panasonic-ac.png",
                            HighlightsJson = "[\"Công nghệ nanoe-G lọc bụi mịn PM2.5\",\"Chế độ ECO tích hợp A.I thông minh\",\"Làm lạnh nhanh Powerful lập tức\",\"Dàn tản nhiệt chống ăn mòn Blue Fin\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"18.000 BTU (2.0 HP)\",\"Công nghệ Inverter\":\"Có Inverter\",\"Môi chất làm lạnh\":\"Gas R32\",\"Điện năng tiêu thụ\":\"1.72 kW/h\",\"Diện tích phòng phù hợp\":\"20 - 30 m²\",\"Thời gian bảo hành\":\"12 tháng chính hãng\",\"Nơi sản xuất\":\"Việt Nam\"}"
                        },

                        // --- LG PRODUCTS ---
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = lgBrand?.Id,
                            Brand = "LG",
                            Name = "Điều Hòa Dual Inverter LG 1HP V10WIN",
                            Slug = "dieu-hoa-lg-dual-inverter-1hp-v10win",
                            Sku = "LG-V10WIN",
                            Model = "V10WIN",
                            SubTitle = "Dual Inverter làm lạnh nhanh hơn 40%, tiết kiệm điện 70%",
                            Description = "Điều hòa LG V10WIN 1HP sử dụng máy nén Dual Inverter làm lạnh nhanh tức thì, hoạt động êm ái và siêu bền bỉ.",
                            ShortDescription = "Điều hòa LG Dual Inverter 9.000 BTU phù hợp phòng dưới 15m²",
                            Price = 9490000,
                            SalePrice = 7290000,
                            Stock = 25,
                            WarrantyMonths = 24,
                            IsInverter = true,
                            CapacityBtu = 9000,
                            RoomAreaCode = "UNDER_15",
                            GasType = "R32",
                            AverageRating = 4.8,
                            ReviewCount = 19,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 89,
                            MainImageUrl = "/images/lg-ac.png",
                            HighlightsJson = "[\"Dual Inverter làm lạnh siêu tốc 40%\",\"Tiết kiệm 70% điện năng tiêu thụ\",\"Màng lọc bụi mịn Pre-Filter\",\"Dàn tản nhiệt mạ vàng Gold Fin bền bỉ\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"9.000 BTU (1.0 HP)\",\"Công nghệ Inverter\":\"Dual Inverter biến tần kép\",\"Môi chất làm lạnh\":\"Gas R32\",\"Điện năng tiêu thụ\":\"0.92 kW/h\",\"Diện tích phòng phù hợp\":\"Dưới 15 m²\",\"Thời gian bảo hành\":\"24 tháng chính hãng\",\"Nơi sản xuất\":\"Thái Lan\"}"
                        },
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = lgBrand?.Id,
                            Brand = "LG",
                            Name = "Điều Hòa Dual Inverter LG 1.5HP V13WIN",
                            Slug = "dieu-hoa-lg-dual-inverter-1-5hp-v13win",
                            Sku = "LG-V13WIN",
                            Model = "V13WIN",
                            SubTitle = "Dual Inverter làm lạnh nhanh hơn 40%, tiết kiệm điện 70%",
                            Description = "Điều hòa LG V13WIN 1.5HP sử dụng máy nén Dual Inverter làm lạnh nhanh tức thì, hoạt động êm ái và siêu bền bỉ.",
                            ShortDescription = "Điều hòa LG Dual Inverter 12.000 BTU phù hợp phòng 15 - 20m²",
                            Price = 11490000,
                            SalePrice = 8890000,
                            Stock = 20,
                            WarrantyMonths = 24,
                            IsInverter = true,
                            CapacityBtu = 12000,
                            RoomAreaCode = "FROM_15_TO_20",
                            GasType = "R32",
                            AverageRating = 4.9,
                            ReviewCount = 25,
                            Status = ProductStatus.Active,
                            IsFeatured = true,
                            SoldCount = 104,
                            MainImageUrl = "/images/lg-ac.png",
                            HighlightsJson = "[\"Dual Inverter làm lạnh siêu tốc 40%\",\"Tiết kiệm 70% điện năng tiêu thụ\",\"Màng lọc bụi mịn Pre-Filter\",\"Dàn tản nhiệt mạ vàng Gold Fin bền bỉ\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"12.000 BTU (1.5 HP)\",\"Công nghệ Inverter\":\"Dual Inverter biến tần kép\",\"Môi chất làm lạnh\":\"Gas R32\",\"Điện năng tiêu thụ\":\"1.15 kW/h\",\"Diện tích phòng phù hợp\":\"15 - 20 m²\",\"Thời gian bảo hành\":\"24 tháng chính hãng\",\"Nơi sản xuất\":\"Thái Lan\"}"
                        },
                        new()
                        {
                            StoreId = demoStore.Id,
                            CategoryId = acCategory.Id,
                            BrandId = lgBrand?.Id,
                            Brand = "LG",
                            Name = "Điều Hòa Dual Inverter LG 2HP V18WIN",
                            Slug = "dieu-hoa-lg-dual-inverter-2hp-v18win",
                            Sku = "LG-V18WIN",
                            Model = "V18WIN",
                            SubTitle = "Dual Inverter làm lạnh nhanh hơn 40%, tiết kiệm điện 70%",
                            Description = "Điều hòa LG V18WIN 2HP sử dụng máy nén Dual Inverter làm lạnh nhanh tức thì, hoạt động êm ái và siêu bền bỉ.",
                            ShortDescription = "Điều hòa LG Dual Inverter 18.000 BTU phù hợp phòng 20 - 30m²",
                            Price = 17990000,
                            SalePrice = 14490000,
                            Stock = 12,
                            WarrantyMonths = 24,
                            IsInverter = true,
                            CapacityBtu = 18000,
                            RoomAreaCode = "FROM_20_TO_30",
                            GasType = "R32",
                            AverageRating = 4.7,
                            ReviewCount = 14,
                            Status = ProductStatus.Active,
                            IsFeatured = false,
                            SoldCount = 47,
                            MainImageUrl = "/images/lg-ac.png",
                            HighlightsJson = "[\"Dual Inverter làm lạnh siêu tốc 40%\",\"Tiết kiệm 70% điện năng tiêu thụ\",\"Màng lọc bụi mịn Pre-Filter\",\"Dàn tản nhiệt mạ vàng Gold Fin bền bỉ\"]",
                            SpecificationsJson = "{\"Công suất làm lạnh\":\"18.000 BTU (2.0 HP)\",\"Công nghệ Inverter\":\"Dual Inverter biến tần kép\",\"Môi chất làm lạnh\":\"Gas R32\",\"Điện năng tiêu thụ\":\"1.65 kW/h\",\"Diện tích phòng phù hợp\":\"20 - 30 m²\",\"Thời gian bảo hành\":\"24 tháng chính hãng\",\"Nơi sản xuất\":\"Thái Lan\"}"
                        }
                    };

                    // Add promotions and reviews to first product
                    var firstAc = productsToSeed.First();
                    firstAc.Promotions.Add(new ProductPromotion
                    {
                        Title = "Miễn phí công lắp đặt + Vận chuyển tại nhà",
                        FreeInstallation = true,
                        GiftItemName = "Gói vật tư phụ 3m ống đồng"
                    });
                    firstAc.Reviews.Add(new ProductReview
                    {
                        StoreId = demoStore.Id,
                        CustomerId = "cust1",
                        CustomerName = "Nguyễn Văn Hùng",
                        Rating = 5,
                        Title = "Máy chạy rất êm và tiết kiệm điện",
                        Comment = "Máy chạy mượt mà, ban đêm siêu êm không nghe thấy tiếng động. Tiền điện tháng vừa rồi giảm rõ rệt.",
                        IsVerifiedPurchase = true
                    });

                    await context.Products.AddRangeAsync(productsToSeed);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Sample products seeded");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding database");
            throw;
        }
    }
}
