using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using web1.Models;
using web1.Infrastructure.Security;

namespace web1.Data.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "web1.Data.ApplicationDbContext";
        }

        protected override void Seed(ApplicationDbContext context)
        {
            var passwordHasher = new PasswordHasherService();

            // Seed admin account
            if (!context.Users.Any(u => u.Username == "admin123"))
            {
                var adminUser = new User
                {
                    Username = "admin123",
                    PasswordHash = passwordHasher.HashPassword("Admin@123456"),
                    Email = "admin@techworld.vn",
                    Phone = "0123456789",
                    FullName = "Administrator",
                    IsVerified = true,
                    OTP = "",
                    OTPExpire = DateTime.Now,
                    Role = "Admin",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            // Seed test user
            if (!context.Users.Any(u => u.Username == "testuser"))
            {
                var testUser = new User
                {
                    Username = "testuser",
                    PasswordHash = passwordHasher.HashPassword("Test@123456"),
                    Email = "testuser@techworld.vn",
                    Phone = "0987654321",
                    FullName = "Test Customer",
                    IsVerified = true,
                    OTP = "",
                    OTPExpire = DateTime.Now,
                    Role = "Customer",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(testUser);
                context.SaveChanges();
            }

            // Seed john_doe user
            if (!context.Users.Any(u => u.Username == "john_doe"))
            {
                var johnUser = new User
                {
                    Username = "john_doe",
                    PasswordHash = passwordHasher.HashPassword("John@123456"),
                    Email = "john@techworld.vn",
                    Phone = "0888888888",
                    FullName = "John Doe",
                    IsVerified = true,
                    OTP = "",
                    OTPExpire = DateTime.Now,
                    Role = "Customer",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(johnUser);
                context.SaveChanges();
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Điện thoại", Type = "Electronics", Description = "Điện thoại di động" },
                    new Category { Name = "Laptop", Type = "Electronics", Description = "Máy tính xách tay" },
                    new Category { Name = "Ipad", Type = "Electronics", Description = "Máy tính bảng" },
                    new Category { Name = "CPU", Type = "Components", Description = "Bộ vi xử lý" },
                    new Category { Name = "GPU", Type = "Components", Description = "Card đồ họa" }
                };

                foreach (var cat in categories)
                {
                    context.Categories.Add(cat);
                }
                context.SaveChanges();
            }

            // Seed Products
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    // Smartphones
                    new Product { CategoryId = 1, Name = "iPhone 15 Pro", Price = 25900000, Description = "Điện thoại flagship Apple với chip A17 Pro", Stock = 50, IsHot = true, Rating = 4.8, ImageUrl = "~/Content/Images/iphone15pro.jpg" },
                    new Product { CategoryId = 1, Name = "Samsung Galaxy S24", Price = 22990000, Description = "Flagship Samsung với AI camera", Stock = 45, IsHot = true, Rating = 4.7, ImageUrl = "~/Content/Images/s24.jpg" },
                    new Product { CategoryId = 1, Name = "Xiaomi 14", Price = 16990000, Description = "Flagship Xiaomi giá rẻ", Stock = 40, Rating = 4.5, ImageUrl = "~/Content/Images/xiaomi14.jpg" },
                    new Product { CategoryId = 1, Name = "OnePlus 12", Price = 18990000, Description = "Điện thoại gaming mạnh mẽ", Stock = 35, Rating = 4.6, ImageUrl = "~/Content/Images/oneplus12.jpg" },
                    new Product { CategoryId = 1, Name = "Google Pixel 8", Price = 19990000, Description = "Pixel cao cấp với AI photography", Stock = 30, Rating = 4.4, ImageUrl = "~/Content/Images/pixel8.jpg" },
                    new Product { CategoryId = 1, Name = "Oppo Find X7", Price = 21990000, Description = "Oppo flagship 2024", Stock = 28, Rating = 4.3, ImageUrl = "~/Content/Images/oppofindx7.jpg" },

                    // Laptops
                    new Product { CategoryId = 2, Name = "MacBook Pro 16\"", Price = 47990000, Description = "Laptop cao cấp từ Apple với M3 Max", Stock = 20, IsHot = true, Rating = 5.0, ImageUrl = "~/Content/Images/mbp16.jpg" },
                    new Product { CategoryId = 2, Name = "Dell XPS 15", Price = 35990000, Description = "Laptop cao cấp Dell dành cho chuyên gia", Stock = 18, Rating = 4.7, ImageUrl = "~/Content/Images/xps15.jpg" },
                    new Product { CategoryId = 2, Name = "ASUS VivoBook 15", Price = 18990000, Description = "Laptop tầm trung chất lượng cao", Stock = 25, Rating = 4.2, ImageUrl = "~/Content/Images/vivobook15.jpg" },
                    new Product { CategoryId = 2, Name = "Lenovo ThinkPad X1", Price = 42990000, Description = "Laptop doanh nhân cao cấp", Stock = 15, Rating = 4.8, ImageUrl = "~/Content/Images/thinkpadx1.jpg" },
                    new Product { CategoryId = 2, Name = "HP Pavilion 15", Price = 16990000, Description = "Laptop phổ thông giá tốt", Stock = 30, Rating = 4.0, ImageUrl = "~/Content/Images/pavilion15.jpg" },
                    new Product { CategoryId = 2, Name = "MSI GE76 Raider", Price = 38990000, Description = "Laptop gaming hiệu năng cao", Stock = 12, IsHot = true, Rating = 4.9, ImageUrl = "~/Content/Images/ge76.jpg" },

                    // Tablets
                    new Product { CategoryId = 3, Name = "iPad Pro 12.9\"", Price = 32990000, Description = "iPad Pro màn hình lớn 2024", Stock = 16, IsHot = true, Rating = 4.9, ImageUrl = "~/Content/Images/ipadpro129.jpg" },
                    new Product { CategoryId = 3, Name = "iPad Air 11\"", Price = 20990000, Description = "iPad Air thiết kế nhỏ gọn", Stock = 20, Rating = 4.6, ImageUrl = "~/Content/Images/ipadair11.jpg" },
                    new Product { CategoryId = 3, Name = "Samsung Galaxy Tab S9", Price = 18990000, Description = "Tablet Samsung cao cấp", Stock = 22, Rating = 4.5, ImageUrl = "~/Content/Images/tabs9.jpg" },
                    new Product { CategoryId = 3, Name = "Xiaomi Pad 6", Price = 12990000, Description = "Tablet Xiaomi giá rẻ hiệu năng tốt", Stock = 35, Rating = 4.3, ImageUrl = "~/Content/Images/xiaomipad6.jpg" },

                    // CPUs
                    new Product { CategoryId = 4, Name = "Intel Core i9-14900K", Price = 15990000, Description = "Bộ vi xử lý cao cấp Intel thế hệ 14", Stock = 10, IsHot = true, Rating = 4.8, ImageUrl = "~/Content/Images/i914900k.jpg" },
                    new Product { CategoryId = 4, Name = "AMD Ryzen 9 9900X", Price = 14990000, Description = "CPU cao cấp AMD Ryzen 9", Stock = 12, Rating = 4.9, ImageUrl = "~/Content/Images/r99900x.jpg" },
                    new Product { CategoryId = 4, Name = "Intel Core i7-14700K", Price = 10990000, Description = "Bộ vi xử lý tầm trung cao", Stock = 15, Rating = 4.7, ImageUrl = "~/Content/Images/i714700k.jpg" },
                    new Product { CategoryId = 4, Name = "AMD Ryzen 7 9700X", Price = 9990000, Description = "CPU tầm trung cao AMD", Stock = 18, Rating = 4.6, ImageUrl = "~/Content/Images/r79700x.jpg" },

                    // GPUs
                    new Product { CategoryId = 5, Name = "RTX 4090", Price = 49990000, Description = "Card đồ họa gaming cao cấp nhất", Stock = 8, IsHot = true, Rating = 5.0, ImageUrl = "~/Content/Images/rtx4090.jpg" },
                    new Product { CategoryId = 5, Name = "RTX 4080 Super", Price = 34990000, Description = "Card đồ họa gaming cao cấp", Stock = 10, Rating = 4.8, ImageUrl = "~/Content/Images/rtx4080s.jpg" },
                    new Product { CategoryId = 5, Name = "RTX 4070 Ti", Price = 24990000, Description = "Card đồ họa gaming tầm trung cao", Stock = 15, Rating = 4.5, ImageUrl = "~/Content/Images/rtx4070ti.jpg" },
                    new Product { CategoryId = 5, Name = "RTX 4060 Ti", Price = 9990000, Description = "Card đồ họa entry-level gaming", Stock = 20, Rating = 4.2, ImageUrl = "~/Content/Images/rtx4060ti.jpg" }
                };

                foreach (var product in products)
                {
                    context.Products.Add(product);
                }
                context.SaveChanges();
            }

            // Seed News
            if (!context.News.Any())
            {
                var newsList = new List<News>
                {
                    new News { Title = "Apple ra mắt iPhone 15 Pro với chip A17 Pro", Content = "Apple chính thức thiết lập tiêu chuẩn mới cho dòng flagship với iPhone 15 Pro. Điểm nhấn lớn nhất là chip A17 Pro - chip 3nm đầu tiên trên thế giới, mang lại hiệu năng đồ họa đột phá cho gaming.", Category = "Technology", PublishDate = DateTime.Now.AddDays(-5), ImageUrl = "~/Content/Images/news1.jpg", Summary = "iPhone 15 Pro với chip A17 Pro" },
                    new News { Title = "Samsung Galaxy S24 lên kệ tại Việt Nam", Content = "Thế hệ Galaxy S24 Series đã chính thức cập bến thị trường Việt Nam, đánh dấu kỷ nguyên điện thoại AI.", Category = "Technology", PublishDate = DateTime.Now.AddDays(-4), ImageUrl = "~/Content/Images/news2.jpg", Summary = "Galaxy S24 tại Việt Nam" },
                    new News { Title = "Quân đội hiện đại hóa vũ khí với công nghệ AI", Content = "Cuộc chạy đua vũ trang đang chuyển dịch sang lĩnh vực kỹ thuật số khi nhiều quốc gia tích hợp trí tuệ nhân tạo.", Category = "Defense", PublishDate = DateTime.Now.AddDays(-3), ImageUrl = "~/Content/Images/news3.jpg", Summary = "AI trong quân sự" },
                    new News { Title = "Nvidia phát hành RTX 4090 Super", Content = "Nvidia tiếp tục nới rộng khoảng cách với các đối thủ khi tung ra biến thể RTX 4090 Super.", Category = "Technology", PublishDate = DateTime.Now.AddDays(-2), ImageUrl = "~/Content/Images/news4.jpg", Summary = "RTX 4090 Super tới" },
                    new News { Title = "Máy bay chiến đấu thế hệ 6 của Mỹ", Content = "Lực lượng Không quân Mỹ vừa hé lộ những hình ảnh phác thảo đầu tiên về chương trình NGAD.", Category = "Defense", PublishDate = DateTime.Now.AddDays(-1), ImageUrl = "~/Content/Images/news5.jpg", Summary = "Máy bay thế hệ 6" },
                    new News { Title = "Intel Core i9-14900K được giải thích chi tiết", Content = "Bộ vi xử lý Intel Core i9-14900K thuộc dòng Raptor Lake Refresh đã chính thức ra mắt.", Category = "Technology", PublishDate = DateTime.Now, ImageUrl = "~/Content/Images/news6.jpg", Summary = "Intel i9-14900K chi tiết" },
                    new News { Title = "Công nghệ radar phát hiện tàng hình mới", Content = "Một bước đột phá trong công nghệ quốc phòng khi các nhà khoa học công bố loại radar lượng tử.", Category = "Defense", PublishDate = DateTime.Now.AddHours(-12), ImageUrl = "~/Content/Images/news7.jpg", Summary = "Radar phát hiện tàng hình" },
                    new News { Title = "Thị trường smartphone toàn cầu tăng trưởng 8%", Content = "Sau giai đoạn trì trệ, thị trường smartphone toàn cầu năm 2024 đã ghi nhận mức tăng trưởng ấn tượng.", Category = "Technology", PublishDate = DateTime.Now.AddHours(-18), ImageUrl = "~/Content/Images/news8.jpg", Summary = "Tăng trưởng smartphone" },
                    new News { Title = "Hàn Quốc phát triển robot quân sự hành động độc lập", Content = "Cơ quan Quản lý Chương trình Tiếp thu Quốc phòng Hàn Quốc (DAPA) vừa công bố mẫu robot.", Category = "Defense", PublishDate = DateTime.Now.AddHours(-20), ImageUrl = "~/Content/Images/news9.jpg", Summary = "Robot quân sự Hàn Quốc" },
                    new News { Title = "Apple M3 Max đạt hiệu năng hơn Core i9", Content = "Các bài kiểm tra Benchmark Geekbench mới nhất cho thấy chip M3 Max vượt qua Core i9.", Category = "Technology", PublishDate = DateTime.Now.AddHours(-24), ImageUrl = "~/Content/Images/news10.jpg", Summary = "M3 Max vs i9" },
                    new News { Title = "Biển Đông: Các nước tăng cường triển khai công nghệ", Content = "Tình hình Biển Đông tiếp tục thu hút sự chú ý khi các quốc gia ven biển nâng cấp.", Category = "Defense", PublishDate = DateTime.Now.AddHours(-30), ImageUrl = "~/Content/Images/news11.jpg", Summary = "Công nghệ giám sát Biển Đông" },
                    new News { Title = "Dell XPS 15 được nâng cấp với CPU Intel mới", Content = "Dell vừa làm mới dòng laptop cao cấp nhất của mình - XPS 15.", Category = "Technology", PublishDate = DateTime.Now.AddHours(-36), ImageUrl = "~/Content/Images/news12.jpg", Summary = "Dell XPS 15 nâng cấp" },
                    new News { Title = "Ấn Độ phát triển tên lửa hành trình mạnh hơn", Content = "Bộ Quốc phòng Ấn Độ thông báo thử nghiệm thành công biến thể mới.", Category = "Defense", PublishDate = DateTime.Now.AddHours(-42), ImageUrl = "~/Content/Images/news13.jpg", Summary = "Tên lửa hành trình Ấn Độ" },
                    new News { Title = "AMD Ryzen 7 9700X hiệu năng giá tốt", Content = "AMD tiếp tục gây áp lực lên đối thủ với dòng Ryzen 7 9700X.", Category = "Technology", PublishDate = DateTime.Now.AddHours(-48), ImageUrl = "~/Content/Images/news14.jpg", Summary = "Ryzen 7 9700X" },
                    new News { Title = "NATO tập trung công nghệ cyber chiến", Content = "Trong bối cảnh các cuộc tấn công mạng gia tăng, NATO quyết định thành lập trung tâm.", Category = "Defense", PublishDate = DateTime.Now.AddHours(-54), ImageUrl = "~/Content/Images/news15.jpg", Summary = "NATO cyber chiến" }
                };

                foreach (var news in newsList)
                {
                    context.News.Add(news);
                }
                context.SaveChanges();
            }

            // Seed Lucky Spin Prizes
            if (!context.LuckySpinPrizes.Any())
            {
                var voucher5 = new Voucher
                {
                    Code = "SALE5",
                    Name = "Giảm 5%",
                    Description = "Giảm 5% cho đơn hàng",
                    DiscountPercent = 5,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    Quantity = 999,
                    IsActive = true,
                    IsLuckySpinVoucher = false,
                    CreatedAt = DateTime.Now
                };

                var voucher10 = new Voucher
                {
                    Code = "SALE10",
                    Name = "Giảm 10%",
                    Description = "Giảm 10% cho đơn hàng",
                    DiscountPercent = 10,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    Quantity = 999,
                    IsActive = true,
                    IsLuckySpinVoucher = false,
                    CreatedAt = DateTime.Now
                };

                var voucher20 = new Voucher
                {
                    Code = "SALE20",
                    Name = "Giảm 20%",
                    Description = "Giảm 20% cho đơn hàng",
                    DiscountPercent = 20,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1),
                    Quantity = 999,
                    IsActive = true,
                    IsLuckySpinVoucher = false,
                    CreatedAt = DateTime.Now
                };

                context.Vouchers.Add(voucher5);
                context.Vouchers.Add(voucher10);
                context.Vouchers.Add(voucher20);
                context.SaveChanges();

                var prizes = new[]
                {
                    new LuckySpinPrize
                    {
                        Name = "Chúc bạn may mắn lần sau",
                        Description = "Không trúng giải, hãy thử lại lần tới",
                        WinRate = 54.3m,
                        PrizeType = "Message",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new LuckySpinPrize
                    {
                        Name = "Voucher 5%",
                        Description = "Giảm 5% cho lần mua tiếp theo",
                        WinRate = 12.7m,
                        PrizeType = "Voucher",
                        VoucherId = voucher5.Id,
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new LuckySpinPrize
                    {
                        Name = "Voucher 10%",
                        Description = "Giảm 10% cho lần mua tiếp theo",
                        WinRate = 23m,
                        PrizeType = "Voucher",
                        VoucherId = voucher10.Id,
                        DisplayOrder = 3,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new LuckySpinPrize
                    {
                        Name = "Voucher 20%",
                        Description = "Giảm 20% cho lần mua tiếp theo",
                        WinRate = 9.99998m,
                        PrizeType = "Voucher",
                        VoucherId = voucher20.Id,
                        DisplayOrder = 4,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    },
                    new LuckySpinPrize
                    {
                        Name = "Quà tặng đặc biệt",
                        Description = "Nhận một sản phẩm bất kỳ từ kho hàng",
                        WinRate = 0.00002m,
                        PrizeType = "Product",
                        ProductId = null,
                        DisplayOrder = 5,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    }
                };

                foreach (var prize in prizes)
                {
                    context.LuckySpinPrizes.Add(prize);
                }

                context.SaveChanges();
            }
        }
    }
}
