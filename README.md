# Hệ Thống Quản Lý Khách Sạn (Hotel Management System)

Đây là một dự án ứng dụng web Quản Lý Khách Sạn được xây dựng trên nền tảng **ASP.NET Core 8 MVC**. Ứng dụng cung cấp các công cụ và giao diện cho phép quản trị viên, lễ tân và khách hàng tương tác với hệ thống một cách dễ dàng và hiệu quả.

## 🚀 Các Tính Năng Chính

Dự án được chia thành 3 phân hệ (Areas) dành cho các đối tượng người dùng khác nhau:

### 1. Phân hệ Admin (Quản trị viên)
- Quản lý tài khoản và phân quyền người dùng (ASP.NET Core Identity).
- Quản lý danh mục phòng (Rooms), loại phòng (Room Types) và trạng thái phòng (Room Statuses).
- Theo dõi nhật ký hoạt động của hệ thống (Audit Logs).

### 2. Phân hệ Receptionist (Lễ tân)
- Xem trạng thái phòng theo thời gian thực.
- Quản lý quá trình nhận phòng (Check-in) và trả phòng (Check-out) cho khách hàng.
- Quản lý thông tin đặt phòng.

### 3. Phân hệ Customer (Khách hàng)
- Xem thông tin, hình ảnh và giá cả các loại phòng.
- Thực hiện tìm kiếm và đặt phòng trực tuyến.
- Quản lý thông tin cá nhân và lịch sử đặt phòng của bản thân.

## 🛠 Công Nghệ Sử Dụng

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core 8.0
- **Authentication/Authorization**: ASP.NET Core Identity
- **Front-end**: HTML, CSS, JavaScript, Bootstrap (hoặc thư viện UI tương đương)

## 💻 Hướng Dẫn Cài Đặt và Chạy Dự Án

### Yêu cầu hệ thống
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) hoặc SQL Server Express LocalDB
- IDE: Visual Studio 2022 hoặc Visual Studio Code.

### Các bước cài đặt

1. **Clone repository:**
   ```bash
   git clone https://github.com/Nghoangmin1/HotelManagerment
   cd QuanLyKhachSan
   ```

2. **Cập nhật Connection String (nếu cần):**
   Mở file `QuanLyKhachSan/appsettings.json` và kiểm tra cấu hình kết nối database:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuanLyKhachSanDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
   *Lưu ý: Bạn có thể thay đổi chuỗi kết nối sao cho phù hợp với SQL Server trên máy bạn.*

3. **Chạy Entity Framework Core Migrations để tạo Database:**
   Mở Package Manager Console (trong Visual Studio) hoặc Terminal và chạy:
   ```bash
   # Nếu dùng .NET CLI
   dotnet ef database update
   
   # Nếu dùng Package Manager Console
   Update-Database
   ```

4. **Chạy ứng dụng:**
   Sử dụng Visual Studio (nhấn `F5` hoặc `Ctrl + F5`) hoặc chạy lệnh sau qua .NET CLI:
   ```bash
   dotnet run
   ```

5. **Truy cập ứng dụng:**
   Mở trình duyệt và truy cập vào địa chỉ thông thường là `http://localhost:5000` hoặc `https://localhost:5001`.

## 🤝 Đóng góp
Nếu bạn có bất kỳ ý tưởng nào để cải thiện dự án, vui lòng mở một Issue hoặc tạo Pull Request.

---
*Dự án thuộc bài tập môn Quản lý dự án phần mềm.*
