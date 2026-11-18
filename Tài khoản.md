# Tài khoản dùng thử đã tạo

1.0/ Tài khoản Admin (Toàn quyền)

- Tên đăng nhập: admin123
- Mật khẩu: Admin@123456

Vai trò: Admin
Email: admin@techworld.vn

1.1/ Khách hàng mẫu 1 (Người dùng thường)

-Tên đăng nhập: testuser
- Mật khẩu: Test@123456
Vai trò: Customer

1.2/ Khách hàng mẫu 2 (Người dùng thường)

- Tên đăng nhập: john_doe
- Mật khẩu: John@123456
Vai trò: Customer

1.3/ Khách hàng mẫu 3 (Người dùng thường)
- Tên đăng nhập: phucngo
- Mật khẩu:

1.5/ Khách hàng mẫu 4 (Người dùng thường)

# Cách tạo tài khoản

-> Cách 1: Tự động (Khuyên dùng)
Hệ thống tự động chạy lệnh Seed dữ liệu khi ứng dụng khởi chạy lần đầu. Mật khẩu được mã hóa bảo mật bằng thuật toán PBKDF2.

-> Cách 2: Chạy SQL thủ côngco
Sử dụng file CreateAdminAccount.sql trong thư mục Data/Migrations để nạp trực tiếp vào SQL Server.

-> Cách 3: Sử dụng Code
Gọi hàm AdminAccountSeeder.SeedAdminAccount(db) trong mã nguồn C#.

# Bảo mật mật khẩu

Thuật toán: PBKDF2 kết hợp SHA256.

Cơ chế: Sử dụng Salt (chuỗi muối) ngẫu nhiên và lặp 10.000 lần để chống tấn công dò mật khẩu.

Định dạng lưu trữ: Base64 của chuỗi (Salt + Hash).

# Kiểm tra đăng nhập

-> Truy cập đường dẫn: /Account/Login
-> Nhập tài khoản: admin123 / Admin@123456
-> Kết quả mong đợi: Đăng nhập thành công, chuyển hướng về trang quản trị (Dashboard), hệ thống nhận diện đúng vai trò Admin.

# Xử lý sự cố thường gặp

-> Lỗi đăng nhập sai dù mật khẩu đúng: Do cơ sở dữ liệu chưa được khởi tạo. 
-> Cách sửa: Xóa database hiện tại và nhấn F5 để hệ thống tự xây dựng lại.

-> Không vào được trang Admin: Kiểm tra lại phân quyền (Role) trong database xem đã đúng là "Admin" chưa.
-> Lỗi mã hóa mật khẩu: Do database cũ dùng chuẩn SHA256 cũ. -> Cách sửa: Xóa bản ghi người dùng cũ và chạy lại lệnh Seed dữ liệu.

# Danh sách kiểm tra an toàn (Checklist)

1./ Mật khẩu không để dạng văn bản thuần (phải mã hóa).
2./ Sử dụng Salt riêng biệt cho mỗi người dùng.
3./ Sử dụng HTTPS để truyền dữ liệu.
4./ Cài đặt Cookie ở chế độ HttpOnly và Secure.
5./ Luôn kiểm tra quyền (Role) ở phía máy chủ (Server-side).