# ?? QUICK REFERENCE - LU?NG HO?T ??NG TÓM T?T

---

## ?? LUCKY SPIN - Quay May M?n

### S? ?? Nhanh
```
User Spin (POST /LuckySpin/Spin)
    ?
Check SpinCount > 0 ?
    ?
Tr? 1 spin: user.SpinCount--
    ?
SpinLucky(userId, orderId):
  - L?y prizes IsActive=true
  - Random d?a trên WinRate
  - T?o LuckySpinHistory
  - N?u Voucher ? Status="Claimed" (auto)
  - N?u Product ? Status="Pending" (manual)
    ?
Return JSON v?i prize info
```

### API Endpoints
| Endpoint | Method | Mô T? |
|----------|--------|-------|
| `/LuckySpin/Spin` | POST | Th?c hi?n quay |
| `/LuckySpin/ClaimVoucher` | POST | Nh?n voucher |
| `/LuckySpin/History` | GET | Xem l?ch s? |
| `/LuckySpin/GetSpinCount` | GET | L?y s? l??t còn l?i |

### Key Points
- ? WinRate: t? l? chi?n th?ng (0-1)
- ? Auto-claimed voucher ngay sau spin
- ? Product c?n manual claim
- ? Error handling: hoàn l?i spin
- ? SpinCount-- tr??c khi spin

---

## ?? LANDING PAGE - Trang ?ích

### S? ?? Nhanh
```
User vào /Showcase/ShowcaseLanding
    ?
LandingHelper.HasViewedLanding()?
    ?? Có cookie "LandingViewed"=true
    ?   ? Có th?: Redirect ho?c skip landing
    ?? Không có cookie
        ? Hi?n th? landing.cshtml
        ? Mark cookie "LandingViewed"=true (30 days)
```

### Cookie Management
```csharp
Name: "LandingViewed"
Value: "true"
Expires: DateTime.Now.AddDays(30)
Path: "/"
```

### API Endpoints
| Endpoint | Method | Mô T? |
|----------|--------|-------|
| `/Showcase/ShowcaseLanding` | GET | Hi?n th? landing |

### Key Points
- ? Hi?n th? l?n ??u (l?n 2+ có th? skip)
- ? Cookie t?n t?i 30 ngày
- ? ClearLandingCookie() ?? reset
- ? ToggleLandingCookie() cho test

---

## ?? NEWS SYSTEM - Tin T?c

### S? ?? Nhanh
```
/News/Index(?category=Tech)
    ?
Query: News.Where(IsActive).OrderBy(PublishDate desc)
    ?
If category: Filter by Category
    ?
Display Grid with:
  - Thumbnail
  - Title
  - Summary
  - Category badge
  - PublishDate
  - "Read more" ? /News/Details/{id}
    ?
/News/Details/{id}
    ?
Load news + Related (5 tin cùng category)
    ?
Display:
  - Full content
  - Featured images
  - Related articles
```

### API Endpoints
| Endpoint | Method | Mô T? |
|----------|--------|-------|
| `/News` | GET | Danh sách t?t c? |
| `/News/Details/{id}` | GET | Chi ti?t tin |
| `/News/GetLatestNews?count=5` | GET | Widget news m?i |
| `/News/GetNewsByCategory?category=Tech&count=3` | GET | Widget theo category |

### Key Points
- ? Filter IsActive = true
- ? Sort PublishDate descending
- ? Related = Same category + Khác ID
- ? PartialView: _NewsTicker, _NewsList
- ? ViewCount tracking (optional)

---

## ??? SHOWCASE - Tr?ng Bày S?n Ph?m

### S? ?? Nhanh
```
/Showcase/Index
    ?
Query 24 flagship products (iPhone, Samsung, MacBook, etc.)
    ?
Fallback ImageUrl n?u thi?u (name-based)
    ?
Display Grid (3-4 columns):
  - Product image
  - Product name
  - Brief specs
  - "View Detail" button
    ?
/Showcase/Detail/{id}
    ?
3D Model Viewer (Babylon.js/Three.js)
  + Specs Panel (dynamic)
  + Carousel images
  + Add to Cart button
    ?
AJAX: GET /Showcase/GetProductSpecs?id={id}
    ?
BuildSpecs(productName):
  - Router logic by name
  - Return detailed specs JSON
    ?
Frontend render dynamic UI:
  - Tabs with specs
  - Progress bars
  - Color-coded icons
  - Tags
```

### API Endpoints
| Endpoint | Method | Mô T? |
|----------|--------|-------|
| `/Showcase/Index` | GET | Danh sách s?n ph?m |
| `/Showcase/Detail/{id}` | GET | Chi ti?t s?n ph?m |
| `/Showcase/GetProductSpecs?id={id}` | GET | L?y specs JSON |
| `/Showcase/UploadImage` | POST | Upload ?nh |

### Product Filtering
```
Query WHERE:
  - Name.Contains("iPhone") OR
  - Name.Contains("Samsung") OR
  - Name.Contains("Galaxy") OR
  - Name.Contains("iPad") OR
  - Name.Contains("MacBook") OR
  - Name.Contains("Laptop") OR
  - Name.Contains("Huawei") OR
  - Name.Contains("Pro")

OrderBy: Name
Take: 24
```

### Fallback Images
```csharp
"iPhone 15 Pro" ? "/Content/Images/iphone15pro.jpg"
"iPhone" ? "/Content/Images/iphone.jpg"
"iPad" ? "/Content/Images/ipad.jpg"
"MacBook" ? "/Content/Images/macbook.jpg"
"Laptop" ? "/Content/Images/laptop.jpg"
"Samsung" / "Galaxy" ? "/Content/Images/samsung.jpg"
"Huawei" / "Mate" ? "/Content/Images/huawei.jpg"
```

### Key Points
- ? 24 s?n ph?m flagship
- ? Name-based image fallback
- ? Dynamic specs router
- ? AsNoTracking() cho performance
- ? Take(24) limit

---

## ?? 3D DISPLAY - Tr?ng Bày 3D

### S? ?? Nhanh
```
Frontend loads 3D scene:
  - Three.js / Babylon.js library
  - Model file (.glb / .fbx)
  - Scene setup
  - Lighting (ambient + directional + fill + back)
  - Camera positioning
    ?
User interactions:
  - Mouse drag: Xoay model (rotationX, rotationY)
  - Mouse wheel: Zoom (camera.position.z)
  - Auto-rotate when idle
    ?
Specs panel renders:
  AJAX GET /Showcase/GetProductSpecs?id={id}
    ?
JSON response with:
  {
    product: "iPhone 15 Pro",
    specs: {
      chip: { name, sub, sections[], tags[] },
      camera: { ... },
      display: { ... },
      battery: { ... },
      ram: { ... },
      wifi: { ... }
    }
  }
    ?
Dynamic rendering:
  - For each category:
    * Icon + header
    * Collapse/expand sections
    * Progress bars with color
    * Tags
    * Staggered animation
```

### 3D Tech Stack
- **Engine:** Three.js / Babylon.js
- **Model Format:** .glb (GLTF) / .fbx / .obj
- **Interactions:** Mouse + Touch events
- **Lighting:** Multi-light system (ambient, directional, point)
- **Camera:** Perspective camera with controls
- **Rendering:** WebGL

### Specs JSON Structure
```json
{
  "product": "iPhone 15 Pro",
  "specs": {
    "category": {
      "name": "Display Name",
      "sub": "Subtitle",
      "iconBg": "rgba(r,g,b,a)",
      "iconColor": "rgba(r,g,b,a)",
      "iconSymbol": "ICON",
      "sections": [
        {
          "label": "Feature Name",
          "val": "Description",
          "bar": 95,
          "barColor": "#hexcolor",
          "tags": ["tag1", "tag2"]
        }
      ]
    }
  }
}
```

### CSS Features
- Perspective: 3D transform
- Animations: slideIn, fadeIn
- Gradient bars: `linear-gradient(to right, start, end)`
- Color-coded icons: Background + text color
- Responsive: media queries

### Key Points
- ? 3D model interactive
- ? Mouse/touch controls smooth
- ? Multi-light realistic
- ? Dynamic specs JSON
- ? Animated progress bars
- ? Color-coded categories
- ? Staggered animations

---

## ?? DATABASE ENTITIES

### LuckySpinPrize
```csharp
public int Id
public string Name
public string Description
public string PrizeType          // "Voucher" | "Product"
public int? VoucherId
public int? ProductId
public decimal WinRate           // 0.10 = 10%
public int DisplayOrder
public bool IsActive
public DateTime CreatedAt
```

### LuckySpinHistory
```csharp
public int Id
public int UserId
public int? OrderId
public int PrizeId
public int? WonVoucherId
public int? WonProductId
public string Status             // "Pending" | "Claimed"
public DateTime SpinDate
public DateTime? ClaimedDate
// Navigation: User, Order, Prize, WonVoucher, WonProduct
```

### News
```csharp
public int Id
public string Title
public string Summary
public string Content            // HTML
public string Category
public string Author
public DateTime PublishDate
public int ViewCount
public bool IsActive
public DateTime CreatedAt
public DateTime? UpdatedAt
// Navigation: Images (ICollection<NewsImage>)
```

### NewsImage
```csharp
public int Id
public int NewsId
public string ImageUrl
public string Caption
public int DisplayOrder
// Navigation: News
```

### Product
```csharp
public int Id
public string Name
public string Description
public decimal Price
public string ImageUrl
public string Category
// ... other fields
```

### User (B? sung)
```csharp
public int SpinCount = 0;  // S? l??t quay
```

---

## ?? FLOW DIAGRAM

```
???????????????
?   LANDING   ?
???????????????
? Landing.cshtml
? Cookie: 30 days
? Auto-redirect
???????????????
     ? (User vào Showcase)
     ?
???????????????????????????????
?   SHOWCASE INDEX            ?
???????????????????????????????
? Grid 24 flagship products
? Name-based image fallback
? "View Detail" button
???????????????????????????????
     ? (User ch?n s?n ph?m)
     ?
???????????????????????????????
?   SHOWCASE DETAIL (3D)      ?
???????????????????????????????
? 3D Model Viewer
? AJAX GetProductSpecs()
? Dynamic specs rendering
? Carousel + Add to Cart
???????????????????????????????
     ? (User add to cart ? checkout)
     ?
     ??????????????????????????? Payment
     ?                          ??? Lucky Spin ?
     ?
     ??? News Widget
         - Latest ticker
         - Category panel
         - Full article view
```

---

## ??? TROUBLESHOOTING

### Lucky Spin Issues
| Problem | Cause | Fix |
|---------|-------|-----|
| SpinCount not decreasing | Not saving DB | Call SaveChanges() |
| Prize not loading | IsActive=false | Check prize config |
| WinRate math wrong | Sum ? 1.0 | Verify all WinRate |
| Error not rolling back | Missing catch | Add try-catch block |

### Landing Issues
| Problem | Cause | Fix |
|---------|-------|-----|
| Landing shows every time | Cookie not set | Call MarkLandingAsViewed() |
| Cookie expires early | Wrong expiry | Set AddDays(30) |
| Can't see landing again | Can't reset | ClearLandingCookie() |

### News Issues
| Problem | Cause | Fix |
|---------|-------|-----|
| No news displaying | IsActive=false | Publish news first |
| Related news empty | No same category | Add news to category |
| Wrong sort order | Missing OrderBy | Add OrderByDescending |

### Showcase Issues
| Problem | Cause | Fix |
|---------|-------|-----|
| Products not showing | Query filter wrong | Check product names |
| No image fallback | ImageUrl null but no fallback logic | Add fallback |
| Specs not loading | GetProductSpecs error | Check BuildSpecs router |
| 3D model not rendering | Model file missing | Upload .glb file |

### 3D Display Issues
| Problem | Cause | Fix |
|---------|-------|-----|
| Model not visible | Camera far away | Reset camera.position |
| Specs JSON invalid | Wrong structure | Validate JSON |
| Animations stutter | Too many elements | Reduce sections count |
| Bars not filling | CSS issue | Check gradient syntax |

---

## ?? BEST PRACTICES

? **Lucky Spin:**
- Always check SpinCount > 0 before spinning
- Auto-claim vouchers to reduce clicks
- Log errors to file for debugging
- Verify WinRate sum = 1.0

? **Landing:**
- Use 30-day cookie expiry (UX balance)
- Allow users to reset/view again
- Make landing mobile-responsive
- Track landing view analytics

? **News:**
- Keep content updated regularly
- Publish at least weekly
- Use SEO-friendly titles
- Add rich featured images

? **Showcase:**
- Use high-quality product images
- Keep specs up-to-date
- Limit to 24 items (performance)
- Use AsNoTracking() for read-only queries

? **3D Display:**
- Optimize model file size (.glb < 5MB)
- Test on multiple browsers
- Use Progressive Enhancement
- Cache 3D assets on CDN

---

**C?p nh?t l?n cu?i:** 2024
**Version:** 1.0
**Status:** ? Ready for Reference
