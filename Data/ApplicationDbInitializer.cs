using System;
using System.Collections.Generic;
using System.Data.Entity;
using web1.Helpers;
using web1.Models;

namespace web1.Data
{
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            // Create Categories
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

            // Create Products (Đã thêm Rating)
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

            // Create News
            var newsList = new List<News>
            {
                new News {
                    Title = "Apple ra mắt iPhone 15 Pro với chip A17 Pro",
                    Content = "Apple chính thức thiết lập tiêu chuẩn mới cho dòng flagship với iPhone 15 Pro. Điểm nhấn lớn nhất là chip A17 Pro - chip 3nm đầu tiên trên thế giới, mang lại hiệu năng đồ họa đột phá cho gaming. Bên cạnh đó, khung vỏ Titanium chuẩn hàng không vũ trụ giúp máy nhẹ hơn đáng kể nhưng bền bỉ hơn, kết hợp cùng hệ thống camera tele 5x độc quyền trên dòng Pro Max và nút Action tùy chỉnh linh hoạt.",
                    Category = "Technology",
                    PublishDate = DateTime.Now.AddDays(-5),
                    ImageUrl = "~/Content/Images/news1.jpg",
                    Summary = "iPhone 15 Pro với chip A17 Pro"
                },
                new News {
                    Title = "Samsung Galaxy S24 lên kệ tại Việt Nam",
                    Content = "Thế hệ Galaxy S24 Series đã chính thức cập bến thị trường Việt Nam, đánh dấu kỷ nguyên điện thoại AI. Với Galaxy AI, người dùng có thể trải nghiệm các tính năng thông minh như khoanh vùng tìm kiếm đa năng, phiên dịch trực tiếp cuộc gọi và trợ lý chỉnh ảnh chuyên nghiệp. Phiên bản Ultra sở hữu khung viền Titan và chip Snapdragon 8 Gen 3 được tối ưu riêng, hứa hẹn thống trị phân khúc cao cấp.",
                    Category = "Technology",
                    PublishDate = DateTime.Now.AddDays(-4),
                    ImageUrl = "~/Content/Images/news2.jpg",
                    Summary = "Galaxy S24 tại Việt Nam"
                },
                new News {
                    Title = "Quân đội hiện đại hóa vũ khí với công nghệ AI",
                    Content = "Cuộc chạy đua vũ trang đang chuyển dịch sang lĩnh vực kỹ thuật số khi nhiều quốc gia tích hợp trí tuệ nhân tạo (AI) vào hệ thống phòng thủ. AI không chỉ giúp phân tích dữ liệu tình báo từ vệ tinh trong thời gian thực mà còn tối ưu hóa quỹ đạo bay của tên lửa và hỗ trợ ra quyết định trên chiến trường nhanh gấp nhiều lần con người, giúp giảm thiểu rủi ro cho binh sĩ trong các vùng chiến sự.",
                    Category = "Defense",
                    PublishDate = DateTime.Now.AddDays(-3),
                    ImageUrl = "~/Content/Images/news3.jpg",
                    Summary = "AI trong quân sự"
                },
                new News {
                    Title = "Nvidia phát hành RTX 4090 Super",
                    Content = "Nvidia tiếp tục nới rộng khoảng cách với các đối thủ khi tung ra biến thể RTX 4090 Super. Được trang bị số nhân CUDA khổng lồ và bộ nhớ VRAM tốc độ cao, dòng card này không chỉ phục vụ các game thủ khát khao độ phân giải 4K mà còn là 'quái vật' trong việc đào tạo các mô hình AI ngôn ngữ lớn. Công nghệ DLSS 3.5 mới giúp tăng khung hình lên gấp đôi trong khi vẫn duy trì chất lượng hình ảnh sắc nét.",
                    Category = "Technology",
                    PublishDate = DateTime.Now.AddDays(-2),
                    ImageUrl = "~/Content/Images/news4.jpg",
                    Summary = "RTX 4090 Super tới"
                },
                new News {
                    Title = "Máy bay chiến đấu thế hệ 6 của Mỹ",
                    Content = "Lực lượng Không quân Mỹ vừa hé lộ những hình ảnh phác thảo đầu tiên về chương trình NGAD (Next Generation Air Dominance). Đây là mẫu tiêm kích thế hệ thứ 6 được thiết kế để hoạt động như một trung tâm điều khiển, phối hợp cùng các phi đội máy bay không người lái hộ tống. Máy bay sở hữu khả năng tàng hình kỹ thuật số, hệ thống tác chiến điện tử tiên tiến và động cơ chu kỳ biến đổi cực kỳ tiết kiệm nhiên liệu.",
                    Category = "Defense",
                    PublishDate = DateTime.Now.AddDays(-1),
                    ImageUrl = "~/Content/Images/news5.jpg",
                    Summary = "Máy bay thế hệ 6"
                },
                new News {
                    Title = "Intel Core i9-14900K được giải thích chi tiết",
                    Content = "Bộ vi xử lý Intel Core i9-14900K thuộc dòng Raptor Lake Refresh đã chính thức ra mắt với xung nhịp lên tới 6.0GHz ngay từ khi xuất xưởng. Với kiến trúc lai tối ưu hóa, CPU này mang lại hiệu suất đơn nhân vượt trội cho các tác vụ sáng tạo nội dung và gaming đỉnh cao. Công nghệ Application Optimization (APO) mới giúp phân bổ nguồn lực xử lý thông minh, giúp tăng hiệu năng thực tế lên tới 30% trong các tựa game được hỗ trợ.",
                    Category = "Technology",
                    PublishDate = DateTime.Now,
                    ImageUrl = "~/Content/Images/news6.jpg",
                    Summary = "Intel i9-14900K chi tiết"
                },
                new News {
                    Title = "Công nghệ radar phát hiện tàng hình mới",
                    Content = "Một bước đột phá trong công nghệ quốc phòng khi các nhà khoa học công bố loại radar lượng tử có khả năng phát hiện các vật thể có diện tích phản xạ radar siêu nhỏ. Công nghệ này có thể 'vô hiệu hóa' ưu thế của các tàu chiến và máy bay tàng hình hiện nay bằng cách phát hiện các nhiễu động cực nhỏ trong môi trường. Đây được xem là quân bài chiến lược trong việc bảo vệ không phận và hải phận quốc gia.",
                    Category = "Defense",
                    PublishDate = DateTime.Now.AddHours(-12),
                    ImageUrl = "~/Content/Images/news7.jpg",
                    Summary = "Radar phát hiện tàng hình"
                },
                new News {
                    Title = "Thị trường smartphone toàn cầu tăng trưởng 8%",
                    Content = "Sau giai đoạn trì trệ, thị trường smartphone toàn cầu năm 2024 đã ghi nhận mức tăng trưởng ấn tượng 8%. Sự thúc đẩy này đến từ làn sóng điện thoại tích hợp AI và nhu cầu nâng cấp thiết bị 5G tại các thị trường mới nổi như Đông Nam Á và Ấn Độ. Các nhà phân tích dự báo xu hướng smartphone màn hình gập và các thiết bị hỗ trợ kết nối vệ tinh sẽ tiếp tục dẫn dắt thị trường trong nửa cuối năm nay.",
                    Category = "Technology",
                    PublishDate = DateTime.Now.AddHours(-18),
                    ImageUrl = "~/Content/Images/news8.jpg",
                    Summary = "Tăng trưởng smartphone"
                },
                new News {
                    Title = "Hàn Quốc phát triển robot quân sự hành động độc lập",
                    Content = "Cơ quan Quản lý Chương trình Tiếp thu Quốc phòng Hàn Quốc (DAPA) vừa công bố mẫu robot bốn chân thế hệ mới tích hợp AI. Robot này có khả năng tự điều hướng trong môi trường phức tạp, nhận diện mục tiêu và đưa ra các đề xuất tác chiến cho chỉ huy. Với khả năng mang vác vũ khí hạng nhẹ và cảm biến nhiệt, đây sẽ là trợ thủ đắc lực trong các nhiệm vụ tuần tra biên giới và cứu hộ trong điều kiện khắc nghiệt.",
                    Category = "Defense",
                    PublishDate = DateTime.Now.AddHours(-20),
                    ImageUrl = "~/Content/Images/news9.jpg",
                    Summary = "Robot quân sự Hàn Quốc"
                },
                new News {
                    Title = "Apple M3 Max đạt hiệu năng hơn Core i9",
                    Content = "Các bài kiểm tra Benchmark Geekbench mới nhất cho thấy chip M3 Max trên MacBook Pro 16 inch đã đạt được điểm số đa nhân vượt qua cả CPU desktop Core i9-13900K trong một số tác vụ render video. Nhờ cấu trúc GPU thế hệ mới với công nghệ Dynamic Caching, M3 Max không chỉ mạnh mà còn cực kỳ tiết kiệm điện năng, biến nó trở thành lựa chọn hàng đầu cho các nhà làm phim và kỹ sư đồ họa chuyên nghiệp.",
                    Category = "Technology",
                    PublishDate = DateTime.Now.AddHours(-24),
                    ImageUrl = "~/Content/Images/news10.jpg",
                    Summary = "M3 Max vs i9"
                },
                new News {
                    Title = "Biển Đông: Các nước tăng cường triển khai công nghệ giám sát",
                    Content = "Tình hình Biển Đông tiếp tục thu hút sự chú ý khi các quốc gia ven biển đồng loạt nâng cấp hệ thống giám sát hàng hải. Các công nghệ mới bao gồm tàu lặn không người lái (UUV) và hệ thống camera hồng ngoại tầm xa có khả năng quan sát xuyên đêm và sương mù. Mục tiêu của việc triển khai này là nhằm nâng cao năng lực nhận thức tình huống, bảo vệ tài nguyên biển và đảm bảo an ninh chủ quyền một cách minh bạch và hiệu quả.",
                    Category = "Defense",
                    PublishDate = DateTime.Now.AddHours(-30),
                    ImageUrl = "~/Content/Images/news11.jpg",
                    Summary = "Công nghệ giám sát Biển Đông"
                },
                new News {
                    Title = "Dell XPS 15 được nâng cấp với CPU Intel mới nhất",
                    Content = "Dell vừa làm mới dòng laptop cao cấp nhất của mình - XPS 15 với cấu hình cực khủng. Máy hiện đã cho phép tùy chọn CPU Intel Core i9 thế hệ 14 mới nhất đi kèm card đồ họa RTX 4090. Màn hình InfinityEdge OLED 3.5K vẫn là điểm ăn tiền nhất, mang lại độ chính xác màu sắc tuyệt đối cho dân thiết kế. Hệ thống tản nhiệt cũng được tái thiết kế để đảm bảo hiệu năng ổn định trong thời gian dài làm việc cường độ cao.",
                    Category = "Technology",
                    PublishDate = DateTime.Now.AddHours(-36),
                    ImageUrl = "~/Content/Images/news12.jpg",
                    Summary = "Dell XPS 15 nâng cấp"
                },
                new News {
                    Title = "Ấn Độ phát triển tên lửa hành trình mạnh hơn",
                    Content = "Bộ Quốc phòng Ấn Độ thông báo thử nghiệm thành công biến thể mới của dòng tên lửa hành trình nội địa. Với tầm bắn được mở rộng lên 2000 km và tích hợp đầu dò radar chủ động, tên lửa có thể xuyên thủng các hệ thống phòng không hiện đại nhất. Đây là một phần trong chiến lược 'Make in India' nhằm tự chủ công nghệ quốc phòng hạng nặng và tăng cường khả năng răn đe trong khu vực.",
                    Category = "Defense",
                    PublishDate = DateTime.Now.AddHours(-42),
                    ImageUrl = "~/Content/Images/news13.jpg",
                    Summary = "Tên lửa hành trình Ấn Độ"
                },
                new News {
                    Title = "AMD Ryzen 7 9700X hiệu năng giá tốt",
                    Content = "AMD tiếp tục gây áp lực lên đối thủ với dòng Ryzen 7 9700X dựa trên kiến trúc Zen 5. Với tiến trình sản xuất tiên tiến, CPU này không chỉ cải thiện chỉ số IPC (lệnh trên mỗi chu kỳ) mà còn duy trì mức tiêu thụ điện năng cực thấp. Đây được coi là 'vị vua mới' ở phân khúc tầm trung, mang lại hiệu năng chơi game tương đương các dòng cao cấp của thế hệ trước với mức giá dễ tiếp cận hơn rất nhiều.",
                    Category = "Technology",
                    PublishDate = DateTime.Now.AddHours(-48),
                    ImageUrl = "~/Content/Images/news14.jpg",
                    Summary = "Ryzen 7 9700X"
                },
                new News {
                    Title = "NATO tập trung công nghệ cyber chiến",
                    Content = "Trong bối cảnh các cuộc tấn công mạng nhắm vào hạ tầng trọng yếu gia tăng, liên minh NATO đã quyết định thành lập trung tâm tác chiến không gian mạng mới. Trọng tâm của chiến lược này là phát triển các công cụ phòng thủ chủ động dựa trên AI, có khả năng phát hiện và ngăn chặn mã độc trước khi chúng xâm nhập vào hệ thống lưới điện hoặc mạng lưới liên lạc quân sự, đảm bảo an ninh tuyệt đối cho các quốc gia thành viên.",
                    Category = "Defense",
                    PublishDate = DateTime.Now.AddHours(-54),
                    ImageUrl = "~/Content/Images/news15.jpg",
                    Summary = "NATO cyber chiến"
                }
            };

            foreach (var news in newsList)
            {
                context.News.Add(news);
            }
            context.SaveChanges();

            // ⭐ SEED ADMIN ACCOUNT WITH NEW PASSWORD HASHER
            AdminAccountSeeder.SeedAdminAccount(context);
            AdminAccountSeeder.SeedTestAccounts(context);

            base.Seed(context);
        }
    }
}