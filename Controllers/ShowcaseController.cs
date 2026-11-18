using System;
using System.Linq;
using System.Web.Mvc;
using web1.Models;
using web1.Data;
using System.Data.Entity;
using System.IO;
using System.Web;
using web1.Helpers;

namespace web1.Controllers
{
    public class ShowcaseController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ─────────────────────────────────────────────────
        //  LANDING GATEWAY — Hiển thị Landing trước Showcase (VÔ HẠN)
        // ─────────────────────────────────────────────────
        public ActionResult ShowcaseLanding()
        {
            // Hiển thị Landing mỗi lần user vào Showcase (VÔ HẠN - không có cookie)
            return View("~/Views/Shared/Landing.cshtml");
        }

        // ─────────────────────────────────────────────────
        //  INDEX  — danh sách trưng bày
        // ─────────────────────────────────────────────────
        public ActionResult Index()
        {
            var products = db.Products
                .Where(p =>
                    p.Name.Contains("iPhone") ||
                    p.Name.Contains("Samsung") ||
                    p.Name.Contains("Galaxy") ||
                    p.Name.Contains("Huawei") ||
                    p.Name.Contains("Mate") ||
                    p.Name.Contains("iPad") ||
                    p.Name.Contains("MacBook") ||
                    p.Name.Contains("Laptop") ||
                    p.Name.Contains("Pro"))
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Take(24)
                .ToList();

            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.ImageUrl))
                {
                    string n = product.Name.ToLower();
                    if (n.Contains("iphone 15 pro")) product.ImageUrl = "/Content/Images/iphone15pro.jpg";
                    else if (n.Contains("iphone")) product.ImageUrl = "/Content/Images/iphone.jpg";
                    else if (n.Contains("ipad")) product.ImageUrl = "/Content/Images/ipad.jpg";
                    else if (n.Contains("macbook")) product.ImageUrl = "/Content/Images/macbook.jpg";
                    else if (n.Contains("laptop")) product.ImageUrl = "/Content/Images/laptop.jpg";
                    else if (n.Contains("samsung") || n.Contains("galaxy")) product.ImageUrl = "/Content/Images/samsung.jpg";
                    else if (n.Contains("huawei") || n.Contains("mate")) product.ImageUrl = "/Content/Images/huawei.jpg";
                }
            }

            ViewBag.Title = "Trưng Bày Sản Phẩm";
            return View(products);
        }

        // ─────────────────────────────────────────────────
        //  DETAIL  — trưng bày 3D
        // ─────────────────────────────────────────────────
        public ActionResult Detail(int id)
        {
            var product = db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();

            ViewBag.Title = $"Trưng Bày - {product.Name}";
            return View(product);
        }

        // ─────────────────────────────────────────────────
        //  GET SPECS  — AJAX
        // ─────────────────────────────────────────────────
        [HttpGet]
        public JsonResult GetProductSpecs(int id)
        {
            var product = db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
            if (product == null)
                return Json(new { error = "Product not found" }, JsonRequestBehavior.AllowGet);

            var specs = BuildSpecs(product.Name);
            return Json(specs, JsonRequestBehavior.AllowGet);
        }

        // ─────────────────────────────────────────────────
        //  MAIN ROUTER
        // ─────────────────────────────────────────────────
        private object BuildSpecs(string name)
        {
            string n = name.ToLower();

            if (n.Contains("macbook") || n.Contains("mac book")) return SpecsMacBook(name);
            if (n.Contains("ipad")) return SpecsIPad(name);
            if (n.Contains("laptop")) return SpecsLaptop(name);
            if (n.Contains("iphone")) return SpecsIPhone(name);
            if (n.Contains("samsung") || n.Contains("galaxy")) return SpecsSamsung(name);
            if (n.Contains("huawei") || n.Contains("mate")) return SpecsHuawei(name);
            return SpecsDefault(name);
        }

        // ─────────────────────────────────────────────────
        //  SAMSUNG / GALAXY
        // ─────────────────────────────────────────────────
        private object SpecsSamsung(string name)
        {
            bool isUltra = name.Contains("Ultra");
            bool isPlus = name.Contains("+") || name.Contains("Plus");

            return new
            {
                product = name,
                specs = new
                {
                    camera = new
                    {
                        name = "Hệ thống Camera",
                        sub = isUltra ? "200MP · Space Zoom 100x · Nightography" : "Camera AI · Nightography · Multi-lens",
                        iconBg = "rgba(90,50,5,.85)",
                        iconColor = "rgba(255,195,70,.95)",
                        iconSymbol = "CAM",
                        sections = new dynamic[] {
                            new { label="Camera chính",           val=isUltra ? "200MP · f/1.7 · OIS · PDAF · Laser AF\nCảm biến 1/1.3\" — lớn nhất dòng Galaxy" : "50MP · f/1.7–f/2.0 · OIS · Sensor AF",                            bar=isUltra?98:90, barColor="#ef9f27" },
                            new { label="Camera góc siêu rộng",   val="12MP · f/2.2 · góc 120° · Macro mode\nNightography AI",                                                                                                           bar=85, barColor="#fac775" },
                            new { label="Camera Tele",             val=isUltra ? "10MP 3x + 50MP 5x Periscope OIS\nSpace Zoom kỹ thuật số lên tới 100x" : "10MP · 3x Zoom quang học · OIS",                                            bar=isUltra?94:80, barColor="#ef9f27" },
                            new { label="Video & AI",              val="8K@30fps · 4K@120fps · HDR10+\nGenerative Edit · Circle to Search · AI Enhancement",  tags=new[]{"Nightography","8K Video","100x Zoom","AI Edit","ProVisual"} }
                        }
                    },
                    chip = new
                    {
                        name = "Snapdragon 8 Gen 3",
                        sub = "4nm · Galaxy Edition · 45 TOPS AI",
                        iconBg = "rgba(15,40,115,.85)",
                        iconColor = "rgba(120,180,255,.95)",
                        iconSymbol = "CPU",
                        sections = new dynamic[] {
                            new { label="CPU — 8 nhân",   val="1× Prime 3.39GHz + 3× Perf 3.1GHz\n2× Eff 2.96GHz + 2× Eff 2.27GHz",               bar=99, barColor="#378add" },
                            new { label="GPU Adreno 750", val="Hiệu suất GPU tăng 25% · Ray Tracing\nVulkan 1.3 · Gaming 120fps ổn định",            bar=96, barColor="#85b7eb" },
                            new { label="NPU — 45 TOPS",  val="Hexagon NPU · Galaxy AI on-device\nXử lý LLM · Không cần kết nối cloud",              bar=97, barColor="#378add" },
                            new { label="Benchmark",      val="AnTuTu v10: ~2,100,000 · Geekbench Single: 2,200+", tags=new[]{"4nm","45 TOPS","Wi-Fi 7","Ray Tracing","Knox Vault"} }
                        }
                    },
                    battery = new
                    {
                        name = isUltra ? "Pin 5000mAh" : "Pin 4000–5000mAh",
                        sub = "Sạc nhanh 45W · Wireless 15W",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(80,220,130,.95)",
                        iconSymbol = "PWR",
                        sections = new dynamic[] {
                            new { label="Dung lượng",     val=isUltra ? "5000mAh · Silicon-carbon · mật độ +10%" : "4000–5000mAh · tối ưu theo dòng máy",          bar=isUltra?88:82, barColor="#1d9e75" },
                            new { label="Sạc có dây 45W", val="0% → 50% trong 35 phút · Đầy trong ~70 phút",                                                        bar=80, barColor="#5dcaa5" },
                            new { label="Sạc không dây",  val="15W Wireless · 4.5W Reverse cho Buds & Watch",                                                        bar=65, barColor="#9fe1cb" },
                            new { label="Thời lượng",     val="Video streaming: ~26 giờ · Dùng hỗn hợp: 2 ngày", tags=new[]{isUltra?"5000mAh":"4000mAh+","45W Fast","15W Wireless","2 ngày"} }
                        }
                    },
                    ram = new
                    {
                        name = "RAM 12GB LPDDR5X",
                        sub = "UFS 4.0 · 256GB / 512GB / 1TB",
                        iconBg = "rgba(50,15,100,.85)",
                        iconColor = "rgba(190,130,255,.95)",
                        iconSymbol = "RAM",
                        sections = new dynamic[] {
                            new { label="RAM LPDDR5X",     val="12GB · 85.3 GB/s · nhanh hơn 50% so với LPDDR5",        bar=94, barColor="#7f77dd" },
                            new { label="UFS 4.0",         val="Đọc tuần tự: 4,200 MB/s · 10,000+ IOPS",                bar=97, barColor="#afa9ec" },
                            new { label="Virtual RAM",     val="RAM Plus 4–16GB từ bộ nhớ trong",                        bar=85, barColor="#7f77dd" },
                            new { label="Đa nhiệm",        val="30+ ứng dụng background · Gaming 3D@120fps ổn định", tags=new[]{"12GB LPDDR5X","UFS 4.0","4200MB/s","Virtual RAM"} }
                        }
                    },
                    display = new
                    {
                        name = "Dynamic AMOLED 2X",
                        sub = isUltra ? "6.8\" QHD+ · 120Hz · 2600 nits" : "6.1\"–6.7\" FHD+ · 120Hz · 1750 nits",
                        iconBg = "rgba(8,38,70,.85)",
                        iconColor = "rgba(80,200,220,.95)",
                        iconSymbol = "DSP",
                        sections = new dynamic[] {
                            new { label="Kích thước & độ phân giải", val=isUltra ? "6.8\" · QHD+ 3088×1440 · 505 PPI · AMOLED 2X" : "6.1\"–6.7\" · FHD+ · Dynamic AMOLED 2X",   bar=isUltra?98:90, barColor="#1d9e75" },
                            new { label="Độ sáng",          val=isUltra ? "Peak: 2,600 nits — dẫn đầu 2024\nHBM: 1,750 nits · Đọc tốt dưới nắng" : "Peak: 1,750 nits · Vision Booster",        bar=isUltra?99:88, barColor="#5dcaa5" },
                            new { label="Adaptive 1–120Hz", val="Tự điều chỉnh theo nội dung · Touch 240Hz",                                                                                      bar=90,            barColor="#378add" },
                            new { label="Bảo vệ",           val="Gorilla Glass Armor · chống chói 75% · IP68", tags=new[]{isUltra?"2600 nits":"1750 nits","QHD+","120Hz","HDR10+","IP68"} }
                        }
                    },
                    wifi = new
                    {
                        name = "5G · Wi-Fi 7 · UWB",
                        sub = "Snapdragon X75 modem · kết nối thế hệ mới",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(60,220,130,.95)",
                        iconSymbol = "5G",
                        sections = new dynamic[] {
                            new { label="5G",             val="Sub-6GHz & mmWave · Modem Snapdragon X75\nDownload lý thuyết: 10 Gbps",       bar=95, barColor="#1d9e75" },
                            new { label="Wi-Fi 7",        val="802.11be · 46 Gbps · Multi-Link Operation\nLatency thấp hơn 5× so với Wi-Fi 6", bar=98, barColor="#5dcaa5" },
                            new { label="Bluetooth & UWB",val="BT 5.3 · LE Audio · Auracast\nUWB định vị 10cm · Digital Car Key",            bar=88, barColor="#9fe1cb" },
                            new { label="NFC & DeX",      val="Samsung Pay · DeX không dây qua Wi-Fi 7", tags=new[]{"Wi-Fi 7","5G mmWave","UWB 10cm","BT 5.3","NFC"} }
                        }
                    }
                }
            };
        }

        // ─────────────────────────────────────────────────
        //  IPHONE
        // ─────────────────────────────────────────────────
        private object SpecsIPhone(string name)
        {
            bool isPro = name.Contains("Pro");
            bool is17 = name.Contains("17");

            return new
            {
                product = name,
                specs = new
                {
                    chip = new
                    {
                        name = is17 ? (isPro ? "Chip A19 Pro" : "Chip A19") : (isPro ? "Chip A18 Pro" : "Chip A18"),
                        sub = "3nm · Neural Engine · Apple Intelligence",
                        iconBg = "rgba(15,40,115,.85)",
                        iconColor = "rgba(120,180,255,.95)",
                        iconSymbol = isPro ? "A19" : "A18",
                        sections = new dynamic[] {
                            new { label="Tiến trình 3nm", val="Sản xuất trên tiến trình 3nm thế hệ 2\nHiệu năng tăng 20% · Tiêu thụ điện giảm 15%",          bar=99, barColor="#378add" },
                            new { label="CPU & GPU",      val=$"CPU{(isPro?" Pro":"")} — Hiệu năng single-core dẫn đầu\nGPU tăng 30% · Metal 3 · Ray Tracing",  bar=97, barColor="#85b7eb" },
                            new { label="Neural Engine",  val="Apple Intelligence on-device · không cần cloud\nSummarize · Rewrite · Image Playground",          bar=98, barColor="#378add" },
                            new { label="Benchmark",      val="Geekbench 6 Single: ~3,500+ · Multi: ~9,000+",  tags=new[]{"3nm","Neural Engine","Apple Intelligence","Face ID","A19"+(isPro?" Pro":"")} }
                        }
                    },
                    camera = new
                    {
                        name = isPro ? "Hệ thống 3 Camera 48MP" : "Camera 48MP + 12MP",
                        sub = isPro ? "Main + Ultra-wide + Periscope Tele" : "Main 48MP · Ultra-wide 12MP",
                        iconBg = "rgba(90,50,5,.85)",
                        iconColor = "rgba(255,195,70,.95)",
                        iconSymbol = "CAM",
                        sections = new dynamic[] {
                            new { label="Camera chính 48MP",  val="f/1.78 · OIS thế hệ mới · Sensor-shift\nMacro tự động · ProRAW · ProRes 4K@120fps",                 bar=97, barColor="#ef9f27" },
                            new { label="Camera Tele 48MP",   val=isPro ? "Zoom quang học 5x Periscope\nPortrait telephoto siêu sắc nét" : "12MP · Zoom quang học 2x", bar=isPro?94:78, barColor="#fac775" },
                            new { label="Ultra-wide 48MP",    val=isPro ? "f/2.2 · 120° · Macro 2cm\n3 ống kính đều 48MP — lần đầu tiên" : "12MP · f/2.2 · 120°",    bar=isPro?90:82, barColor="#ef9f27" },
                            new { label="Video & AI",         val="Cinematic mode 4K@120fps · ProRes Log\nSmart HDR 7 · Action mode · Spatial Video", tags=new[]{isPro?"3× 48MP":"48MP+12MP","ProRes Log","4K@120fps","Smart HDR","8x Zoom"} }
                        }
                    },
                    display = new
                    {
                        name = isPro ? "LTPO OLED ProMotion" : "OLED Super Retina XDR",
                        sub = isPro ? "6.3\" · 120Hz · Ceramic Shield · Always-On" : "6.1\" · 60Hz · Ceramic Shield",
                        iconBg = "rgba(8,38,70,.85)",
                        iconColor = "rgba(80,200,220,.95)",
                        iconSymbol = "DSP",
                        sections = new dynamic[] {
                            new { label="Panel",       val=isPro ? "6.3\" LTPO OLED · ProMotion 1–120Hz\nAlways-On Display · Dynamic Island" : "6.1\" OLED Super Retina XDR · 60Hz",  bar=isPro?97:88, barColor="#1d9e75" },
                            new { label="Độ sáng",     val=isPro ? "Peak 3,000 nits · HBM 2,000 nits\nĐọc tốt dưới nắng trực tiếp" : "Peak 2,000 nits",                                bar=isPro?98:86, barColor="#5dcaa5" },
                            new { label="Màu sắc",     val="P3 Wide Color · True Tone · HDR Dolby Vision\nDelta-E cực thấp — chuẩn hiệu chỉnh màu",                                     bar=97, barColor="#378add" },
                            new { label="Bảo vệ",      val="Ceramic Shield 2 · IP68 6m/30 phút", tags=new[]{isPro?"120Hz LTPO":"60Hz OLED","3000 nits","Ceramic Shield","IP68",isPro?"Dynamic Island":"Notch"} }
                        }
                    },
                    battery = new
                    {
                        name = isPro ? "Pin ~4252mAh" : "Pin ~3279mAh",
                        sub = isPro ? "Sạc 35W · MagSafe 25W" : "Sạc 20W · MagSafe 15W",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(80,220,130,.95)",
                        iconSymbol = "PWR",
                        sections = new dynamic[] {
                            new { label="Dung lượng",    val=isPro ? "~4252mAh · lớn nhất lịch sử iPhone Pro\nPin silicon-carbon thế hệ mới" : "~3279mAh · tối ưu iOS", bar=isPro?82:72, barColor="#1d9e75" },
                            new { label="Sạc có dây",    val=isPro ? "USB-C 35W · 0% → 50% trong 30 phút" : "Lightning/USB-C 20W",                                        bar=isPro?78:65, barColor="#5dcaa5" },
                            new { label="MagSafe",       val=isPro ? "MagSafe 25W · Reverse sạc Apple Watch" : "MagSafe 15W",                                              bar=isPro?72:60, barColor="#9fe1cb" },
                            new { label="Thời lượng",    val=isPro ? "Video playback: ~33 giờ · Dùng thực tế: 1.5–2 ngày" : "Video playback: ~26 giờ", tags=new[]{isPro?"4252mAh":"3279mAh",isPro?"35W Fast":"20W Fast",isPro?"MagSafe 25W":"MagSafe 15W",isPro?"33h Video":"26h Video"} }
                        }
                    },
                    ram = new
                    {
                        name = isPro ? "RAM 12GB" : "RAM 8GB",
                        sub = "Apple Intelligence · Multitasking tối ưu",
                        iconBg = "rgba(50,15,100,.85)",
                        iconColor = "rgba(190,130,255,.95)",
                        iconSymbol = "RAM",
                        sections = new dynamic[] {
                            new { label="RAM",           val=isPro ? "12GB — cao nhất lịch sử iPhone\nHỗ trợ mượt mà Apple Intelligence" : "8GB · Quản lý bộ nhớ iOS tối ưu",     bar=isPro?92:80, barColor="#7f77dd" },
                            new { label="Storage",       val="256GB / 512GB / 1TB · NVMe tốc độ cao\nKhông khe thẻ SD",                                                            bar=85, barColor="#afa9ec" },
                            new { label="Performance",   val="Mở ứng dụng nặng gần như tức thì\nXử lý ProRes 4K@120fps mượt mà",                                                  bar=90, barColor="#7f77dd" },
                            new { label="AI & On-device",val="Apple Intelligence · Siri nâng cao\nSummarize · Rewrite · Image AI", tags=new[]{isPro?"12GB":"8GB","Apple Intelligence","ProRes","NVMe","Multitask"} }
                        }
                    }
                }
            };
        }

        // ─────────────────────────────────────────────────
        //  HUAWEI / MATE
        // ─────────────────────────────────────────────────
        private object SpecsHuawei(string name)
        {
            bool isFold = name.Contains("X") || name.Contains("Fold");

            return new
            {
                product = name,
                specs = new
                {
                    chip = new
                    {
                        name = "Kirin 9030 Pro",
                        sub = "Huawei · HarmonyOS AI · Tự thiết kế",
                        iconBg = "rgba(80,20,5,.85)",
                        iconColor = "rgba(255,160,80,.95)",
                        iconSymbol = "K9K",
                        sections = new dynamic[] {
                            new { label="Kiến trúc",     val="Bộ vi xử lý tự thiết kế mạnh nhất của Huawei\nTối ưu cho HarmonyOS 5 và tác vụ AI",  bar=95, barColor="#ef9f27" },
                            new { label="AI Engine NPU", val="Xử lý AI on-device thế hệ mới\nDịch thuật · Nhận diện cảnh · Tóm tắt realtime",      bar=93, barColor="#fac775" },
                            new { label="GPU",           val="Hiệu suất đồ họa cải tiến\nHỗ trợ màn hình kép 120Hz đồng thời",                    bar=90, barColor="#ef9f27" },
                            new { label="HarmonyOS 5",   val="Hệ sinh thái Huawei · IoT đa thiết bị\nAppGallery · PC Mode · Multi-device AI", tags=new[]{"Kirin 9030","HarmonyOS 5","AI On-device","Multi-device","NPU"} }
                        }
                    },
                    camera = new
                    {
                        name = "Hệ thống Camera XMAGE",
                        sub = "50MP · Khẩu độ vật lý · Periscope Tele",
                        iconBg = "rgba(90,50,5,.85)",
                        iconColor = "rgba(255,195,70,.95)",
                        iconSymbol = "XMG",
                        sections = new dynamic[] {
                            new { label="Camera chính 50MP",      val="f/1.4–f/4.0 · Khẩu độ vật lý thay đổi\nOIS · PDAF · Cảm biến lớn nhất dòng Mate",        bar=95, barColor="#ef9f27" },
                            new { label="Ultra-wide 40MP",        val="Góc siêu rộng 120° · Macro mode\nXMAGE tuning — màu sắc tự nhiên chính xác",                bar=88, barColor="#fac775" },
                            new { label="Periscope Tele 50MP",    val="Zoom quang học 3.5x · OIS đa trục\nZoom kỹ thuật số 50x với AI chống rung",                  bar=90, barColor="#ef9f27" },
                            new { label="XMAGE Video",            val="4K@60fps · OIS 3 trục · Movie Aperture\nXMAGE Cinema mode", tags=new[]{"50MP Variable f/","XMAGE AI","Periscope 3.5x","Movie Aperture","8K"} }
                        }
                    },
                    display = new
                    {
                        name = isFold ? "Màn hình kép LTPO OLED" : "LTPO OLED",
                        sub = isFold ? "Trong 8\" 2K + Ngoài 6.49\" · Kunlun Glass" : "6.1\"–6.8\" · 1–120Hz · 2500 nits",
                        iconBg = "rgba(8,38,70,.85)",
                        iconColor = "rgba(80,200,220,.95)",
                        iconSymbol = isFold ? "2DSP" : "DSP",
                        sections = new dynamic[] {
                            new { label=isFold?"Màn hình trong 8\"":"Panel LTPO OLED",     val=isFold?"8\" LTPO OLED 2K · 1–120Hz thích ứng\nTỷ lệ vuông · ít nếp gấp nhất thị trường":"6.1\"–6.8\" · LTPO OLED · 1–120Hz adaptive",  bar=97, barColor="#1d9e75" },
                            new { label=isFold?"Màn hình ngoài 6.49\"":"Độ sáng",         val=isFold?"6.49\" OLED 120Hz · Kunlun Glass 2\nĐộ bền tốt nhất cho điện thoại gập":"Peak 2,500 nits · Sáng đọc tốt ngoài trời",               bar=94, barColor="#5dcaa5" },
                            new { label=isFold?"Bản lề siêu mỏng":"Màu sắc",             val=isFold?"4.5mm khi mở — mỏng nhất thị trường\nTitan · mở/đóng 500,000 lần":"DCI-P3 · HDR10+ · Delta-E < 1",                              bar=isFold?99:95, barColor="#378add" },
                            new { label="Bảo vệ",                                          val=isFold?"Kunlun Glass 2 · chống rơi 2m · IPX8":"Kunlun Glass · IP68", tags=new[]{isFold?"8\" OLED 2K":"LTPO OLED",isFold?"6.49\" Ngoài":"2500 nits","Kunlun Glass","120Hz",isFold?"4.5mm Fold":"IP68"} }
                        }
                    },
                    battery = new
                    {
                        name = isFold ? "Pin Silicon-Carbon 5600mAh" : "Pin 4500–5000mAh",
                        sub = "66W Fast · 50W Wireless · Graphene",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(80,220,130,.95)",
                        iconSymbol = "PWR",
                        sections = new dynamic[] {
                            new { label="Dung lượng",         val=isFold?"5600mAh · Silicon-carbon\nMật độ năng lượng cao trong thân siêu mỏng":"4500–5000mAh · Silicon-carbon · tối ưu theo model",                      bar=isFold?90:85, barColor="#1d9e75" },
                            new { label="Sạc có dây 66W",     val="0% → 50% trong ~25 phút · Đầy ~60 phút",                                                                                                                    bar=88, barColor="#5dcaa5" },
                            new { label="Sạc không dây 50W",  val="Nhanh gấp đôi chuẩn Qi · Reverse 10W",                                                                                                                      bar=80, barColor="#9fe1cb" },
                            new { label="Tản nhiệt Graphene", val="Công nghệ tản nhiệt Graphene độc quyền\nDuy trì nhiệt ổn định khi dùng liên tục", tags=new[]{isFold?"5600mAh":"4500mAh+","66W Fast","50W Wireless","Graphene","Satellite"} }
                        }
                    },
                    wifi = new
                    {
                        name = "5G · Wi-Fi 7 · Vệ tinh",
                        sub = "BeiDou · UWB · HarmonyOS IoT",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(60,220,130,.95)",
                        iconSymbol = "5G",
                        sections = new dynamic[] {
                            new { label="Satellite Messaging", val="Nhắn tin khi không có sóng 4G/5G\nSOS khẩn cấp qua vệ tinh",                           bar=90, barColor="#1d9e75" },
                            new { label="GPS Đa hệ thống",    val="GPS + BeiDou + GLONASS + Galileo\nĐịnh vị chính xác ở vùng sâu, núi rừng",              bar=95, barColor="#5dcaa5" },
                            new { label="Wi-Fi 7 & 5G",       val="802.11be Wi-Fi 7 · 5G Sub-6GHz & mmWave\nKết nối HarmonyOS đa thiết bị",                bar=96, barColor="#1d9e75" },
                            new { label="HarmonyOS IoT",      val="SmartHome · PC · Tablet · Wearables\nKết nối liền mạch toàn hệ sinh thái", tags=new[]{"Satellite SOS","BeiDou GPS","IP58","Wi-Fi 7","HarmonyOS IoT"} }
                        }
                    }
                }
            };
        }

        // ─────────────────────────────────────────────────
        //  IPAD
        // ─────────────────────────────────────────────────
        private object SpecsIPad(string name)
        {
            bool isPro = name.Contains("Pro");
            bool isAir = name.Contains("Air");
            bool isMini = name.Contains("mini") || name.Contains("Mini");

            string chipName = isPro ? "M4" : (isAir ? "M2" : "A16 Bionic");
            string displayInch = isPro ? (name.Contains("13") ? "13\"" : "11\"") : (isAir ? "11\"–13\"" : isMini ? "8.3\"" : "10.9\"");

            return new
            {
                product = name,
                specs = new
                {
                    chip = new
                    {
                        name = $"Chip Apple {chipName}",
                        sub = isPro ? "3nm · Neural Engine 38-core · Tốc độ máy tính" : $"Neural Engine · GPU Mạnh mẽ · {chipName}",
                        iconBg = "rgba(15,40,115,.85)",
                        iconColor = "rgba(120,180,255,.95)",
                        iconSymbol = chipName,
                        sections = new dynamic[] {
                            new { label="CPU & GPU",           val=isPro ? $"{chipName} 10-core CPU · 10-core GPU\nHiệu suất cao hơn M2 Pro 50% · Nhẹ điện" : $"{chipName} CPU · GPU mạnh · Tối ưu iPadOS",           bar=isPro?99:88, barColor="#378add" },
                            new { label="Neural Engine",       val=isPro ? "38-core Neural Engine · 38 TOPS\nApple Intelligence · On-device LLM" : "Neural Engine · AI Photography · Siri nâng cao",                     bar=isPro?98:85, barColor="#85b7eb" },
                            new { label="RAM Hợp nhất",        val=isPro ? "8GB–16GB Unified Memory · 120GB/s\nXử lý video ProRes 4K và 8K đồng thời" : "8GB Unified Memory · tốc độ băng thông cao",                  bar=isPro?97:84, barColor="#378add" },
                            new { label="Năng suất",           val=isPro ? "Logic Pro · Final Cut Pro · Xcode\nThay thế laptop hoàn toàn" : "App chuyên dụng · Split View · Stage Manager", tags=new[]{chipName,$"{(isPro?"38":"16")}-core NPU","Apple Intelligence","ProRes",isPro?"8–16GB RAM":"8GB RAM"} }
                        }
                    },
                    display = new
                    {
                        name = isPro ? "Ultra Retina XDR OLED" : "Liquid Retina",
                        sub = isPro ? $"{displayInch} · Tandem OLED · 1000–2000 nits" : $"{displayInch} · True Tone · ProMotion",
                        iconBg = "rgba(8,38,70,.85)",
                        iconColor = "rgba(80,200,220,.95)",
                        iconSymbol = "DSP",
                        sections = new dynamic[] {
                            new { label="Panel",        val=isPro ? $"{displayInch} Ultra Retina XDR · Tandem OLED\nĐộ phân giải 2732×2048 · 264 PPI" : $"{displayInch} Liquid Retina IPS · 264 PPI\nTrue Tone · P3 Wide Color",  bar=isPro?99:88, barColor="#1d9e75" },
                            new { label="Độ sáng",      val=isPro ? "Full-screen: 1,000 nits · HDR: 1,600 nits\nPeak: 2,000 nits · Toàn màn hình OLED" : "600–800 nits · True Tone · P3 Wide Color",                             bar=isPro?98:82, barColor="#5dcaa5" },
                            new { label="ProMotion",    val=isPro ? "Adaptive 10–120Hz ProMotion\nApple Pencil Pro latency < 9ms" : "60Hz · Cảm ứng Apple Pencil nhạy",                                                           bar=isPro?96:80, barColor="#378add" },
                            new { label="Bảo vệ",       val="Nano-texture glass (tùy chọn) · IP68\nApple Pencil Pro · Magic Keyboard", tags=new[]{isPro?"Tandem OLED":"Liquid Retina",isPro?"2000 nits":"800 nits",isPro?"120Hz":"60Hz","P3 Wide","Apple Pencil"} }
                        }
                    },
                    camera = new
                    {
                        name = isPro ? "Camera 12MP Pro" : "Camera 12MP",
                        sub = "ProRes Video · Center Stage · Ultra-wide",
                        iconBg = "rgba(90,50,5,.85)",
                        iconColor = "rgba(255,195,70,.95)",
                        iconSymbol = "CAM",
                        sections = new dynamic[] {
                            new { label="Camera sau",     val=isPro ? "12MP Wide f/1.8 + 10MP Ultra-wide f/2.4\nLiDAR Scanner · OIS · Macro mode" : "12MP Wide f/1.8 · Smart HDR · True Tone Flash", bar=isPro?88:78, barColor="#ef9f27" },
                            new { label="Camera trước",   val="12MP Ultra-wide · Center Stage AI\nTự động theo dõi chủ thể khi gọi video",                                                              bar=82, barColor="#fac775" },
                            new { label="Video",          val=isPro ? "4K@60fps ProRes · ProRes Log · Cinematic\nLiDAR AR · Depth mapping" : "4K@60fps · Slo-mo 240fps · Time-lapse",                  bar=isPro?90:80, barColor="#ef9f27" },
                            new { label="LiDAR & AR",     val=isPro ? "LiDAR Scanner thế hệ 2\nAR chuyên nghiệp · Object occlusion" : "AR Apps · RealityKit 2", tags=new[]{isPro?"LiDAR Scanner":"12MP",isPro?"ProRes Log":"4K@60fps","Center Stage","Macro",isPro?"Cinematic":"HDR Video"} }
                        }
                    },
                    battery = new
                    {
                        name = isPro ? "Pin 10758mAh" : "Pin 7606–10758mAh",
                        sub = "Sạc USB-C · 20W PD · ~10 giờ sử dụng",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(80,220,130,.95)",
                        iconSymbol = "PWR",
                        sections = new dynamic[] {
                            new { label="Dung lượng",   val=isPro ? $"10758mAh ({displayInch}) · Pin lớn nhất iPad Pro" : "7606–10758mAh · tùy theo kích thước", bar=isPro?90:82, barColor="#1d9e75" },
                            new { label="Sạc USB-C",    val="20W–27W PD · USB-C 3.2 / Thunderbolt 4\nSạc đầy ~3–4 giờ",                                          bar=72, barColor="#5dcaa5" },
                            new { label="Thời lượng",   val="~10 giờ duyệt web · Video: ~12 giờ\nChế độ màn hình thấp tối ưu pin",                               bar=88, barColor="#9fe1cb" },
                            new { label="Kết nối",      val="USB-C Thunderbolt 4 · 40Gbps · 8K output\nSạc ngược thiết bị khác", tags=new[]{isPro?"10758mAh":"7606mAh+","27W Fast","~10h sử dụng","Thunderbolt 4",isPro?"8K Output":"4K Output"} }
                        }
                    },
                    wifi = new
                    {
                        name = isPro ? "Wi-Fi 6E · 5G · Bluetooth 5.3" : "Wi-Fi 6 · 5G · Bluetooth 5.3",
                        sub = "Kết nối nhanh · GPS · Ultra Wideband",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(60,220,130,.95)",
                        iconSymbol = "WIFI",
                        sections = new dynamic[] {
                            new { label=isPro?"Wi-Fi 6E":"Wi-Fi 6", val=isPro?"802.11ax Wi-Fi 6E · 2.4/5/6GHz · 2400 Mbps\nMạng băng tần 6GHz độc quyền":"802.11ax Wi-Fi 6 · 2.4/5GHz · 1200 Mbps",   bar=isPro?95:85, barColor="#1d9e75" },
                            new { label="5G / LTE",                  val="5G Sub-6GHz · Nano-SIM + eSIM\nFallback LTE Cat. 25",                                                                            bar=88, barColor="#5dcaa5" },
                            new { label="Ultra Wideband",            val="UWB · Precision Finding · AirDrop nâng cao\nApple CarPlay Wireless",                                                             bar=85, barColor="#9fe1cb" },
                            new { label="GPS & Cellular",            val="GPS / GLONASS / BeiDou / Galileo\nBluetooth 5.3 · LE Audio", tags=new[]{isPro?"Wi-Fi 6E":"Wi-Fi 6","5G","UWB","BT 5.3","GPS"} }
                        }
                    }
                }
            };
        }

        // ─────────────────────────────────────────────────
        //  MACBOOK
        // ─────────────────────────────────────────────────
        private object SpecsMacBook(string name)
        {
            bool isPro = name.Contains("Pro");
            bool isAir = name.Contains("Air");
            bool is16inch = name.Contains("16");
            bool is14inch = name.Contains("14");

            string chipName = isPro ? (is16inch || is14inch ? "M4 Pro / M4 Max" : "M4 Pro") : "M4";
            string ramStr = isPro ? "24GB–128GB" : "16GB–32GB";
            string displayIn = isPro ? (is16inch ? "16.2\"" : "14.2\"") : (isAir ? "13.6\"–15.3\"" : "14.2\"");

            return new
            {
                product = name,
                specs = new
                {
                    chip = new
                    {
                        name = $"Apple Silicon {chipName}",
                        sub = "3nm · Neural Engine · GPU Mạnh mẽ",
                        iconBg = "rgba(15,40,115,.85)",
                        iconColor = "rgba(120,180,255,.95)",
                        iconSymbol = isPro ? "M4P" : "M4",
                        sections = new dynamic[] {
                            new { label="CPU",            val=isPro ? $"{chipName} — 14-core CPU (10P + 4E)\nHiệu năng tăng 75% so với Intel i9" : "M4 — 10-core CPU (4P + 6E)\nNhanh hơn M3 Air 28%",                          bar=isPro?99:94, barColor="#378add" },
                            new { label="GPU",            val=isPro ? "20-core GPU (Pro) / 40-core GPU (Max)\nRay Tracing · ProRes Codec · Metal 3" : "10-core GPU · Metal 3 · Ray Tracing hardware",                             bar=isPro?97:88, barColor="#85b7eb" },
                            new { label="Neural Engine",  val=$"38-core Neural Engine · 38 TOPS\nApple Intelligence · On-device LLM · {(isPro?"Studio":"Standard")} model",                                                       bar=98, barColor="#378add" },
                            new { label="Benchmark",      val=isPro ? "Geekbench 6 Single: ~4,000 · Multi: ~25,000\nCinebench R23: ~14,000 Multi" : "Geekbench 6 Single: ~3,800 · Multi: ~15,000", tags=new[]{chipName,"3nm","38-core NPU","Metal 3","Apple Intelligence",isPro?"20-core GPU":"10-core GPU"} }
                        }
                    },
                    display = new
                    {
                        name = isPro ? "Liquid Retina XDR" : "Liquid Retina",
                        sub = isPro ? $"{displayIn} · Mini-LED · 1000–1600 nits" : $"{displayIn} · True Tone · P3 Wide",
                        iconBg = "rgba(8,38,70,.85)",
                        iconColor = "rgba(80,200,220,.95)",
                        iconSymbol = "DSP",
                        sections = new dynamic[] {
                            new { label="Panel",       val=isPro ? $"{displayIn} Liquid Retina XDR · Mini-LED\n3456×2234 (16\") / 3024×1964 (14\") · Retina" : $"{displayIn} Liquid Retina IPS\n2560×1600 (13.6\") / 2880×1864 (15.3\")",  bar=isPro?99:90, barColor="#1d9e75" },
                            new { label="Độ sáng",     val=isPro ? "Sustained: 1,000 nits · HDR: 1,600 nits\nPeak: 1,000 nits full-screen / Local dimming" : "500 nits · True Tone · True Black",                                          bar=isPro?96:82, barColor="#5dcaa5" },
                            new { label="ProMotion",   val=isPro ? "Adaptive 24–120Hz ProMotion\nPhù hợp video 24fps, 48fps, 60fps, 120fps" : "60Hz · True Tone · Thân thiện mắt",                                                       bar=isPro?94:80, barColor="#378add" },
                            new { label="Màu sắc",     val="P3 Wide Color · True Tone · 1B màu\nĐộ chính xác màu Delta-E < 1 — chuẩn in ấn", tags=new[]{isPro?"Mini-LED XDR":"Liquid Retina",isPro?"120Hz":"60Hz",isPro?"1600 nits":"500 nits","P3 Wide","True Tone"} }
                        }
                    },
                    ram = new
                    {
                        name = $"Unified Memory {ramStr}",
                        sub = "Băng thông cực cao · Dùng chung CPU/GPU/NPU",
                        iconBg = "rgba(50,15,100,.85)",
                        iconColor = "rgba(190,130,255,.95)",
                        iconSymbol = "RAM",
                        sections = new dynamic[] {
                            new { label="Unified Memory", val=isPro ? "24GB / 48GB / 96GB / 128GB · 273–546 GB/s\nCPU, GPU, NPU cùng chia sẻ băng thông" : "16GB / 24GB / 32GB · 120 GB/s · Hiệu quả cao",             bar=isPro?99:90, barColor="#7f77dd" },
                            new { label="SSD tốc độ cao", val="256GB–8TB · 7.4 GB/s đọc · 5.0 GB/s ghi\nKhởi động từ sleep < 1 giây",                                                                                     bar=96, barColor="#afa9ec" },
                            new { label="Đa nhiệm nặng", val=isPro ? "Logic Pro + Final Cut + Xcode cùng lúc\n8K ProRes RAW edit realtime" : "Xcode · Lightroom · 4K ProRes · Đa nhiệm mượt", bar=isPro?97:88, barColor="#7f77dd" },
                            new { label="AI Workload",    val="Chạy LLM 7B–70B local · Stable Diffusion\nApple Intelligence không cần cloud", tags=new[]{ramStr,"273GB/s+","7.4GB/s SSD","Unified","Apple Intelligence"} }
                        }
                    },
                    battery = new
                    {
                        name = isPro ? "Pin 70–100Wh" : "Pin 52.6–66.5Wh",
                        sub = isPro ? "Sạc 96W–140W MagSafe · ~18 giờ" : "Sạc 30W–67W MagSafe · ~15–18 giờ",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(80,220,130,.95)",
                        iconSymbol = "PWR",
                        sections = new dynamic[] {
                            new { label="Dung lượng",    val=isPro ? "70.8Wh (14\") / 99.6Wh (16\") · Pin lớn nhất MacBook" : "52.6Wh (13\") / 66.5Wh (15\") · Compact nhẹ nhàng",         bar=isPro?92:80, barColor="#1d9e75" },
                            new { label="Sạc MagSafe",   val=isPro ? "140W MagSafe 3 · Đầy trong ~2 giờ\nFast Charge 50% trong 30 phút với 140W" : "67W MagSafe 3 · Nhanh và tiện lợi",    bar=isPro?88:78, barColor="#5dcaa5" },
                            new { label="Thời lượng",    val=isPro ? "Video playback: ~22h (14\") / ~24h (16\")\nChạy Xcode liên tục: ~16–18 giờ" : "Video: ~18h (15\") · Web: ~15–18h · Code: ~12h", bar=isPro?94:90, barColor="#9fe1cb" },
                            new { label="Hiệu quả điện", val="M4 tiêu thụ điện thấp hơn Intel thế hệ 13 tới 40%\nFanless (Air) · Im lặng hoàn toàn", tags=new[]{isPro?"100Wh":"66Wh",isPro?"140W MagSafe":"67W MagSafe",isPro?"24h Video":"18h Video","M4 Efficient",isAir?"Fanless":"Active Cooling"} }
                        }
                    },
                    wifi = new
                    {
                        name = "Wi-Fi 6E · Bluetooth 5.3",
                        sub = isPro ? "Thunderbolt 4 · HDMI · SD Card · MagSafe" : "Thunderbolt 3/4 · USB-C · MagSafe",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(60,220,130,.95)",
                        iconSymbol = "WIFI",
                        sections = new dynamic[] {
                            new { label="Wi-Fi 6E",           val="802.11ax · 2.4/5/6GHz tri-band\n2,400 Mbps · Độ trễ siêu thấp",                                                                  bar=95, barColor="#1d9e75" },
                            new { label="Kết nối",            val=isPro ? "3× Thunderbolt 4 · HDMI 2.1 · SD UHS-II\nMagSafe 3 · 3.5mm jack · Hỗ trợ 2 màn hình ngoài" : "2× Thunderbolt 3 · MagSafe 3 · 3.5mm jack",        bar=isPro?92:82, barColor="#5dcaa5" },
                            new { label="Bluetooth 5.3",      val="LE Audio · Auracast · AirDrop\nHandoff · Continuity Camera · Universal Control",                                                   bar=88, barColor="#9fe1cb" },
                            new { label="Bảo mật & Display",  val="Face ID qua camera (tùy model) / Touch ID\nMirror 6K · Sidecar với iPad", tags=new[]{"Wi-Fi 6E","Thunderbolt 4","BT 5.3","MagSafe 3","AirDrop","Continuity"} }
                        }
                    }
                }
            };
        }

        // ─────────────────────────────────────────────────
        //  LAPTOP (Windows)
        // ─────────────────────────────────────────────────
        private object SpecsLaptop(string name)
        {
            bool isGaming = name.ToLower().Contains("gaming") || name.ToLower().Contains("rog") || name.ToLower().Contains("legion") || name.ToLower().Contains("predator");
            bool isUltra = name.ToLower().Contains("ultra") || name.ToLower().Contains("slim") || name.ToLower().Contains("zenbook");

            return new
            {
                product = name,
                specs = new
                {
                    chip = new
                    {
                        name = isGaming ? "Intel Core i7/i9 / AMD Ryzen" : "Intel Core Ultra / AMD Ryzen AI",
                        sub = isGaming ? "Desktop-grade GPU · High TDP · Turbo Boost" : "Tiết kiệm điện · AI Processing · Thin & Light",
                        iconBg = "rgba(15,40,115,.85)",
                        iconColor = "rgba(120,180,255,.95)",
                        iconSymbol = "CPU",
                        sections = new dynamic[] {
                            new { label="CPU",              val=isGaming ? "Intel Core i7-14700HX / AMD Ryzen 9 8945HX\n8P + 16E cores · TDP 55W–75W · Turbo 5.5GHz" : "Intel Core Ultra 7 165H / AMD Ryzen AI 9 HX\nLP E-cores · AI Accelerator on-chip · TDP 28W",  bar=isGaming?97:90, barColor="#378add" },
                            new { label="GPU",              val=isGaming ? "NVIDIA RTX 4080 / AMD RX 7900M\nDLSS 3 · Ray Tracing · 12–16GB GDDR6" : "Intel Arc / AMD Radeon 890M integrated\nAI-enhanced · Efficient gaming",                                       bar=isGaming?98:72, barColor="#85b7eb" },
                            new { label="AI Engine / NPU",  val=isGaming ? "AI FPS Booster · DLSS 3 Frame Gen\nNVIDIA AI Studio" : "Intel NPU 11.5 TOPS / AMD XDNA 50 TOPS\nCopilot+ PC · Windows AI · On-device LLM",                                            bar=isGaming?88:94, barColor="#378add" },
                            new { label="Hiệu năng",        val=isGaming ? "Cinebench R23 Multi: ~25,000–35,000\nFPS: 120+ @ 1080p High trên game AAA" : "Geekbench 6 Multi: ~12,000–18,000\nPin toàn ngày > 15 giờ", tags=new[]{isGaming?"RTX 4080":"Core Ultra",isGaming?"DLSS 3":"NPU AI",isGaming?"DDR5 5600":"LPDDR5X","PCIe 5.0","Thunderbolt"} }
                        }
                    },
                    display = new
                    {
                        name = isGaming ? "IPS / OLED 144–240Hz Gaming" : "OLED / IPS 2.8K+ Thin & Light",
                        sub = isGaming ? "1080p–2K · Low latency · G-Sync" : "2.8K–4K OLED · True Tone · 90Hz+",
                        iconBg = "rgba(8,38,70,.85)",
                        iconColor = "rgba(80,200,220,.95)",
                        iconSymbol = "DSP",
                        sections = new dynamic[] {
                            new { label="Panel",            val=isGaming ? "15.6\"–17.3\" IPS/VA/OLED · FHD–QHD\n144Hz / 165Hz / 240Hz · 3ms response" : "13.3\"–15.6\" OLED 2.8K–4K · 60–120Hz\nTrue-to-life color · VESA DisplayHDR True Black",  bar=isGaming?90:94, barColor="#1d9e75" },
                            new { label="Độ sáng & màu",   val=isGaming ? "300–400 nits · sRGB 100% · DCI-P3 72%\nG-Sync / FreeSync Premium Pro" : "400–550 nits OLED · DCI-P3 100% · Delta-E < 1\nPantone Validated",                                    bar=isGaming?78:95, barColor="#5dcaa5" },
                            new { label="Refresh Rate",    val=isGaming ? "144Hz–240Hz · 1ms–3ms · 0.1% Motion Blur\nG-Sync Compatible" : "60Hz–120Hz · OLED burn-in prevention\nLow blue light mode",                                                    bar=isGaming?96:86, barColor="#378add" },
                            new { label="Output",          val="HDMI 2.1 · DisplayPort 2.0 · USB-C Alt-mode\nHỗ trợ màn hình ngoài 4K/8K", tags=new[]{isGaming?"240Hz":"120Hz OLED",isGaming?"G-Sync":"OLED","DCI-P3","HDMI 2.1","HDR"} }
                        }
                    },
                    ram = new
                    {
                        name = isGaming ? "RAM DDR5 16–32GB" : "RAM LPDDR5X 16–64GB",
                        sub = isGaming ? "PCIe 5.0 NVMe · Nâng cấp dễ" : "Gắn trực tiếp mainboard · Tốc độ cao",
                        iconBg = "rgba(50,15,100,.85)",
                        iconColor = "rgba(190,130,255,.95)",
                        iconSymbol = "RAM",
                        sections = new dynamic[] {
                            new { label="RAM",           val=isGaming ? "16GB–64GB DDR5 5600MHz · 2 khe SO-DIMM\nNâng cấp lên tối đa 64GB" : "16GB–64GB LPDDR5X 7500MHz · Hàn trên board\nBăng thông cao hơn DDR5 thông thường 30%", bar=isGaming?88:92, barColor="#7f77dd" },
                            new { label="SSD NVMe",      val=isGaming ? "512GB–4TB PCIe 5.0 NVMe · 12,000 MB/s đọc\n2 khe M.2 (tùy model)" : "512GB–2TB PCIe 4.0/5.0 · 7,000–12,000 MB/s\nNhỏ gọn · Tích hợp",                                             bar=isGaming?94:90, barColor="#afa9ec" },
                            new { label="GPU RAM",       val=isGaming ? "NVIDIA RTX: 8–16GB GDDR6X\nDùng riêng — không chia sẻ với RAM hệ thống" : "GPU tích hợp chia sẻ RAM hệ thống\nTối ưu AI workload nhẹ",                                           bar=isGaming?95:70, barColor="#7f77dd" },
                            new { label="Hiệu năng",     val="Mở Chrome 50 tab + VS Code + video mượt\nBiên dịch code nhanh hơn SSD HDD 3x", tags=new[]{isGaming?"DDR5 5600":"LPDDR5X","PCIe 5.0","NVMe",isGaming?"2 SO-DIMM":"Soldered","Upgradeable"} }
                        }
                    },
                    battery = new
                    {
                        name = isGaming ? "Pin 90–99Wh" : "Pin 75–90Wh",
                        sub = isGaming ? "Sạc 240W–330W · USB-C 100W" : "Sạc 65W–100W USB-C · ~12–15 giờ",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(80,220,130,.95)",
                        iconSymbol = "PWR",
                        sections = new dynamic[] {
                            new { label="Dung lượng",    val=isGaming ? "90Wh–99Wh · Intel Speed Shift tự động tiết kiệm\nChế độ Gaming / Balanced / Silent" : "75Wh–90Wh · Ultra-low power design\nARM-like efficiency trên x86",                           bar=isGaming?85:88, barColor="#1d9e75" },
                            new { label="Sạc",           val=isGaming ? "240W–330W barrel jack · Sạc đầy ~1.5 giờ\nUSB-C 100W khi dùng nhẹ" : "65W–100W GaN USB-C · Sạc đầy ~1.5–2 giờ\nSạc nhanh 50% trong 30 phút",                                    bar=isGaming?90:85, barColor="#5dcaa5" },
                            new { label="Thời lượng",    val=isGaming ? "Gaming RTX On: 2–3 giờ · Hybrid mode: 6–8 giờ\nNghỉ ngơi / văn phòng: 10–14 giờ" : "Web & văn phòng: 12–15 giờ · Video: 14–16 giờ\nChạy LLM local: 8–10 giờ",                    bar=isGaming?60:88, barColor="#9fe1cb" },
                            new { label="Battery AI",    val=isGaming ? "NVIDIA Whisper mode · Optimus G-Sync\nFPS Limiter khi dùng pin" : "AI Battery Optimizer · Windows Battery Saver\nCopilot+ power management", tags=new[]{isGaming?"99Wh":"90Wh",isGaming?"330W Charger":"100W USB-C",isGaming?"Optimus":"AI Optimizer",isGaming?"3h Gaming":"15h Office"} }
                        }
                    },
                    wifi = new
                    {
                        name = "Wi-Fi 7 · Bluetooth 5.4",
                        sub = isGaming ? "2.5G Ethernet · Thunderbolt 4 · USB4" : "Thunderbolt 4 · USB4 · HDMI 2.1",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(60,220,130,.95)",
                        iconSymbol = "WIFI",
                        sections = new dynamic[] {
                            new { label="Wi-Fi 7",          val="802.11be · Tri-band 2.4/5/6GHz · 5.8 Gbps\nMLO · Độ trễ cực thấp gaming & video call",                                                bar=98, barColor="#1d9e75" },
                            new { label="Kết nối",          val=isGaming ? "2.5GbE Ethernet · 3×USB-A · 2×USB-C\nHDMI 2.1 · DisplayPort · SD card · Thunderbolt 4" : "2× Thunderbolt 4 · USB-A · HDMI 2.1\nSD Card · 3.5mm · USB4 40Gbps",               bar=isGaming?94:88, barColor="#5dcaa5" },
                            new { label="Bluetooth 5.4",    val="LE Audio · Auracast · BLE 5.4\nKết nối tai nghe, chuột, bàn phím không dây",                                                           bar=86, barColor="#9fe1cb" },
                            new { label="Bảo mật",          val="TPM 2.0 · Windows Hello Face/Fingerprint\nIR Camera · Webcam 1080p/2K", tags=new[]{"Wi-Fi 7",isGaming?"2.5G Ethernet":"Thunderbolt 4","BT 5.4","TPM 2.0","Windows Hello"} }
                        }
                    }
                }
            };
        }

        // ─────────────────────────────────────────────────
        //  DEFAULT
        // ─────────────────────────────────────────────────
        private object SpecsDefault(string name)
        {
            return new
            {
                product = name,
                specs = new
                {
                    camera = new
                    {
                        name = "Hệ thống Camera",
                        sub = "Multi-lens · AI Photography",
                        iconBg = "rgba(90,50,5,.85)",
                        iconColor = "rgba(255,195,70,.95)",
                        iconSymbol = "CAM",
                        sections = new dynamic[] {
                            new { label="Camera chính",  val="Camera chính độ phân giải cao · f/1.7–f/2.0 · OIS · PDAF",  bar=85, barColor="#ef9f27" },
                            new { label="Ultra-wide",    val="Góc siêu rộng 120° · Macro mode · Night vision",             bar=82, barColor="#fac775" },
                            new { label="AI & Video",    val="4K@60fps · HDR10+ · AI Night mode · Portrait",  tags=new[]{"AI Camera","4K Video","Night Mode","OIS","HDR10+"} }
                        }
                    },
                    chip = new
                    {
                        name = "Processor",
                        sub = "Multi-core · AI Engine · Performance",
                        iconBg = "rgba(15,40,115,.85)",
                        iconColor = "rgba(120,180,255,.95)",
                        iconSymbol = "CPU",
                        sections = new dynamic[] {
                            new { label="CPU", val="Đa nhân hiệu suất cao · Turbo Boost", bar=88, barColor="#378add" },
                            new { label="GPU", val="GPU mạnh mẽ · Gaming & Đồ họa",       bar=85, barColor="#85b7eb" },
                            new { label="AI",  val="Xử lý AI on-device · Smart features", tags=new[]{"Multi-core","AI Engine","Gaming","Efficient"} }
                        }
                    },
                    battery = new
                    {
                        name = "Pin dung lượng lớn",
                        sub = "Sạc nhanh · Sử dụng cả ngày",
                        iconBg = "rgba(8,55,18,.85)",
                        iconColor = "rgba(80,220,130,.95)",
                        iconSymbol = "PWR",
                        sections = new dynamic[] {
                            new { label="Dung lượng",  val="Pin lớn · Dùng cả ngày thoải mái", bar=82, barColor="#1d9e75" },
                            new { label="Sạc nhanh",   val="Công nghệ sạc nhanh · 0→50% nhanh", bar=80, barColor="#5dcaa5" },
                            new { label="Hiệu quả",    val="Tối ưu phần mềm · AI Battery saver", tags=new[]{"Fast Charge","All-day","AI Saver","Wireless"} }
                        }
                    },
                    display = new
                    {
                        name = "Màn hình chất lượng cao",
                        sub = "AMOLED / OLED · High refresh rate",
                        iconBg = "rgba(8,38,70,.85)",
                        iconColor = "rgba(80,200,220,.95)",
                        iconSymbol = "DSP",
                        sections = new dynamic[] {
                            new { label="Panel",       val="AMOLED / OLED · Màu sắc sống động · Đen sâu", bar=88, barColor="#1d9e75" },
                            new { label="Refresh rate",val="60Hz–120Hz Adaptive · Touch nhạy",             bar=85, barColor="#5dcaa5" },
                            new { label="Bảo vệ",      val="Gorilla Glass · IP68 chống nước · HDR",  tags=new[]{"AMOLED","120Hz","HDR10+","IP68","Wide Color"} }
                        }
                    }
                }
            };
        }

        // ─────────────────────────────────────────────────
        //  UPLOAD IMAGE
        // ─────────────────────────────────────────────────
        [HttpPost]
        public JsonResult UploadImage(HttpPostedFileBase file)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, message = "Không có file được chọn" });

                if (file.ContentLength > 5 * 1024 * 1024)
                    return Json(new { success = false, message = "Kích thước ảnh không được vượt quá 5MB" });

                string[] allowed = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string ext = Path.GetExtension(file.FileName).ToLower();
                if (!allowed.Contains(ext))
                    return Json(new { success = false, message = "Định dạng ảnh không hợp lệ (JPG, PNG, GIF, WebP)" });

                string fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + Guid.NewGuid().ToString().Substring(0, 8) + ext;
                string folder = HttpContext.Server.MapPath("~/Content/Images/Products");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                file.SaveAs(Path.Combine(folder, fileName));

                return Json(new { success = true, message = "Upload thành công", imageUrl = "/Content/Images/Products/" + fileName, fileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
