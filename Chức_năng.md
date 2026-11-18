# Tài liệu Chức Năng Hệ Thống TechWorld

Phiên bản: 1.0  
Ngày cập nhật: 2024  
Công nghệ: ASP.NET MVC 5, Entity Framework 6, SQL Server

---

## I. TỔNG QUAN HỆ THỐNG

Hệ thống TechWorld là nền tảng thương mại điện tử chuyên bán hàng công nghệ với các tính năng: quản lý sản phẩm, giỏ hàng, thanh toán, voucher khuyến mại, sự kiện tương tác, vòng quay may mắn, cộng đồng người dùng, và quản lý admin.

---

## II. CÁC CHỨC NĂNG CHÍNH

### 1. CHỨC NĂNG QUẢN LÝ NGƯỜI DÙNG (Authentication & Authorization Module)

#### 1.1 Đăng Ký Tài Khoản (Registration)
- Mô tả: Người dùng tạo tài khoản mới trên hệ thống
- Controller: `AccountController`
- Action: `Register` [HttpPost] (1)
- HTTP Method: POST
- Tham số:
  - `fullName`: Họ và tên người dùng (string)
  - `email`: Email đăng ký (string, unique)
  - `phone`: Số điện thoại (string)
  - `password`: Mật khẩu (string, mã hóa)
  - `confirmPassword`: Xác nhận mật khẩu
- Yêu cầu:
  - Email phải là định dạng hợp lệ
  - Email không được trùng lặp trong hệ thống
  - Mật khẩu tối thiểu 6 ký tự
  - Mật khẩu được mã hóa bằng `PasswordHasherService` [Service] (2)
- Đầu ra (Response):
  - Tài khoản được tạo với role mặc định "User"
  - Redirect đến trang Login
- Chủ thể tác động: User (người dùng cuối)
- Model liên quan: `User` [Model] (3), `PasswordHasherService` [Service] (2)
- Luồng hoạt động:
  ```
  User nhập thông tin → Validate dữ liệu → Mã hóa password 
  → Lưu vào DB → Thành công/Lỗi
  ```

#### 1.2 Đăng Nhập Tài Khoản (Login)
- Mô tả: Xác thực và cấp phiên đăng nhập cho người dùng
- Controller: `AccountController`
- Action: `Login` [HttpPost] (1)
- HTTP Method: POST
- Tham số:
  - `email`: Email đăng nhập
  - `password`: Mật khẩu
  - `rememberMe`: Ghi nhớ đăng nhập (boolean)
- Yêu cầu:
  - Email phải tồn tại trong hệ thống
  - Mật khẩu phải khớp (so sánh hash)
  - Tài khoản không được bị khóa (banned)
- Đầu ra (Response):
  - Session được tạo với keys: `UserId`, `FullName`, `Role`, `Email`
  - FormsAuthentication Cookie được tạo (nếu rememberMe = true)
  - Redirect đến HomePage
- Chủ thể tác động: User (người dùng cuối)
- Model liên quan: `User`, `LoginAttempt` [Model] (3), `AuthenticationService` [Service] (2)
- Security: Rate limiting [Filter] (4) để ngăn chặn brute force attack
- Luồng hoạt động:
  ```
  User nhập tên đăng nhập/password → Validate → Check tài khoản tồn tại 
  → Kiểm tra mật khẩu → Check status banned 
  → Tạo session + Cookie → Redirect HomePage
  ```

#### 1.3 Đăng Xuất (Logout)
- Mô tả: Kết thúc phiên đăng nhập hiện tại
- Controller: `AccountController`
- Action: `Logout` [HttpPost] (1)
- Yêu cầu: [Authorize] (5) - Phải đã đăng nhập
- Đầu ra (Response):
  - Session được xóa hoàn toàn
  - FormsAuthentication Cookie bị loại bỏ
  - Redirect tới HomePage
- Luồng hoạt động:
  ```
  User click Logout → Xóa Session → Xóa Cookie → Redirect HomePage
  ```

#### 1.4 Quảng Lý Tài Khoản Người Dùng (User Profile Management)
- Mô tả: Người dùng cập nhật thông tin cá nhân
- Controller: `AccountProfileController`
- Action: `MyAccount` [HttpGet/HttpPost] (1)
- Yêu cầu: [Authorize] (5)
- Tham số (POST):
  - `fullName`: Họ và tên cập nhật
  - `phone`: Số điện thoại cập nhật
  - `address`: Địa chỉ
  - `birthDate`: Ngày sinh
- Đầu ra (Response):
  - Thông tin User được cập nhật trong DB
  - Session được làm mới
- Chủ thể tác động: User (người dùng cuối)
- Model liên quan: `User` [Model] (3)

---

### 2. CHỨC NĂNG QUẢN LÝ SẢN PHẨM (Product Management Module)

#### 2.1 Danh Sách Sản Phẩm (Product Listing)
- Mô tả: Hiển thị danh sách toàn bộ sản phẩm có hỗ trợ lọc và tìm kiếm
- Controller: `ProductController`
- Action: `Index` [HttpGet] (1)
- HTTP Method: GET
- Tham số Query:
  - `categoryId`: Lọc theo danh mục (int, optional)
  - `minPrice`: Giá tối thiểu (decimal, optional)
  - `maxPrice`: Giá tối đa (decimal, optional)
  - `searchKeyword`: Từ khóa tìm kiếm (string, optional)
- Đầu ra (Response):
  - Danh sách sản phẩm đã lọc (pagination cũng được hỗ trợ)
  - Danh sách danh mục để lọc
- Chủ thể tác động: User (người dùng cuối), Public (người truy cập không đăng nhập)
- Model liên quan: `Product` [Model] (3), `Category` [Model] (3)
- Luồng hoạt động:
  ```
  User truy cập trang → Áp dụng filter → Query DB 
  → Hiển thị kết quả → Hiển thị danh mục
  ```

#### 2.2 Chi Tiết Sản Phẩm (Product Details)
- Mô tả: Hiển thị thông tin chi tiết của một sản phẩm
- Controller: `ProductController`
- Action: `Details` [HttpGet] (1)
- Tham số:
  - `id`: ID sản phẩm (int)
- Đầu ra (Response):
  - Thông tin chi tiết sản phẩm (tên, giá, mô tả, hình ảnh, stock)
  - Danh sách sản phẩm liên quan (cùng danh mục)
  - Nút "Thêm vào giỏ" và "Mua ngay"
- Chủ thể tác động: User, Public
- Model liên quan: `Product`, `Category`, `OrderDetail` [Model] (3)

#### 2.3 Quản Lý Sản Phẩm (CRUD - Admin)
- Mô tả: Admin tạo, sửa, xóa sản phẩm
- Controller: `Areas/Admin/Controllers/ProductController`
- Actions:
  - `Create` [HttpGet/HttpPost] (1)
  - `Edit` [HttpGet/HttpPost] (1)
  - `Delete` [HttpGet/HttpPost] (1)
  - `Index` [HttpGet] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Tham số (Create/Edit):
  - `name`: Tên sản phẩm
  - `description`: Mô tả
  - `price`: Giá bán (decimal)
  - `stock`: Số lượng tồn kho (int)
  - `categoryId`: ID danh mục
  - `imageUrl`: URL hình ảnh
  - `isHot`: Có phải sản phẩm hot không (boolean)
- Đầu ra (Response):
  - Sản phẩm được tạo/sửa/xóa
  - Redirect tới danh sách sản phẩm
- Chủ thể tác động: Admin (quản trị viên)
- Model liên quan: `Product`, `Category`

---

### 3. CHỨC NĂNG QUẢN LÝ GIỎ HÀNG (Shopping Cart Module)

#### 3.1 Thêm Sản Phẩm Vào Giỏ (Add to Cart)
- Mô tả: Thêm một sản phẩm vào giỏ hàng của người dùng
- Controller: `CartController`
- Action: `Add` [HttpPost] (1)
- Tham số:
  - `productId`: ID sản phẩm (int)
  - `quantity`: Số lượng (int, mặc định 1)
- Yêu cầu:
  - Sản phẩm phải tồn tại
  - Số lượng > 0
- Đầu ra (Response):
  - CartItem được thêm/cập nhật trong Session
  - JSON: `{ success: true, cartCount: X }`
- Chủ thể tác động: User
- Model liên quan: `CartItem` [Model] (3), `Product`
- Lưu trữ: Session variable `CartItems` (danh sách các item trong giỏ)
- Luồng hoạt động:
  ```
  User click "Thêm vào giỏ" → Lấy CartItems từ Session 
  → Kiểm tra product tồn tại → Thêm/Cập nhật quantity 
  → Lưu lại Session → Return JSON
  ```

#### 3.2 Cập Nhật Số Lượng (Update Quantity)
- Mô tả: Thay đổi số lượng sản phẩm trong giỏ hàng
- Controller: `CartController`
- Action: `Update` [HttpPost] (1)
- Tham số:
  - `productId`: ID sản phẩm (int)
  - `quantity`: Số lượng mới (int)
- Đầu ra (Response):
  - Quantity được cập nhật hoặc item bị xóa (nếu quantity ≤ 0)
- Chủ thể tác động: User
- Model liên quan: `CartItem`

#### 3.3 Xóa Sản Phẩm Khỏi Giỏ (Remove from Cart)
- Mô tả: Loại bỏ sản phẩm khỏi giỏ hàng
- Controller: `CartController`
- Action: `Remove` [HttpPost] (1)
- Tham số:
  - `productId`: ID sản phẩm
- Đầu ra (Response):
  - Item bị xóa khỏi Session
  - JSON: `{ success: true, cartCount: X }`
- Chủ thể tác động: User

#### 3.4 Lấy Số Lượng Giỏ Hàng (Get Cart Count)
- Mô tả: Trả về số lượng sản phẩm trong giỏ (dùng để cập nhật badge)
- Controller: `CartController`
- Action: `GetCartCount` [HttpGet] (1)
- Yêu cầu: AllowGet = true
- Đầu ra (Response):
  - JSON: `{ count: X }`
- Chủ thể tác động: User

---

### 4. CHỨC NĂNG THANH TOÁN & ĐƠN HÀNG (Checkout & Order Module)

#### 4.1 Trang Thanh Toán (Checkout)
- Mô tả: Xem thông tin giỏ hàng, nhập thông tin giao hàng, chọn mã giảm giá trước khi thanh toán
- Controller: `CartController`
- Action: `Checkout` [HttpGet] (1)
- Yêu cầu: [Authorize] (5), giỏ hàng không trống
- Đầu ra (Response):
  - Hiển thị danh sách sản phẩm trong giỏ
  - Form nhập thông tin giao hàng: Tên, SĐT, Địa chỉ
  - Combobox phương thức thanh toán (COD, VNPay)
  - Input chọn/nhập mã giảm giá
  - Hiển thị tổng tiền, phí vận chuyển
- Chủ thể tác động: User (người dùng đã đăng nhập)
- Model liên quan: `Order` [Model] (3), `CartItem`, `Voucher` [Model] (3)

#### 4.2 Lấy Danh Sách Voucher Khả Dụng (Get User Vouchers)
- Mô tả: Lấy danh sách voucher có thể áp dụng cho đơn hàng hiện tại của user
- Controller: `CartController`
- Action: `GetUserVouchers` [HttpGet] (1)
- Yêu cầu: [Authorize] (5)
- Đầu ra (Response):
  ```json
  {
    "success": true,
    "vouchers": [
      {
        "Id": 1,
        "Code": "SALE30",
        "Name": "Giảm 30%",
        "Description": "...",
        "DiscountDisplay": "-30%",
        "EndDate": "dd/MM/yyyy HH:mm",
        "DaysLeft": 5,
        "IsPrivate": true,
        "MinOrderAmount": "100000đ"
      }
    ]
  }
  ```
- Logic Lấy Voucher:
  1. Voucher private của user (từ Lucky Spin, còn hạn)
  2. Voucher public đang hoạt động (còn hạn, đủ số lượng, chưa dùng)
  3. Sắp xếp theo ngày hết hạn (gần nhất lên trước)
  4. Chỉ lấy voucher có điều kiện tối thiểu ≤ tổng tiền giỏ
- Chủ thể tác động: User
- Model liên quan: `Voucher`, `VoucherUsage` [Model] (3)
- Luồng hoạt động:
  ```
  User click "Chọn mã" → Ajax gọi GetUserVouchers 
  → Filter vouchers → Sort by EndDate 
  → Trả về danh sách → Hiển thị modal
  ```

#### 4.3 Áp Dụng Mã Giảm Giá (Apply Voucher)
- Mô tả: Validate và áp dụng voucher cho đơn hàng hiện tại
- Controller: `CartController`
- Action: `ApplyVoucher` [HttpPost] (1)
- Tham số:
  - `code`: Mã voucher (string)
- Validation:
  - Mã phải tồn tại
  - Phải còn hạn (`StartDate <= now < EndDate`)
  - Phải còn số lượng (nếu public)
  - User phải đủ điều kiện (tổng tiền ≥ `MinOrderAmount`)
  - Voucher phải active
  - Nếu yêu cầu event participation: user phải tham gia event đó
  - Nếu public: user chưa dùng voucher này
- Đầu ra (Response):
  ```json
  {
    "success": true,
    "discountAmount": 20000,
    "message": "Áp dụng thành công! Tiết kiệm 20000đ"
  }
  ```
- Session Update:
  - `Session["AppliedVoucher"]` = Code
  - `Session["VoucherDiscount"]` = DiscountAmount
- Chủ thể tác động: User
- Model liên quan: `Voucher`, `VoucherService` [Service] (2)
- Luồng hoạt động:
  ```
  User nhập/chọn mã → Gửi AJAX → VoucherService.ValidateVoucher 
  → Check điều kiện → Nếu OK: Lưu Session + Return discount 
  → JS cập nhật giao diện
  ```

#### 4.4 Đặt Hàng (Place Order)
- Mô tả: Tạo đơn hàng mới và lưu vào DB
- Controller: `CartController`
- Action: `PlaceOrder` [HttpPost] (1)
- Yêu cầu: [Authorize] (5), giỏ không trống, đã điền thông tin giao hàng
- Tham số:
  - `shippingName`: Tên người nhận (string)
  - `shippingPhone`: SĐT giao hàng (string)
  - `shippingAddress`: Địa chỉ giao hàng (string)
  - `paymentMethod`: Phương thức TT (string: "COD" hoặc "VNPay")
  - `voucherCode`: Mã voucher đã áp dụng (string, optional)
- Đầu ra (Response):
  ```json
  {
    "success": true,
    "orderId": 123,
    "message": "Đặt hàng thành công"
  }
  ```
- Xử lý Dữ Liệu:
  1. Tính toán tiền:
     - Subtotal = Tổng giá sản phẩm
     - Discount = Lấy từ Session (nếu có)
     - Shipping = 30,000đ
     - Total = Subtotal - Discount + Shipping
  2. Tạo Order record với status = "Pending"
  3. Tạo OrderDetails cho mỗi CartItem
  4. Cập nhật Stock sản phẩm (giảm theo quantity mua)
  5. Nếu có voucher: Tạo VoucherUsage record + cập nhật UsedQuantity
  6. Xóa CartItems từ Session
  7. Clear Session voucher
- Side Effects:
  - Product.Stock được giảm
  - Voucher.UsedQuantity được tăng (nếu public)
  - VoucherUsage record được tạo
- Chủ thể tác động: User
- Model liên quan: `Order`, `OrderDetail`, `Voucher`, `VoucherUsage`, `Product`
- Luồng hoạt động:
  ```
  User click "Xác nhận đơn" → Validate thông tin → 
  PlaceOrder() → Tính tiền → Tạo Order + OrderDetails → 
  Cập nhật Stock → Xử lý Voucher → Xóa Cart → 
  Return orderId → Redirect LuckySpin
  ```

#### 4.5 Xem Lịch Sử Đơn Hàng (My Orders)
- Mô tả: Hiển thị danh sách tất cả đơn hàng của user
- Controller: `AccountProfileController`
- Action: `MyOrders` [HttpGet] (1)
- Yêu cầu: [Authorize] (5)
- Đầu ra (Response):
  - Danh sách Order của user (sắp xếp theo ngày giảm dần)
  - Mỗi Order hiển thị: ID, Ngày, Trạng thái, Tổng tiền, Link chi tiết
- Chủ thể tác động: User
- Model liên quan: `Order`

#### 4.6 Chi Tiết Đơn Hàng (Order Details)
- Mô tả: Xem chi tiết một đơn hàng cụ thể
- Controller: `AccountProfileController`
- Action: `OrderDetails` [HttpGet] (1)
- Tham số:
  - `id`: ID Order (int)
- Yêu cầu: 
  - [Authorize] (5)
  - Order phải thuộc user đó (Security check)
- Đầu ra (Response):
  - Thông tin đơn hàng: ID, Ngày, Trạng thái
  - Thông tin giao hàng: Tên, SĐT, Địa chỉ
  - Danh sách OrderDetails: Sản phẩm, số lượng, giá
  - Mã voucher áp dụng (nếu có)
  - Tổng tiền chi tiết: Subtotal, Discount, Shipping, Total
- Chủ thể tác động: User
- Model liên quan: `Order`, `OrderDetail`, `Product`

---

### 5. CHỨC NĂNG QUẢN LÝ VOUCHER (Voucher & Promotion Module)

#### 5.1 Danh Sách Voucher Công Khai (Public Vouchers)
- Mô tả: Hiển thị danh sách tất cả voucher có thể sử dụng
- Controller: `VoucherController`
- Action: `Index` [HttpGet] (1)
- Đầu ra (Response):
  - Danh sách voucher active, còn hạn
  - Không yêu cầu tham gia event (RequireEventParticipation = false)
  - Sắp xếp theo % giảm giá (giảm dần)
- Chủ thể tác động: User, Public
- Model liên quan: `Voucher`

#### 5.2 Quản Lý Voucher (Admin)
- Mô tả: Admin tạo, sửa, xóa voucher
- Controller: `Areas/Admin/Controllers/VoucherController`
- Actions:
  - `Create` [HttpGet/HttpPost] (1)
  - `Edit` [HttpGet/HttpPost] (1)
  - `Delete` [HttpGet/HttpPost] (1)
  - `Index` [HttpGet] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Tham số (Create/Edit):
  - `code`: Mã voucher (string, unique, UPPERCASE)
  - `name`: Tên voucher
  - `description`: Mô tả chi tiết
  - `discountPercent`: % giảm (0-100)
  - `discountAmount`: Giảm theo tiền cố định (VNĐ)
  - `startDate`: Ngày bắt đầu có hiệu lực
  - `endDate`: Ngày kết thúc (phải > startDate)
  - `quantity`: Số lượng voucher (0 = vô hạn)
  - `minOrderAmount`: Tối thiểu đơn hàng để áp dụng
  - `isActive`: Kích hoạt/Vô hiệu (boolean)
  - `isPrivate`: Riêng tư (true) hoặc công khai (false)
  - `ownerUserId`: ID chủ sở hữu (nếu private)
  - `requireEventParticipation`: Cần tham gia event không
  - `eventId`: ID event liên kết (nếu có)
  - `isLuckySpinVoucher`: Từ vòng quay may mắn (boolean, system auto set)
- Validation:
  - Mã phải unique
  - % giảm: 0-100
  - Giảm theo tiền: ≥ 0
  - StartDate < EndDate
  - Nếu discountPercent > 0 và discountAmount > 0: Cảnh báo (chỉ dùng 1 trong 2)
- Đầu ra (Response):
  - Voucher được tạo/sửa/xóa
  - Redirect danh sách voucher
- Chủ thể tác động: Admin
- Model liên quan: `Voucher`

#### 5.3 Xem Voucher Của Tôi (My Vouchers)
- Mô tả: User xem danh sách voucher mà họ sở hữu hoặc có quyền sử dụng
- Controller: `AccountProfileController`
- Action: `MyVouchers` [HttpGet] (1)
- Yêu cầu: [Authorize] (5)
- Đầu ra (Response):
  - Danh sách VoucherViewModel (Voucher + IsUsed flag)
  - Bao gồm:
    1. Private vouchers của user
    2. Vouchers từ event mà user tham gia
    3. Public vouchers (admin tạo)
  - Hiển thị: Mã, Tên, % giảm, Ngày hết hạn, Trạng thái (Đã dùng/Còn)
- Chủ thể tác động: User
- Model liên quan: `Voucher`, `VoucherViewModel`, `VoucherUsage`, `EventParticipant`
- Luồng hoạt động:
  ```
  User click "Voucher của tôi" → Lấy private vouchers 
  → Lấy event vouchers → Lấy public vouchers 
  → Loại duplicate → Sort by CreatedAt 
  → Hiển thị danh sách
  ```

---

### 6. CHỨC NĂNG SỰ KIỆN & QUIZ (Event & Interactive Quiz Module)

#### 6.1 Danh Sách Sự Kiện (Events Listing)
- Mô tả: Hiển thị sự kiện đang diễn ra, sắp tới, và đã qua
- Controller: `VoucherController`
- Action: `Events` [HttpGet] (1)
- Đầu ra (Response):
  - Sự kiện đang diễn ra (StartDate ≤ now < EndDate)
  - Sự kiện sắp tới (StartDate > now)
  - Sự kiện đã qua (EndDate < now)
  - Mỗi sự kiện: Tên, ảnh, mô tả, ngày bắt đầu/kết thúc
- Chủ thể tác động: User, Public
- Model liên quan: `Event` [Model] (3), `EventParticipant` [Model] (3)

#### 6.2 Chi Tiết Sự Kiện & Tham Gia (Event Detail & Participate)
- Mô tả: Xem chi tiết sự kiện và tham gia nếu là thành viên đăng nhập
- Controller: `VoucherController`
- Action: `EventDetail` [HttpGet] (1)
- Tham số:
  - `id`: ID Event (int)
- Đầu ra (Response):
  - Thông tin sự kiện: Tên, ảnh, mô tả, ngày, câu hỏi
  - Danh sách voucher liên kết
  - Nút "Tham gia" (nếu user chưa tham gia)
  - Status tham gia (nếu user đã tham gia)
- Chủ thể tác động: User, Public
- Model liên quan: `Event`, `Voucher`, `EventParticipant`, `EventQuestion` [Model] (3)

#### 6.3 Tham Gia Sự Kiện (Participate Event)
- Mô tả: User tham gia vào sự kiện để được trả lời câu hỏi
- Controller: `VoucherController`
- Action: `ParticipateEvent` [HttpPost] (1)
- Yêu cầu: [Authorize] (5)
- Tham số:
  - `eventId`: ID Event (int)
- Validation:
  - User phải đã đăng nhập
  - Event phải tồn tại
  - User chưa tham gia event này trước đó
- Đầu ra (Response):
  ```json
  {
    "success": true,
    "participantId": 456,
    "questionCount": 5
  }
  ```
- Xử lý Dữ Liệu:
  - Tạo EventParticipant record
  - Score = 0, DiscountPercent = 0 ban đầu
- Chủ thể tác động: User
- Model liên quan: `EventParticipant`
- Luồng hoạt động:
  ```
  User click "Tham gia" → Check user login → 
  Check event exists → Check not participated → 
  Tạo EventParticipant → Return questionCount
  ```

#### 6.4 Lấy Danh Sách Câu Hỏi Event (Get Event Questions)
- Mô tả: Lấy danh sách câu hỏi quiz của sự kiện
- Controller: `VoucherController`
- Action: `GetEventQuestions` [HttpGet] (1)
- Tham số:
  - `eventId`: ID Event (int)
- Yêu cầu: [Authorize] (5)
- Đầu ra (Response):
  ```json
  [
    {
      "Id": 1,
      "Question": "Câu hỏi 1?",
      "OptionA": "A",
      "OptionB": "B",
      "OptionC": "C",
      "OptionD": "D"
    }
  ]
  ```
- Chủ thể tác động: User
- Model liên quan: `EventQuestion`

#### 6.5 Nộp Bài Quiz (Submit Quiz)
- Mô tả: User nộp câu trả lời và nhận kết quả + voucher
- Controller: `VoucherController`
- Action: `SubmitQuiz` [HttpPost] (1)
- Yêu cầu: [Authorize] (5)
- Tham số:
  - `participantId`: ID EventParticipant (int)
  - `answers`: Danh sách câu trả lời (format: "1:A,2:B,3:C,...")
- Validation:
  - User phải đã đăng nhập
  - Participant phải tồn tại
  - Participant.UserId phải = current user (security check)
- Xử lý:
  1. Parse answers string thành Dictionary
  2. So sánh với CorrectAnswer của mỗi EventQuestion
  3. Tính score = (correctCount * 10) / totalQuestions
  4. Tính discount % dựa trên score:
     - Score ≥ 7: 25%
     - Score ≥ 5: 20%
     - Score ≥ 1: 15%
     - Nếu 0 đúng: 0%
  5. Cập nhật EventParticipant: Score, DiscountPercent, CompletedAt
  6. Tự động cấp voucher private cho user (trong LuckySpinPrizeController)
- Đầu ra (Response):
  ```json
  {
    "success": true,
    "score": 8,
    "correctCount": 4,
    "totalQuestions": 5,
    "discountPercent": 25,
    "message": "Bạn đã trả lời đúng 4/5 câu. Điểm: 8/10. Nhận 25% giảm giá!"
  }
  ```
- Chủ thể tác động: User
- Model liên quan: `EventParticipant`, `EventQuestion`
- Luồng hoạt động:
  ```
  User submit quiz → Parse answers → Tính score 
  → Tính discount % → Cập nhật EventParticipant 
  → Return kết quả → Trigger voucher creation
  ```

#### 6.6 Quản Lý Sự Kiện (Admin)
- Mô tả: Admin tạo, sửa, xóa, quản lý sự kiện
- Controller: `Areas/Admin/Controllers/EventController`
- Actions:
  - `Create` [HttpGet/HttpPost] (1)
  - `Edit` [HttpGet/HttpPost] (1)
  - `Delete` [HttpGet/HttpPost] (1)
  - `Detail` [HttpGet] (1)
  - `Index` [HttpGet] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Tham số (Create/Edit):
  - `name`: Tên sự kiện
  - `description`: Mô tả
  - `startDate`: Ngày bắt đầu
  - `endDate`: Ngày kết thúc (phải > startDate)
  - `imageUrl`: URL ảnh banner
  - `isActive`: Active/Inactive
- Chủ thể tác động: Admin
- Model liên quan: `Event`

#### 6.7 Quản Lý Câu Hỏi Event (Admin)
- Mô tả: Admin thêm, xóa, sửa câu hỏi cho sự kiện
- Controller: `Areas/Admin/Controllers/EventController`
- Actions:
  - `CreateQuestion` [HttpPost] (1)
  - `DeleteQuestion` [HttpPost] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Tham số (CreateQuestion):
  - `eventId`: ID Event (int)
  - `question`: Nội dung câu hỏi (string)
  - `optionA`, `optionB`, `optionC`, `optionD`: Các lựa chọn
  - `correctAnswer`: Đáp án đúng ("A", "B", "C", hoặc "D")
- Validation:
  - Câu hỏi không được trống
  - Đáp án đúng phải là A/B/C/D
- Đầu ra (Response):
  - JSON: `{ success: true/false, message: ... }`
- Chủ thể tác động: Admin
- Model liên quan: `EventQuestion`

---

### 7. CHỨC NĂNG VÒNG QUAY MAY MẮN (Lucky Spin Module)

#### 7.1 Vòng Quay May Mắn (Lucky Spin Game)
- Mô tả: User quay vòng may mắn để nhận giải thưởng
- Controller: `LuckySpinController`
- Action: `Index` [HttpGet] (1), `Spin` [HttpPost] (1)
- Yêu cầu: [Authorize] (5)
- Đầu ra (Response Spin):
  ```json
  {
    "success": true,
    "prizeId": 1,
    "prizeName": "Giảm 30%",
    "prizeType": "Voucher",
    "angle": 45
  }
  ```
- Logic:
  1. Tính toán giải thắng dựa trên WinRate của LuckySpinPrize
  2. Tạo LuckySpinHistory record
  3. Nếu là voucher: Tạo private voucher cho user
  4. Nếu là sản phẩm: Thêm vào giỏ hàng hoặc tạo special order
  5. Trả về angle cho animation
- Chủ thể tác động: User
- Model liên quan: `LuckySpinPrize` [Model] (3), `LuckySpinHistory` [Model] (3), `LuckySpinService` [Service] (2)
- Luồng hoạt động:
  ```
  User click quay → Ajax Spin → Random dựa WinRate 
  → Tạo LuckySpinHistory → 
  Nếu Voucher: Tạo private voucher → 
  Nếu Product: Thêm order/gift 
  → Return angle → Animate spin → Hiển thị giải
  ```

#### 7.2 Quản Lý Giải Thưởng (Admin)
- Mô tả: Admin quản lý các giải thưởng trong vòng quay
- Controller: `Areas/Admin/Controllers/LuckySpinPrizeController`
- Actions:
  - `Index` [HttpGet] (1)
  - `Edit` [HttpPost] (1)
- Tham số (Edit):
  - `id`: ID Prize (int)
  - `name`: Tên giải
  - `description`: Mô tả
  - `winRate`: Tỷ lệ trúng (0-100, % cơ hội)
  - `prizeType`: Loại giải ("Voucher" hoặc "Product")
  - `voucherId`: ID Voucher (nếu là voucher)
  - `productId`: ID Product (nếu là product)
  - `displayOrder`: Thứ tự hiển thị trên vòng quay (1-8)
  - `isActive`: Kích hoạt/Vô hiệu
- Chủ thể tác động: Admin
- Model liên quan: `LuckySpinPrize`, `Voucher`, `Product`

#### 7.3 Lịch Sử Quay (Lucky Spin History - Admin)
- Mô tả: Admin xem lịch sử tất cả người dùng quay
- Controller: `Areas/Admin/Controllers/LuckySpinPrizeController`
- Action: `History` [HttpGet] (1), `UserSpinDetail` [HttpGet] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Đầu ra (Response):
  - Danh sách LuckySpinHistory (sắp xếp theo ngày mới nhất)
  - Mỗi entry: User, Giải thắng, Ngày, Trạng thái
- Chủ thể tác động: Admin
- Model liên quan: `LuckySpinHistory`, `User`, `LuckySpinPrize`

#### 7.4 Cấp Voucher Cho User Quay (Grant Voucher - Admin)
- Mô tả: Admin thủ công cấp voucher nếu hệ thống chưa cấp tự động
- Controller: `Areas/Admin/Controllers/LuckySpinPrizeController`
- Action: `GrantVoucher` [HttpPost] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Tham số:
  - `spinHistoryId`: ID LuckySpinHistory (int)
- Xử lý:
  1. Kiểm tra LuckySpinHistory tồn tại
  2. Kiểm tra Prize là voucher type
  3. Kiểm tra chưa được cấp (WonVoucherId null)
  4. Tạo private voucher từ base voucher
  5. Cập nhật WonVoucherId, Status = "Claimed", ClaimedDate
- Chủ thể tác động: Admin
- Model liên quan: `LuckySpinHistory`, `Voucher`, `LuckySpinPrize`

---

### 8. CHỨC NĂNG TIN TỨC (News Management Module)

#### 8.1 Danh Sách Tin Tức (News Listing)
- Mô tả: Hiển thị danh sách tin tức mới nhất
- Controller: `NewsController`
- Action: `Index` [HttpGet] (1)
- Đầu ra (Response):
  - Danh sách News active (sắp xếp theo ngày publish giảm dần)
  - Mỗi tin: Tiêu đề, tóm tắt, hình thumbnail, ngày publish
- Chủ thể tác động: User, Public
- Model liên quan: `News` [Model] (3)

#### 8.2 Chi Tiết Tin Tức (News Details)
- Mô tả: Xem nội dung đầy đủ của một bài tin
- Controller: `NewsController`
- Action: `Details` [HttpGet] (1)
- Tham số:
  - `id`: ID News (int)
- Chủ thể tác động: User, Public
- Model liên quan: `News`, `NewsImage` [Model] (3)

#### 8.3 Quản Lý Tin Tức (Admin)
- Mô tả: Admin tạo, sửa, xóa tin tức
- Controller: `Areas/Admin/Controllers/NewsController`
- Actions:
  - `Create` [HttpGet/HttpPost] (1)
  - `Edit` [HttpGet/HttpPost] (1)
  - `Delete` [HttpGet/HttpPost] (1)
  - `Index` [HttpGet] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Tham số (Create/Edit):
  - `title`: Tiêu đề
  - `content`: Nội dung (HTML/Rich text)
  - `category`: Danh mục ("Technology", "Defense", etc)
  - `publishDate`: Ngày publish (auto = now nếu create)
  - `imageUrl`: URL ảnh thumbnail
  - `isActive`: Kích hoạt/Vô hiệu
- Chủ thể tác động: Admin
- Model liên quan: `News`, `NewsImage`

---

### 9. CHỨC NĂNG CỘNG ĐỒNG (Community Module)

#### 9.1 Danh Sách Bài Viết (Posts Listing)
- Mô tả: Hiển thị danh sách bài viết từ cộng đồng
- Controller: `Areas/Community/Controllers/PostController`
- Action: `Index` [HttpGet] (1)
- Đầu ra (Response):
  - Danh sách Post active, không bị xóa (sắp xếp theo ngày tạo giảm dần)
  - Mỗi bài: Tiêu đề, nội dung tóm tắt, tác giả, số bình luận, số like
- Chủ thể tác động: User, Public
- Model liên quan: `Post` [Model] (3), `User`

#### 9.2 Chi Tiết Bài Viết (Post Details)
- Mô tả: Xem bài viết đầy đủ cùng bình luận
- Controller: `Areas/Community/Controllers/PostController`
- Action: `Detail` [HttpGet] (1)
- Tham số:
  - `id`: ID Post (int)
- Đầu ra (Response):
  - Nội dung bài viết đầy đủ
  - Danh sách Comment
  - Form thêm bình luận (nếu user đã login)
  - Số lượt like
- Chủ thể tác động: User, Public
- Model liên quan: `Post`, `Comment` [Model] (3), `Like` [Model] (3), `Reaction` [Model] (3)

#### 9.3 Tạo Bài Viết (Create Post)
- Mô tả: User viết bài viết mới đăng lên cộng đồng
- Controller: `Areas/Community/Controllers/PostController`
- Action: `Create` [HttpGet/HttpPost] (1)
- Yêu cầu: [Authorize] (5)
- Tham số (POST):
  - `title`: Tiêu đề bài viết
  - `content`: Nội dung
  - `imageUrl`: URL ảnh (optional)
- Validation:
  - Title không được trống
  - Content tối thiểu 10 ký tự
- Xử lý:
  - Tạo Post record với UserId = current user
  - CreatedAt = now
  - IsDeleted = false
- Chủ thể tác động: User
- Model liên quan: `Post`, `User`

#### 9.4 Chỉnh Sửa Bài Viết (Edit Post)
- Mô tả: User chỉnh sửa bài viết của mình
- Controller: `Areas/Community/Controllers/PostController`
- Action: `Edit` [HttpGet/HttpPost] (1)
- Yêu cầu: 
  - [Authorize] (5)
  - Chỉ chủ sở hữu bài viết hoặc admin mới được sửa
- Chủ thể tác động: User (chủ sở hữu), Admin

#### 9.5 Xóa Bài Viết (Delete Post)
- Mô tả: User xóa bài viết của mình (soft delete)
- Controller: `Areas/Community/Controllers/PostController`
- Action: `Delete` [HttpPost] (1)
- Yêu cầu: 
  - [Authorize] (5)
  - Chỉ chủ sở hữu hoặc admin
- Xử lý:
  - Set Post.IsDeleted = true (không xóa hẳn từ DB)
  - UpdatedAt = now
- Chủ thể tác động: User, Admin

#### 9.6 Bình Luận (Comments)
- Mô tả: User bình luận trên bài viết
- Controller: `Areas/Community/Controllers/CommentController`
- Actions:
  - `Create` [HttpPost] (1)
  - `Delete` [HttpPost] (1)
- Yêu cầu: [Authorize] (5)
- Tham số (Create):
  - `postId`: ID Post (int)
  - `content`: Nội dung comment (string)
- Validation:
  - Content không được trống
  - Content tối thiểu 1 ký tự
- Xử lý:
  - Tạo Comment record
  - CreatedAt = now
- Chủ thể tác động: User
- Model liên quan: `Comment`, `Post`, `User`

#### 9.7 Like & Reaction (Like & Reaction System)
- Mô tả: User like bài viết hoặc comment, hoặc react (emoji)
- Controller: `Areas/Community/Controllers/LikeController`, `ReactionController`
- Actions:
  - `Like` [HttpPost] (1) - Toggle like
  - `AddReaction` [HttpPost] (1) - Thêm reaction
- Yêu cầu: [Authorize] (5)
- Tham số:
  - `postId` hoặc `commentId`: ID object được like
  - `reactionType`: Loại reaction ("love", "haha", "wow", etc) - cho Reaction action
- Logic:
  - Toggle: Nếu user đã like, xóa Like record; nếu chưa, tạo mới
  - Reaction: Lưu Reaction record với type
- Chủ thể tác động: User
- Model liên quan: `Like`, `Reaction`, `Post`, `Comment`

#### 9.8 Trang Cá Nhân (User Profile - Community)
- Mô tả: Xem trang cá nhân của user trong cộng đồng
- Controller: `Areas/Community/Controllers/ProfileController`
- Action: `Index` [HttpGet] (1)
- Tham số:
  - `userId`: ID User (int) - optional, mặc định = user hiện tại
- Đầu ra (Response):
  - Thông tin user: Avatar, tên, bio, số bài viết, số lượt like
  - Danh sách bài viết của user
  - Nút follow/unfollow (nếu không phải chính mình)
- Chủ thể tác động: User, Public
- Model liên quan: `User`, `Post`

---

### 10. CHỨC NĂNG TIN NHẮN & THÔNG BÁO (Messaging & Notification Module)

#### 10.1 Gửi Tin Nhắn Đến Admin (Send Message)
- Mô tả: User gửi tin nhắn tới admin/quản lý
- Controller: `HomeController`
- Action: `SendMessage` [HttpPost] (1)
- Yêu cầu: [Authorize] (5)
- Tham số:
  - `content`: Nội dung tin nhắn (string, max 5000 ký tự)
- Validation:
  - Content không được trống
  - Độ dài ≤ 5000 ký tự
- Xử lý:
  1. Tạo Message record
  2. IsRead = false
  3. CreatedAt = now
  4. Tự động xóa tin nhắn cũ hơn 15 ngày
- Chủ thể tác động: User
- Model liên quan: `Message` [Model] (3)

#### 10.2 Lấy Thông Báo (Get Notifications)
- Mô tả: Lấy danh sách thông báo cho user
- Controller: `HomeController`
- Action: `GetNotifications` [HttpGet] (1)
- Yêu cầu: [Authorize] (5)
- Đầu ra (Response):
  ```json
  {
    "notifications": [
      {
        "title": "Tin nhắn từ Admin",
        "message": "Tóm tắt tin nhắn...",
        "type": "info"
      }
    ]
  }
  ```
- Logic:
  - Lấy Message unread từ user
  - Nếu user là admin: Cũng lấy Message unread từ các user khác
  - Sắp xếp theo ngày mới nhất
  - Lấy tối đa 5-8 thông báo
- Chủ thể tác động: User, Admin
- Model liên quan: `Message`, `User`
- Ghi chú: Được gọi định kỳ (mỗi 30 giây) qua JavaScript

---

### 11. CHỨC NĂNG LIÊN HỆ (Contact Module)

#### 11.1 Trang Liên Hệ (Contact Page)
- Mô tả: Hiển thị form liên hệ
- Controller: `HomeController`
- Action: `Contact` [HttpGet] (1)
- Đầu ra (Response):
  - Form liên hệ: Tên, Email, SĐT, Tin nhắn
  - Thông tin liên lạc của công ty

#### 11.2 Gửi Liên Hệ (Submit Contact)
- Mô tả: User gửi form liên hệ
- Controller: `HomeController`
- Action: `Contact` [HttpPost] (1)
- Tham số:
  - `name`: Tên người liên hệ (string)
  - `email`: Email (string)
  - `phone`: SĐT (string, optional)
  - `message`: Nội dung (string)
- Validation:
  - Name, email, message bắt buộc
  - Email phải hợp lệ
- Xử lý:
  - Ghi log hoặc gửi email (hiện tại chỉ debug log)
  - Hiển thị thông báo thành công
- Chủ thể tác động: User, Public

---

### 12. CHỨC NĂNG TRANG CHỦ (Home Page Module)

#### 12.1 Trang Landing (Landing Page)
- Mô tả: Trang chào mừng hiển thị 1 lần duy nhất khi truy cập lần đầu
- Controller: `HomeController`
- Action: `Index` [HttpGet] (1)
- Xử lý:
  - Check Session["LandingShown"]
  - Nếu null hoặc false: Hiển thị Landing → Set Session = true
  - Nếu true: Redirect đến HomePage
- Chủ thể tác động: User, Public
- Luồng hoạt động:
  ```
  User lần đầu → Check Session → 
  Hiển thị Landing → Set Session → 
  Lần sau: Redirect HomePage
  ```

#### 12.2 Trang Chủ (Home Page)
- Mô tả: Trang chủ chính hiển thị sản phẩm nổi bật, tin tức
- Controller: `HomeController`
- Action: `HomePage` [HttpGet] (1)
- Đầu ra (Response):
  - Danh sách sản phẩm hot (IsHot = true, lấy 6 sản phẩm)
  - Danh sách tin tức mới nhất (lấy 5 tin)
  - Danh sách danh mục
- Chủ thể tác động: User, Public
- Model liên quan: `Product`, `News`, `Category`

#### 12.3 Trang Giới Thiệu (About Page)
- Mô tả: Trang giới thiệu về công ty
- Controller: `HomeController`
- Action: `About` [HttpGet] (1)
- Chủ thể tác động: User, Public

---

### 13. CHỨC NĂNG QUẢN TRỊ (Admin Dashboard Module)

#### 13.1 Dashboard Admin
- Mô tả: Trang tổng quan các thống kê quan trọng
- Controller: `Areas/Admin/Controllers/HomeController`
- Action: `Index` [HttpGet] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Đầu ra (Response):
  - Tổng số sản phẩm
  - Tổng số user
  - Tổng số đơn hàng
  - Tổng doanh thu (từ đơn delivered)
  - Thống kê đơn hàng theo trạng thái (Pending, Processing, Delivered, Cancelled)
  - Biểu đồ doanh thu 7 ngày gần nhất
  - Thống kê sản phẩm theo danh mục
- Chủ thể tác động: Admin
- Model liên quan: `Product`, `User`, `Order`, `News`

#### 13.2 Quản Lý Đơn Hàng (Orders Management)
- Mô tả: Admin xem và quản lý tất cả đơn hàng
- Controller: `Areas/Admin/Controllers/OrderController`
- Actions:
  - `Index` [HttpGet] (1)
  - `Details` [HttpGet] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Đầu ra (Response):
  - Danh sách tất cả Order
  - Mỗi Order: ID, Khách hàng, Ngày, Trạng thái, Tổng tiền
  - Link update trạng thái
- Chủ thể tác động: Admin
- Model liên quan: `Order`, `OrderDetail`, `User`

#### 13.3 Quản Lý Người Dùng (Users Management)
- Mô tả: Admin quản lý tài khoản người dùng
- Controller: `Areas/Admin/Controllers/UserController`
- Actions:
  - `Index` [HttpGet] (1)
  - `Edit` [HttpPost] (1)
  - `Ban/Unban` [HttpPost] (1)
- Yêu cầu: [Authorize] (5), Role = "Admin"
- Chủ thể tác động: Admin
- Model liên quan: `User`, `UserBan` [Model] (3)

---

### 14. CHỨC NĂNG TRƯNG BÀY (Showcase Module)

#### 14.1 Danh Sách Trưng Bày (Showcase Listing)
- Mô tả: Hiển thị các sản phẩm trưng bày (demo, mẫu)
- Controller: `ShowcaseController`
- Action: `Index` [HttpGet] (1)
- Chủ thể tác động: User, Public
- Model liên quan: `Product`

#### 14.2 Chi Tiết Trưng Bày (Showcase Details)
- Mô tả: Xem chi tiết sản phẩm trưng bày
- Controller: `ShowcaseController`
- Action: `Detail` [HttpGet] (1)
- Chủ thể tác động: User, Public

---

## III. CÁC MÔ HÌNH DỮ LIỆU (Data Models)

### 1. User [Model] (3)
- Properties: Id, Email, FullName, Phone, PasswordHash, Role, IsActive, CreatedAt, UpdatedAt, Orders, Messages, Posts, Likes, Comments

### 2. Product [Model] (3)
- Properties: Id, Name, Description, Price, Stock, ImageUrl, Category, IsHot, CreatedAt

### 3. Order [Model] (3)
- Properties: Id, UserId, OrderDate, ShippingName, ShippingPhone, ShippingAddress, PaymentMethod, Status, TotalAmount, TotalVoucherDiscount, OrderDetails, AppliedVoucherCodes

### 4. OrderDetail [Model] (3)
- Properties: Id, OrderId, ProductId, Quantity, UnitPrice, TotalPrice

### 5. CartItem [Model] (3)
- Properties: ProductId, ProductName, Price, Quantity, ImageUrl, TotalPrice (calculated)

### 6. Voucher [Model] (3)
- Properties: Id, Code, Name, Description, DiscountPercent, DiscountAmount, StartDate, EndDate, Quantity, UsedQuantity, MinOrderAmount, IsActive, IsPrivate, OwnerUserId, IsLuckySpinVoucher, RequireEventParticipation, EventId, CreatedAt

### 7. VoucherUsage [Model] (3)
- Properties: Id, VoucherId, UserId, OrderId, UsedAt, Notes

### 8. Event [Model] (3)
- Properties: Id, Name, Description, StartDate, EndDate, ImageUrl, IsActive, CreatedAt

### 9. EventParticipant [Model] (3)
- Properties: Id, EventId, UserId, Score, DiscountPercent, ParticipatedAt, CompletedAt

### 10. EventQuestion [Model] (3)
- Properties: Id, EventId, Question, OptionA, OptionB, OptionC, OptionD, CorrectAnswer

### 11. LuckySpinPrize [Model] (3)
- Properties: Id, Name, Description, WinRate, PrizeType, VoucherId, ProductId, DisplayOrder, IsActive, CreatedAt

### 12. LuckySpinHistory [Model] (3)
- Properties: Id, UserId, PrizeId, WonVoucherId, WonProductId, SpinDate, Status, ClaimedDate

### 13. News [Model] (3)
- Properties: Id, Title, Content, Category, PublishDate, ImageUrl, IsActive, CreatedAt

### 14. NewsImage [Model] (3)
- Properties: Id, NewsId, ImageUrl, DisplayOrder, CreatedAt

### 15. Post [Model] (3)
- Properties: Id, UserId, Title, Content, ImageUrl, CreatedAt, UpdatedAt, IsDeleted, Comments, Likes, Reactions

### 16. Comment [Model] (3)
- Properties: Id, PostId, UserId, Content, CreatedAt, UpdatedAt, IsDeleted, Likes

### 17. Like [Model] (3)
- Properties: Id, PostId, UserId, CreatedAt (chỉ dùng toggle)

### 18. Reaction [Model] (3)
- Properties: Id, PostId, UserId, ReactionType, CreatedAt

### 19. Message [Model] (3)
- Properties: Id, UserId, Content, CreatedAt, IsRead, User

### 20. Category [Model] (3)
- Properties: Id, Name, Description, Products

### 21. UserBan [Model] (3)
- Properties: Id, UserId, Reason, BanDate, UnbanDate, BannedBy

---

## IV. CÁC LAYERS & SERVICES

### 1. Services [Service] (2)
- `VoucherService`: Validate và tính toán voucher
- `LuckySpinService`: Logic spin và random giải
- `PasswordHasherService`: Mã hóa/kiểm tra password
- `AuthenticationService`: Xác thực user
- `EmailService`: Gửi email
- `OtpService`: Quản lý OTP
- `NewsImageService`: Quản lý ảnh tin tức
- `AuditService`: Ghi log hoạt động

### 2. Filters [Filter] (4)
- `AuthorizeAttribute`: Kiểm tra quyền đăng nhập
- `RateLimitAttribute`: Giới hạn request (chống brute force)
- `HandleErrorAttribute`: Xử lý lỗi toàn cục
- `SecurityHeadersAttribute`: Thêm headers bảo mật
- `MaintenanceCheckAttribute`: Kiểm tra trạng thái bảo trì

### 3. Controllers [Controller] (1)
- `HomeController`: Trang chủ, tin nhắn, thông báo
- `ProductController`: Quản lý sản phẩm
- `CartController`: Giỏ hàng, checkout, đơn hàng, voucher
- `VoucherController`: Danh sách voucher, sự kiện, quiz
- `NewsController`: Tin tức
- `AccountController`: Đăng ký, đăng nhập, đăng xuất
- `AccountProfileController`: Hồ sơ người dùng
- `ShowcaseController`: Trưng bày sản phẩm
- `LuckySpinController`: Vòng quay may mắn
- `HealthCheckController`: Kiểm tra trạng thái hệ thống
- Admin Controllers:
  - `AdminController`: Dashboard admin
  - `ProductController`: Quản lý sản phẩm
  - `NewsController`: Quản lý tin tức
  - `OrderController`: Quản lý đơn hàng
  - `UserController`: Quản lý người dùng
  - `VoucherController`: Quản lý voucher
  - `EventController`: Quản lý sự kiện
  - `LuckySpinPrizeController`: Quản lý giải thưởng
- Community Controllers:
  - `PostController`: Quản lý bài viết
  - `CommentController`: Quản lý bình luận
  - `LikeController`: Quản lý like
  - `ReactionController`: Quản lý reaction
  - `ProfileController`: Hồ sơ cộng đồng

---

## V. SECURITY & AUTHORIZATION

### 1. Authentication [Authorize] (5)
- Session-based: UserId, FullName, Role, Email lưu trong Session
- FormsAuthentication: Cookie cho "Remember Me"
- Password: Mã hóa bcrypt qua PasswordHasherService

### 2. Authorization
- [Authorize]: Chỉ user đã đăng nhập
- [Authorize(Roles = "Admin")]: Chỉ admin
- Manual checks: Role check trong action (Session["Role"] == "Admin")

### 3. Security Measures
- CSRF Protection: [ValidateAntiForgeryToken] (6) trên POST requests
- Rate Limiting: [RateLimitAttribute] (4) để chống brute force
- SQL Injection Prevention: Entity Framework + Parameterized queries
- User Ban System: Kiểm tra UserBan status
- Input Validation: ModelState.IsValid checks

---

## VI. API ENDPOINTS (JSON AJAX)

### Cart Module
- `POST /Cart/Add` - Thêm sản phẩm
- `POST /Cart/Update` - Cập nhật quantity
- `POST /Cart/Remove` - Xóa sản phẩm
- `GET /Cart/GetCartCount` - Lấy số lượng
- `POST /Cart/ApplyVoucher` - Áp dụng mã
- `GET /Cart/GetUserVouchers` - Danh sách voucher

### Order Module
- `POST /Cart/PlaceOrder` - Đặt hàng

### Voucher Module
- `POST /Voucher/ParticipateEvent` - Tham gia sự kiện
- `GET /Voucher/GetEventQuestions` - Lấy câu hỏi
- `POST /Voucher/SubmitQuiz` - Nộp quiz
- `POST /Voucher/ValidateCode` - Kiểm tra mã

### Community Module
- `POST /Community/Comment/Create` - Thêm bình luận
- `POST /Community/Like/Like` - Toggle like
- `POST /Community/Reaction/AddReaction` - Thêm reaction

### Home Module
- `GET /Home/GetNotifications` - Lấy thông báo
- `POST /Home/SendMessage` - Gửi tin nhắn

---

## VII. GHI CHÚ KỸ THUẬT

### Session Timeout
- Mặc định: 20 phút (cấu hình Web.config)
- Dữ liệu tạm: CartItems, AppliedVoucher, VoucherDiscount, LandingShown

### Database
- SQL Server (Entity Framework 6)
- ConnectionString: Web.config
- Migrations: Code-first

### Validation
- Client-side: HTML5, Bootstrap validation
- Server-side: DataAnnotations, Custom validation

### Error Handling
- Try-catch với logging
- 404 HttpNotFound() cho resource not found
- 403 Redirect khi không có quyền
- Generic error page [HandleErrorAttribute]

### Performance
- Caching: CacheService (optional implementation)
- Async/await: Có hỗ trợ (optional)
- Query optimization: .AsNoTracking() trên admin danh sách

---

## VIII. LIÊN KẾT GIỮA CÁC CHỨC NĂNG

```
┌─────────────────────────────────────────────────────────────────┐
│                          HOME PAGE                               │
│  ├─→ Product Listing ─→ Add to Cart ─→ Checkout ─→ Place Order │
│  ├─→ News Listing ─→ News Details                               │
│  ├─→ Events ─→ Event Detail ─→ Quiz ─→ Voucher                 │
│  ├─→ Voucher Listing ─→ Apply Voucher                           │
│  ├─→ Lucky Spin ─→ Win Prize (Voucher)                          │
│  └─→ Community ─→ Posts ─→ Comments & Reactions                 │
│                                                                   │
│  USER PROFILE                                                     │
│  ├─→ My Account ─→ Update Info                                   │
│  ├─→ My Orders ─→ Order Details                                  │
│  ├─→ My Vouchers ─→ View Vouchers                                │
│  └─→ Community Profile ─→ Posts                                  │
│                                                                   │
│  ADMIN DASHBOARD                                                  │
│  ├─→ Products Management (CRUD)                                  │
│  ├─→ News Management (CRUD)                                      │
│  ├─→ Orders Management ─→ Details                                │
│  ├─→ Users Management ─→ Ban/Unban                               │
│  ├─→ Vouchers Management (CRUD)                                  │
│  ├─→ Events Management (CRUD) ─→ Questions (CRUD)               │
│  └─→ Lucky Spin Prizes (CRUD) ─→ History ─→ Grant Voucher      │
└─────────────────────────────────────────────────────────────────┘
```

---

## IX. KÝ HIỆU & CHỈ SỐ THAM CHIẾU

- (1) [HttpGet/HttpPost] - HTTP method attribute
- (2) [Service] - Business logic service class
- (3) [Model] - Data model/Entity class
- (4) [Filter] - Action filter attribute
- (5) [Authorize] - Authorization attribute
- (6) [ValidateAntiForgeryToken] - CSRF protection attribute

---

Tài liệu này được sử dụng để:
- Vẽ biểu đồ Use Case
- Thiết kế Sequence Diagram
- Lập kế hoạch phát triển
- Bảo trì và mở rộng hệ thống
