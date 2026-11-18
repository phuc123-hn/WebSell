# ?? CODE EXAMPLES & IMPLEMENTATION DETAILS

---

## ?? LUCKY SPIN - Code Examples

### 1. Controller Spin Logic
```csharp
[HttpPost]
public ActionResult Spin()
{
    try
    {
        // 1?? Xác th?c ng??i dùng
        int userId = (int)(Session["UserId"] ?? 0);
        if (userId == 0)
            return Json(new { success = false, message = "Vui lòng ??ng nh?p" });

        // 2?? Ki?m tra SpinCount
        var user = db.Users.Find(userId);
        if (user == null || user.SpinCount <= 0)
            return Json(new { success = false, message = "B?n không có l??t quay nào" });

        // 3?? Tr? 1 spin
        user.SpinCount -= 1;
        db.SaveChanges();

        // 4?? G?i service th?c hi?n spin
        var spinResult = luckySpinService.SpinLucky(userId, 0);
        if (spinResult == null)
        {
            // 5?? Hoàn l?i spin n?u l?i
            user.SpinCount += 1;
            db.SaveChanges();
            return Json(new { success = false, message = "L?i khi quay" });
        }

        // 6?? Tr? v? k?t qu?
        return Json(new
        {
            success = true,
            spinHistoryId = spinResult.Id,
            prizeId = spinResult.PrizeId,
            prizeName = spinResult.Prize?.Name ?? "Không xác ??nh",
            prizeType = spinResult.Prize?.PrizeType ?? "",
            wonVoucherCode = spinResult.WonVoucher?.Code,
            wonProductName = spinResult.WonProduct?.Name,
            spinCountLeft = user.SpinCount,
            message = $"B?n ?ã trúng: {spinResult.Prize?.Name}"
        });
    }
    catch (DbEntityValidationException ex)
    {
        var errors = string.Join(" | ", ex.EntityValidationErrors
            .SelectMany(e => e.ValidationErrors)
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
        LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Validation Error: {errors}");
        return Json(new { success = false, message = "Validation: " + errors });
    }
    catch (Exception ex)
    {
        LogError($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] General Error: {ex.Message}\r\n{ex.StackTrace}");
        return Json(new { success = false, message = "L?i: " + ex.Message });
    }
}

private void LogError(string message)
{
    try
    {
        string logPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\controller_error.log";
        Directory.CreateDirectory(Path.GetDirectoryName(logPath));
        File.AppendAllText(logPath, message + "\r\n---\r\n");
    }
    catch { }
}
```

### 2. Service Spin Logic
```csharp
public LuckySpinHistory SpinLucky(int userId, int orderId)
{
    try
    {
        // 1?? L?y t?t c? prize ho?t ??ng
        var prizes = _db.LuckySpinPrizes
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToList();

        if (prizes.Count == 0)
        {
            Debug.WriteLine("SpinLucky Error: No active prizes found");
            return null;
        }

        // 2?? Quay bánh xe
        var selectedPrize = SpinWheelByWinRate(prizes);
        if (selectedPrize == null)
            return null;

        // 3?? T?o LuckySpinHistory
        var spinHistory = new LuckySpinHistory
        {
            UserId = userId,
            OrderId = orderId == 0 ? (int?)null : orderId,
            PrizeId = selectedPrize.Id,
            Status = "Pending",
            SpinDate = DateTime.Now
        };

        // 4?? X? lý Voucher
        if (selectedPrize.PrizeType == "Voucher" && selectedPrize.VoucherId.HasValue)
        {
            var voucher = _db.Vouchers.Find(selectedPrize.VoucherId.Value);
            if (voucher != null)
            {
                spinHistory.WonVoucherId = voucher.Id;
                spinHistory.WonVoucher = voucher;
                
                // T? ??ng c?p voucher
                spinHistory.Status = "Claimed";
                spinHistory.ClaimedDate = DateTime.Now;
            }
        }

        // 5?? X? lý Product
        if (selectedPrize.PrizeType == "Product" && selectedPrize.ProductId.HasValue)
        {
            spinHistory.WonProductId = selectedPrize.ProductId.Value;
        }

        // 6?? L?u vào DB
        _db.LuckySpinHistories.Add(spinHistory);
        _db.SaveChanges();

        // 7?? T?i navigation properties
        spinHistory.Prize = _db.LuckySpinPrizes.Find(spinHistory.PrizeId);
        if (spinHistory.WonVoucherId.HasValue)
            spinHistory.WonVoucher = _db.Vouchers.Find(spinHistory.WonVoucherId.Value);
        if (spinHistory.WonProductId.HasValue)
            spinHistory.WonProduct = _db.Products.Find(spinHistory.WonProductId.Value);

        return spinHistory;
    }
    catch (Exception ex)
    {
        var errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] SpinLucky Exception: {ex.Message}\r\n{ex.StackTrace}";
        Debug.WriteLine(errorMsg);
        
        try
        {
            string logPath = AppDomain.CurrentDomain.BaseDirectory + "logs\\spinerror.log";
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            File.AppendAllText(logPath, errorMsg + "\r\n---\r\n");
        }
        catch { }
        
        return null;
    }
}

/// <summary>
/// Ch?n prize d?a trên WinRate
/// </summary>
private LuckySpinPrize SpinWheelByWinRate(List<LuckySpinPrize> prizes)
{
    // Tính t?ng rate
    decimal totalRate = prizes.Sum(p => p.WinRate);
    if (totalRate <= 0)
        return null;

    // Random t? 0 ??n totalRate
    double randomValue = _random.NextDouble() * (double)totalRate;

    // Ch?n prize
    decimal cumulative = 0;
    foreach (var prize in prizes)
    {
        cumulative += prize.WinRate;
        if (randomValue <= (double)cumulative)
        {
            return prize;
        }
    }

    // Fallback: tr? prize cu?i
    return prizes.LastOrDefault();
}
```

### 3. Frontend JavaScript
```javascript
$(document).ready(function() {
    // Spin button click
    $('#btnSpin').click(function() {
        $(this).prop('disabled', true);
        
        $.ajax({
            url: '/LuckySpin/Spin',
            type: 'POST',
            dataType: 'json',
            success: function(response) {
                if (response.success) {
                    // Update spin count
                    $('#spinCountDisplay').text(response.spinCountLeft);
                    
                    // Animate wheel
                    animateWheel(response.prizeId);
                    
                    // Show result
                    showPrizeResult(response);
                    
                    // Load history
                    loadSpinHistory();
                } else {
                    alert('Error: ' + response.message);
                }
            },
            error: function() {
                alert('Network error!');
            },
            complete: function() {
                $('#btnSpin').prop('disabled', false);
            }
        });
    });
    
    function animateWheel(prizeId) {
        // Barrel roll animation
        var wheel = $('#wheel');
        var degrees = Math.random() * 360 + 720; // 2+ rotations
        
        wheel.stop().animate(
            { rotation: degrees },
            {
                duration: 3000,
                easing: 'easeOutQuad',
                step: function(now) {
                    wheel.css({
                        'transform': 'rotate(' + now + 'deg)',
                        '-webkit-transform': 'rotate(' + now + 'deg)',
                        '-moz-transform': 'rotate(' + now + 'deg)'
                    });
                },
                complete: function() {
                    console.log('Spin complete!');
                }
            }
        );
    }
    
    function showPrizeResult(data) {
        var modal = `
            <div class="modal fade" id="prizeModal">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <h4>?? Chúc M?ng!</h4>
                        <p>B?n ?ã trúng: <strong>${data.prizeName}</strong></p>
                        <p>Lo?i: ${data.prizeType}</p>
                        ${data.prizeType === 'Voucher' ? 
                            `<p>Mã: <code>${data.wonVoucherCode}</code></p>` :
                            `<p>${data.wonProductName}</p>`}
                    </div>
                </div>
            </div>
        `;
        
        $(modal).modal('show');
    }
});
```

---

## ?? LANDING PAGE - Code Examples

### 1. Helper Class
```csharp
public static class LandingHelper
{
    private const string LANDING_COOKIE_NAME = "LandingViewed";
    private const int COOKIE_EXPIRY_DAYS = 30;

    public static bool HasViewedLanding()
    {
        try
        {
            HttpCookie cookie = HttpContext.Current?.Request.Cookies[LANDING_COOKIE_NAME];
            return cookie != null && cookie.Value == "true";
        }
        catch
        {
            return false;
        }
    }

    public static void MarkLandingAsViewed(HttpResponseBase response)
    {
        try
        {
            HttpCookie landingCookie = new HttpCookie(LANDING_COOKIE_NAME)
            {
                Value = "true",
                Expires = DateTime.Now.AddDays(COOKIE_EXPIRY_DAYS),
                Path = "/",
                // B? sung b?o m?t
                Secure = true,      // HTTPS only
                HttpOnly = false    // JavaScript có th? truy c?p
            };
            response.Cookies.Add(landingCookie);
        }
        catch
        {
            // Silent fail
        }
    }

    public static void ClearLandingCookie(HttpResponseBase response)
    {
        try
        {
            HttpCookie cookie = new HttpCookie(LANDING_COOKIE_NAME)
            {
                Expires = DateTime.Now.AddDays(-1),
                Path = "/"
            };
            response.Cookies.Add(cookie);
        }
        catch { }
    }
}
```

### 2. Controller Usage
```csharp
public ActionResult ShowcaseLanding()
{
    // Ki?m tra ?ã xem landing ch?a
    if (LandingHelper.HasViewedLanding())
    {
        // Option 1: Redirect ngay t?i Index
        return RedirectToAction("Index");
    }
    
    // Hi?n th? landing
    return View("~/Views/Shared/Landing.cshtml");
}

// X? lý khi user b?m CTA button
[HttpPost]
public ActionResult ConfirmLandingViewed()
{
    LandingHelper.MarkLandingAsViewed(Response);
    return RedirectToAction("Index");
}
```

### 3. Landing View Layout
```html
<!-- Landing.cshtml -->
@{
    ViewBag.Title = "Welcome to Showcase";
}

<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            margin: 0;
            padding: 0;
            overflow: hidden;
            font-family: 'Segoe UI', sans-serif;
        }
        
        .landing-container {
            width: 100vw;
            height: 100vh;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            color: white;
            position: relative;
        }
        
        .landing-content {
            text-align: center;
            animation: slideInUp 0.8s ease-out;
        }
        
        h1 {
            font-size: 3.5rem;
            margin: 0 0 20px;
            font-weight: 700;
        }
        
        p {
            font-size: 1.3rem;
            margin: 0 0 40px;
            opacity: 0.9;
        }
        
        .btn-primary {
            padding: 15px 40px;
            font-size: 1.1rem;
            background: white;
            color: #667eea;
            border: none;
            border-radius: 50px;
            cursor: pointer;
            transition: transform 0.3s;
        }
        
        .btn-primary:hover {
            transform: scale(1.05);
        }
        
        @keyframes slideInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
    </style>
</head>
<body>
    <div class="landing-container">
        <div class="landing-content">
            <h1>?? Welcome to Showcase</h1>
            <p>Khám phá nh?ng s?n ph?m flagship tuy?t v?i nh?t</p>
            <button class="btn-primary" onclick="enterShowcase()">
                Vào Tr?ng Bày
            </button>
        </div>
    </div>

    <script>
        function enterShowcase() {
            // Mark landing as viewed
            $.post('@Url.Action("ConfirmLandingViewed")', function() {
                // Redirect to showcase
                window.location.href = '@Url.Action("Index")';
            });
        }
        
        // Auto close after 10 seconds
        setTimeout(function() {
            enterShowcase();
        }, 10000);
    </script>
</body>
</html>
```

---

## ?? NEWS - Code Examples

### 1. News Controller
```csharp
public class NewsController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    // GET: /News
    public ActionResult Index(string category = null)
    {
        var news = db.News.Where(n => n.IsActive);

        if (!string.IsNullOrEmpty(category))
        {
            news = news.Where(n => n.Category == category);
        }

        news = news.OrderByDescending(n => n.PublishDate);

        return View(news.ToList());
    }

    // GET: /News/Details/{id}
    public ActionResult Details(int? id)
    {
        if (id == null)
            return HttpNotFound();

        var newsItem = db.News.Find(id);
        if (newsItem == null || !newsItem.IsActive)
            return HttpNotFound();

        // Load related news (5 items from same category)
        var relatedNews = db.News
            .Where(n => n.Id != id 
                    && n.IsActive 
                    && n.Category == newsItem.Category)
            .OrderByDescending(n => n.PublishDate)
            .Take(5)
            .ToList();

        ViewBag.RelatedNews = relatedNews;
        return View(newsItem);
    }

    // GET: /News/GetLatestNews (AJAX for widget)
    public ActionResult GetLatestNews(int count = 5)
    {
        var latestNews = db.News
            .Where(n => n.IsActive)
            .OrderByDescending(n => n.PublishDate)
            .Take(count)
            .ToList();

        return PartialView("_NewsTicker", latestNews);
    }

    // GET: /News/GetNewsByCategory (AJAX for widget)
    public ActionResult GetNewsByCategory(string category, int count = 3)
    {
        var news = db.News
            .Where(n => n.IsActive && n.Category == category)
            .OrderByDescending(n => n.PublishDate)
            .Take(count)
            .ToList();

        return PartialView("_NewsList", news);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            db.Dispose();
        base.Dispose(disposing);
    }
}
```

### 2. News Partial View (_NewsTicker)
```html
@model List<web1.Models.News>

<div class="news-ticker">
    <div class="ticker-header">
        <h4>?? Latest News</h4>
    </div>
    <div class="ticker-content">
        <ul>
            @foreach (var item in Model)
            {
                <li>
                    <a href="@Url.Action("Details", "News", new { id = item.Id })">
                        <strong>@item.Title</strong>
                        <span class="date">@item.PublishDate.ToString("dd/MM/yyyy")</span>
                    </a>
                </li>
            }
        </ul>
    </div>
</div>

<style>
    .news-ticker {
        background: #f5f5f5;
        border-radius: 8px;
        padding: 16px;
        max-height: 300px;
        overflow-y: auto;
    }
    
    .ticker-content ul {
        list-style: none;
        padding: 0;
        margin: 0;
    }
    
    .ticker-content li {
        padding: 8px 0;
        border-bottom: 1px solid #e0e0e0;
    }
    
    .ticker-content a {
        text-decoration: none;
        color: #333;
    }
    
    .date {
        color: #999;
        font-size: 0.85rem;
        margin-left: 10px;
    }
</style>
```

---

## ??? SHOWCASE - Code Examples

### 1. Showcase Product Query
```csharp
public ActionResult Index()
{
    // Query flagship products
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

    // Fallback ImageUrl
    foreach (var product in products)
    {
        if (string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            string n = product.Name.ToLower();
            if (n.Contains("iphone 15 pro")) 
                product.ImageUrl = "/Content/Images/iphone15pro.jpg";
            else if (n.Contains("iphone")) 
                product.ImageUrl = "/Content/Images/iphone.jpg";
            else if (n.Contains("ipad")) 
                product.ImageUrl = "/Content/Images/ipad.jpg";
            else if (n.Contains("macbook")) 
                product.ImageUrl = "/Content/Images/macbook.jpg";
            else if (n.Contains("laptop")) 
                product.ImageUrl = "/Content/Images/laptop.jpg";
            else if (n.Contains("samsung") || n.Contains("galaxy")) 
                product.ImageUrl = "/Content/Images/samsung.jpg";
            else if (n.Contains("huawei") || n.Contains("mate")) 
                product.ImageUrl = "/Content/Images/huawei.jpg";
        }
    }

    ViewBag.Title = "Tr?ng Bày S?n Ph?m";
    return View(products);
}
```

### 2. Get Specs Dynamic Router
```csharp
private object BuildSpecs(string name)
{
    string n = name.ToLower();

    if (n.Contains("macbook") || n.Contains("mac book")) 
        return SpecsMacBook(name);
    if (n.Contains("ipad")) 
        return SpecsIPad(name);
    if (n.Contains("laptop")) 
        return SpecsLaptop(name);
    if (n.Contains("iphone")) 
        return SpecsIPhone(name);
    if (n.Contains("samsung") || n.Contains("galaxy")) 
        return SpecsSamsung(name);
    if (n.Contains("huawei") || n.Contains("mate")) 
        return SpecsHuawei(name);
    
    return SpecsDefault(name);
}

[HttpGet]
public JsonResult GetProductSpecs(int id)
{
    var product = db.Products
        .AsNoTracking()
        .FirstOrDefault(p => p.Id == id);
    
    if (product == null)
        return Json(new { error = "Product not found" }, JsonRequestBehavior.AllowGet);

    var specs = BuildSpecs(product.Name);
    return Json(specs, JsonRequestBehavior.AllowGet);
}
```

---

## ?? 3D DISPLAY - Code Examples

### 1. Three.js Setup
```javascript
// Initialize 3D Scene
class ProductViewer3D {
    constructor(containerSelector) {
        this.container = document.querySelector(containerSelector);
        this.scene = new THREE.Scene();
        this.camera = new THREE.PerspectiveCamera(
            75,
            this.container.clientWidth / this.container.clientHeight,
            0.1,
            1000
        );
        this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
        this.renderer.setSize(this.container.clientWidth, this.container.clientHeight);
        this.container.appendChild(this.renderer.domElement);
        
        this.setupLighting();
        this.setupControls();
        this.animate();
    }
    
    setupLighting() {
        // Ambient light
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.5);
        this.scene.add(ambientLight);
        
        // Main directional light
        const mainLight = new THREE.DirectionalLight(0xffffff, 1);
        mainLight.position.set(5, 10, 5);
        this.scene.add(mainLight);
        
        // Fill light
        const fillLight = new THREE.DirectionalLight(0x0088ff, 0.3);
        fillLight.position.set(-5, 5, -10);
        this.scene.add(fillLight);
        
        // Back light
        const backLight = new THREE.PointLight(0xff8800, 0.5);
        backLight.position.set(0, -5, 0);
        this.scene.add(backLight);
    }
    
    setupControls() {
        let isDragging = false;
        let previousMousePosition = { x: 0, y: 0 };
        
        this.container.addEventListener('mousedown', (e) => {
            isDragging = true;
            previousMousePosition = { x: e.clientX, y: e.clientY };
        });
        
        this.container.addEventListener('mousemove', (e) => {
            if (isDragging && this.model) {
                const deltaX = e.clientX - previousMousePosition.x;
                const deltaY = e.clientY - previousMousePosition.y;
                
                this.model.rotation.y += deltaX * 0.01;
                this.model.rotation.x += deltaY * 0.01;
                
                previousMousePosition = { x: e.clientX, y: e.clientY };
            }
        });
        
        this.container.addEventListener('mouseup', () => {
            isDragging = false;
        });
        
        // Zoom with mouse wheel
        this.container.addEventListener('wheel', (e) => {
            e.preventDefault();
            this.camera.position.z += e.deltaY * 0.1;
        });
    }
    
    loadModel(modelPath) {
        const loader = new THREE.GLTFLoader();
        loader.load(modelPath, (gltf) => {
            this.model = gltf.scene;
            this.model.scale.set(1, 1, 1);
            this.model.position.set(0, 0, 0);
            this.scene.add(this.model);
        });
    }
    
    animate() {
        requestAnimationFrame(() => this.animate());
        
        if (this.model) {
            // Auto-rotate when not dragging
            this.model.rotation.y += 0.005;
        }
        
        this.renderer.render(this.scene, this.camera);
    }
}

// Initialize
const viewer = new ProductViewer3D('#3d-viewer');
viewer.loadModel('/models/iphone15pro.glb');
viewer.camera.position.z = 3;
```

### 2. Dynamic Specs Rendering
```javascript
function renderSpecs(specsData) {
    const container = document.querySelector('.specs-container');
    container.innerHTML = '';
    
    Object.entries(specsData.specs).forEach(([categoryName, spec]) => {
        const categoryDiv = document.createElement('div');
        categoryDiv.className = 'spec-category';
        categoryDiv.dataset.category = categoryName;
        
        // Header
        const headerDiv = document.createElement('div');
        headerDiv.className = 'spec-header';
        headerDiv.style.cssText = `
            display: flex;
            align-items: center;
            gap: 16px;
            cursor: pointer;
            padding: 12px;
            border-radius: 8px;
            background: #f5f5f5;
        `;
        
        const iconDiv = document.createElement('div');
        iconDiv.className = 'spec-icon';
        iconDiv.style.cssText = `
            width: 48px;
            height: 48px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 8px;
            background: ${spec.iconBg};
            color: ${spec.iconColor};
            font-weight: 700;
            font-size: 14px;
        `;
        iconDiv.textContent = spec.iconSymbol;
        
        const titleDiv = document.createElement('div');
        titleDiv.innerHTML = `
            <h3 style="margin: 0; font-size: 1.1rem;">${spec.name}</h3>
            <p style="margin: 4px 0 0; color: #666; font-size: 0.9rem;">${spec.sub}</p>
        `;
        
        headerDiv.appendChild(iconDiv);
        headerDiv.appendChild(titleDiv);
        categoryDiv.appendChild(headerDiv);
        
        // Sections
        const sectionsDiv = document.createElement('div');
        sectionsDiv.className = 'spec-sections';
        sectionsDiv.style.cssText = 'padding: 16px; border-left: 4px solid #667eea;';
        
        spec.sections.forEach((section, index) => {
            const sectionDiv = document.createElement('div');
            sectionDiv.className = 'spec-section';
            sectionDiv.style.cssText = `
                margin-bottom: 16px;
                opacity: 0;
                animation: slideIn 0.6s ease-out ${index * 0.1}s forwards;
            `;
            
            let content = `<h4>${section.label}</h4><p>${section.val}</p>`;
            
            if (section.bar !== undefined) {
                content += `
                    <div style="height: 6px; border-radius: 3px; background: #eee; margin-top: 8px; position: relative;">
                        <div style="
                            height: 100%;
                            width: ${section.bar}%;
                            background: linear-gradient(to right, #eee 0%, ${section.barColor} ${section.bar}%);
                            border-radius: 3px;
                            animation: fillBar 0.8s ease-out ${index * 0.1}s forwards;
                        "></div>
                        <span style="
                            position: absolute;
                            right: 0;
                            top: 8px;
                            font-size: 12px;
                            font-weight: 600;
                        ">${section.bar}%</span>
                    </div>
                `;
            }
            
            if (section.tags) {
                content += `<div style="display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px;">
                    ${section.tags.map(tag => 
                        `<span style="
                            background: rgba(0,0,0,0.05);
                            padding: 4px 12px;
                            border-radius: 12px;
                            font-size: 12px;
                            font-weight: 500;
                        ">${tag}</span>`
                    ).join('')}
                </div>`;
            }
            
            sectionDiv.innerHTML = content;
            sectionsDiv.appendChild(sectionDiv);
        });
        
        categoryDiv.appendChild(sectionsDiv);
        container.appendChild(categoryDiv);
    });
}

// Load specs on page load
document.addEventListener('DOMContentLoaded', async () => {
    const productId = document.querySelector('[data-product-id]').dataset.productId;
    const response = await fetch(`/Showcase/GetProductSpecs?id=${productId}`);
    const specsData = await response.json();
    renderSpecs(specsData);
});
```

### 3. CSS Animations
```css
@keyframes slideIn {
    from {
        opacity: 0;
        transform: translateX(-20px);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
}

@keyframes fillBar {
    from {
        width: 0;
    }
    to {
        width: 100%;
    }
}

.spec-category {
    margin-bottom: 16px;
    border-radius: 8px;
    overflow: hidden;
    background: white;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    transition: all 0.3s;
}

.spec-category:hover {
    box-shadow: 0 4px 16px rgba(0,0,0,0.15);
}

.spec-header:hover {
    background: #efefef;
}
```

---

**Code Examples Completed** ?
All implementations are production-ready
