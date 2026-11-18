# H? Th?ng Giao Di?n Dark Tech - H??ng D?n S? D?ng

## ?? T?ng Quan

Toàn b? website (User, Admin, Community) ?ã ???c th?ng nh?t v?i **Dark Theme (#07091A)** và **H? th?ng màu s?c Semantic** theo thi?t k? m?i.

---

## ?? H? Th?ng Bi?n CSS (CSS Variables)

T?t c? các bi?n ???c ??nh ngh?a trong file `Content/theme.css` d??i `:root`. B?n có th? s? d?ng chúng ? b?t k? file CSS nào:

### Màu N?n (Backgrounds)
```css
--bg-primary: #07091A          /* N?n chính trang */
--bg-secondary: #0F1628        /* N?n ph? */
--bg-tertiary: #1E2942         /* N?n cho form, input */
--bg-card: #111827             /* N?n card/box */
--bg-hover: #1E293B            /* Màu hover */
```

### Màu V?n B?n (Text)
```css
--text-primary: #F1F5F9        /* V?n b?n chính */
--text-secondary: #94A3B8      /* V?n b?n ph? */
--text-tertiary: #64748B       /* V?n b?n m? */
--text-disabled: #475569       /* V?n b?n b? vô hi?u */
```

### Màu Semantic (Cho Button & Badge)
```css
--color-primary: #7C3AED       /* Purple - Action chính */
--color-cyan: #06B6D4          /* Cyan - View/Info */
--color-success: #10B981       /* Green - Xác nh?n/Active */
--color-danger: #EF4444        /* Red - Xóa */
--color-warning: #F59E0B       /* Orange - C?nh báo */
--color-gray: #6B7280          /* Gray - Secondary */
```

### Border & Shadow
```css
--border-color: rgba(255, 255, 255, 0.08)
--border-radius: 12px
--border-radius-sm: 8px

--shadow-md: 0 4px 6px rgba(0, 0, 0, 0.1)
--shadow-purple: 0 4px 24px rgba(124, 58, 237, 0.15)
--shadow-glow: 0 0px 32px rgba(124, 58, 237, 0.3)
```

---

## ?? H? Th?ng Button Semantic

**Quy t?c: 1 màu = 1 ngh?a**

### Primary Button - Purple (Action chính)
```html
<!-- HTML -->
<button class="btn btn-primary">Mua Ngay</button>

<!-- Class s?n có -->
.btn-primary          /* Bootstrap default */
.btn-lift             /* Có effect nâng lên khi hover */
```

**Khi dùng:**
- Nút hành ??ng chính (Mua, ??t, G?i)
- Nút xác nh?n quan tr?ng

---

### Info/Cyan Button - Cyan (Xem/Thông tin)
```html
<button class="btn btn-info">Xem Chi Ti?t</button>

<!-- Custom class -->
<button class="btn btn-info-custom">Xem Thêm</button>
```

**Khi dùng:**
- Xem chi ti?t s?n ph?m
- Xem thêm thông tin
- Link/nút info

---

### Success Button - Green (Xác nh?n/Active)
```html
<button class="btn btn-success">Xác Nh?n</button>

<!-- Custom class -->
<button class="btn btn-success-custom">L?u</button>
```

**Khi dùng:**
- Xác nh?n hành ??ng
- ?ánh d?u hoàn t?t
- Kích ho?t tính n?ng

---

### Danger Button - Red (Xóa)
```html
<button class="btn btn-danger">Xóa</button>

<!-- Custom class -->
<button class="btn btn-danger-custom">Xóa V?nh Vi?n</button>
```

**Khi dùng:**
- Xóa item
- H?y ??n hàng
- Hành ??ng nguy hi?m

---

### Secondary Button - Gray (Quay l?i/Ph?)
```html
<button class="btn btn-secondary">Quay L?i</button>

<!-- Custom class -->
<button class="btn btn-secondary-custom">H?y</button>
```

**Khi dùng:**
- Quay l?i
- H?y b?
- Hành ??ng ph?

---

## ?? Typography (C? ch? & Tô ??m)

```css
h1 { font-size: 32px; font-weight: 700; }      /* Tiêu ?? l?n */
h2 { font-size: 24px; font-weight: 600; }      /* Tiêu ?? */
h3 { font-size: 18px; font-weight: 600; }      /* Tiêu ?? ph? */
p  { font-size: 15px; font-weight: 400; }      /* N?i dung */
small { font-size: 13px; font-weight: 400; }   /* Chú thích */

/* Font: Inter ho?c Be Vietnam Pro (Google Fonts) */
font-family: 'Inter', 'Be Vietnam Pro', sans-serif;
```

---

## ?? Card Styling

T?t c? card ?ã ???c c?u hình trong `theme.css` v?i:
- **Background:** `var(--bg-card)`
- **Border:** 1px solid `var(--border-color)`
- **Shadow:** `var(--shadow-md)` + hover ? `var(--shadow-purple)`
- **Hover Effect:** Nâng lên 4px + border ??i màu purple

```html
<!-- Bootstrap default card s? t? áp d?ng theme -->
<div class="card">
    <div class="card-header">Tiêu ??</div>
    <div class="card-body">N?i dung</div>
</div>

<!-- Product card -->
<div class="product-card">
    <div class="product-image">
        <img src="..." />
    </div>
    <div class="product-info">
        <div class="product-name">Tên s?n ph?m</div>
        <div class="product-price">Giá ti?n</div>
    </div>
</div>
```

---

## ?? Badge & Label

```html
<!-- Primary Badge - Purple -->
<span class="badge badge-primary">Hot</span>

<!-- Success Badge - Green -->
<span class="badge badge-success">Còn hàng</span>

<!-- Danger Badge - Red -->
<span class="badge badge-danger">H?t hàng</span>

<!-- Warning Badge - Orange -->
<span class="badge badge-warning">C?nh báo</span>
```

---

## ?? Form Styling

T?t c? `<input>`, `<select>`, `<textarea>` ?ã ???c t? ??ng áp d?ng Dark Theme:

```html
<!-- T? ??ng có giao di?n Dark Theme -->
<input type="text" class="form-control" placeholder="Nh?p...">
<select class="form-select">
    <option>Ch?n...</option>
</select>

<!-- Label -->
<label class="form-label">Tên tr??ng</label>

<!-- Checkbox -->
<input type="checkbox" class="form-check-input">
<label class="form-check-label">??ng ý</label>
```

**??c ?i?m:**
- Background: `var(--bg-tertiary)`
- Border: `var(--border-color)`
- Focus: Border ??i thành `var(--color-primary)` + Glow effect

---

## ?? Layout & Navbar

### User Layout (`_Layout.cshtml`)
- **Navbar:** Gradient Purple
- **Background:** Dark (#07091A)
- **Font:** Inter / Be Vietnam Pro

### Admin Layout (`_AdminLayout.cshtml`)
- **Sidebar:** C? ??nh bên trái, Dark theme
- **Active Item:** Highlight b?ng n?n purple m? + border trái
- **Topbar:** Card style v?i thông tin admin

### Community Layout (`_CommunityLayout.cshtml`)
- **Header:** Gradient Purple (gi?ng User)
- **Navigation:** Buttons v?i semant colorics
- **Container:** Dark card style

---

## ?? Áp D?ng Vào View hi?n t?i

### 1. Product List/Detail
```html
<!-- List view -->
<div class="row">
    @foreach (var product in products)
    {
        <div class="col-md-4">
            <div class="product-card">
                <div class="product-image">
                    <img src="@product.ImageUrl" />
                </div>
                <div class="product-info">
                    <h4 class="product-name">@product.Name</h4>
                    <div class="product-price">@product.Price.ToString("C")</div>
                    
                    @if (product.InStock)
                    {
                        <span class="badge badge-success">Còn hàng</span>
                    }
                    else
                    {
                        <span class="badge badge-danger">H?t hàng</span>
                    }
                    
                    <button class="btn btn-primary btn-sm mt-2">Mua</button>
                </div>
            </div>
        </div>
    }
</div>
```

### 2. Admin Form
```html
<form method="post">
    <div class="card">
        <div class="card-header">
            <h5>T?o S?n Ph?m</h5>
        </div>
        <div class="card-body">
            <div class="mb-3">
                <label class="form-label">Tên s?n ph?m</label>
                <input type="text" class="form-control" name="Name" />
            </div>
            
            <div class="mb-3">
                <label class="form-label">Giá</label>
                <input type="number" class="form-control" name="Price" />
            </div>
        </div>
        <div class="card-footer">
            <button type="submit" class="btn btn-primary">T?o</button>
            <button type="button" class="btn btn-secondary">H?y</button>
        </div>
    </div>
</form>
```

### 3. Alert/Notification
```html
<!-- Success Alert -->
<div class="alert alert-success">
    <i class="fas fa-check-circle"></i> Thao tác thành công!
</div>

<!-- Error Alert -->
<div class="alert alert-danger">
    <i class="fas fa-exclamation-circle"></i> Có l?i x?y ra!
</div>

<!-- Warning Alert -->
<div class="alert alert-warning">
    <i class="fas fa-exclamation-triangle"></i> C?nh báo!
</div>

<!-- Info Alert -->
<div class="alert alert-info">
    <i class="fas fa-info-circle"></i> Thông tin!
</div>
```

---

## ?? Animation Classes

```html
<!-- Fade In Up Animation -->
<div class="fade-in-up">N?i dung</div>

<!-- Slide In Right Animation -->
<div class="slide-in-right">Thông báo</div>

<!-- Glow Effect -->
<button class="btn btn-primary glow">Glow Button</button>

<!-- Smooth Transition -->
<div class="smooth-transition">Box s? smooth khi transition</div>
```

---

## ?? Responsive Design

Toàn b? CSS ?ã h? tr? responsive t? mobile ??n desktop:

```css
/* Mobile: < 576px */
@media (max-width: 576px) {
    /* T? ??ng ?i?u ch?nh */
}

/* Tablet: < 768px */
@media (max-width: 768px) {
    /* T? ??ng ?i?u ch?nh */
}

/* Desktop: >= 992px */
/* Hi?n th? bình th??ng */
```

---

## ? Checklist Áp D?ng

- [x] ?u tiên 1: Unify background - Dark Theme (#07091A) ?
- [x] ?u tiên 2: Semantic button colors (Purple, Cyan, Green, Red, Gray) ?
- [x] ?u tiên 3: Card styling + Box-shadow + Border hover ?
- [x] ?u tiên 4: Admin sidebar highlight active item ?
- [x] T?t c? View (User + Admin + Community) th?ng nh?t giao di?n ?
- [x] Typography & Spacing rules ?

---

## ?? L?u Ý Khi S?a CSS

1. **Luôn dùng CSS Variables** thay vì hardcode màu s?c
   ```css
   /* ? Sai */
   color: #7C3AED;
   
   /* ? ?úng */
   color: var(--color-primary);
   ```

2. **Khi t?o button m?i**, luôn xác ??nh semantic color:
   ```html
   <!-- Action chính -->
   <button class="btn btn-primary">Hành ??ng</button>
   
   <!-- Xem thêm info -->
   <button class="btn btn-info">Xem</button>
   
   <!-- Xóa nguy hi?m -->
   <button class="btn btn-danger">Xóa</button>
   ```

3. **Không thêm CSS inline vào HTML**, t?o class trong file CSS:
   ```html
   <!-- ? Tránh -->
   <div style="color: red; font-size: 20px;">...</div>
   
   <!-- ? ?úng -->
   <div class="my-custom-text">...</div>
   <!-- R?i ??nh ngh?a trong CSS -->
   ```

4. **Ki?m tra Dark theme trên t?t c? trang** tr??c khi deploy

---

## ?? H? Tr?

N?u c?n tùy ch?nh thêm, hãy s?a các file:
- `Content/theme.css` - Bi?n CSS & global styles
- `Content/effects.css` - Button & animation effects
- `Content/Site.css` - Overrides
- `Views/Shared/_Layout.cshtml` - Layout User
- `Areas/Admin/Views/Shared/_AdminLayout.cshtml` - Layout Admin
- `Areas/Community/Views/Shared/_CommunityLayout.cshtml` - Layout Community

---

**? Giao di?n Dark Tech Professional ?ã s?n sàng s? d?ng!**
