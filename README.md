AuthServer - .NET 10 Identity Service
Dự án AuthServer là một hệ thống xác thực (Authentication) tập trung được xây dựng trên nền tảng ASP.NET Core Web API. Dự án tập trung vào việc xử lý logic Backend chuyên sâu, quản lý người dùng và bảo mật thông tin bằng các tiêu chuẩn hiện đại.

🚀 Tính năng nổi bật
Quản lý người dùng: Đăng ký tài khoản với quy trình kiểm tra dữ liệu đầu vào.

Bảo mật mật khẩu: Sử dụng thuật toán BCrypt để băm (hash) mật khẩu, chống tấn công Brute-force và Rainbow table.

Kiến trúc phân lớp (Layered Architecture): Phân tách rõ ràng giữa Entities, DTOs, Controllers và Data Access.

OpenAPI/Swagger: Tích hợp tài liệu API trực quan, cho phép dùng thử các Endpoint trực tiếp trên trình duyệt.

🛠️ Công nghệ sử dụng
Framework: .NET 10 (ASP.NET Core Web API)

Database: Microsoft SQL Server

ORM: Entity Framework Core (Code First)

Security: BCrypt.Net-Next

API Documentation: Swashbuckle (Swagger)

🏗️ Cấu trúc thư mục
Plaintext
AuthServer/
├── Controllers/    # Xử lý các HTTP Request (Login, Register)
├── Data/           # Cấu hình DbContext và Migrations
├── DTOs/           # Data Transfer Objects (Hứng dữ liệu từ Client)
├── Entities/       # Các thực thể Database (User, Role,...)
├── Properties/     # Cấu hình môi trường (launchSettings.json)
└── appsettings.json # Cấu hình Connection String & Secret Key
⚙️ Cấu hình và Cài đặt
Clone dự án:

Bash
git clone https://github.com/your-username/AuthServer.git
cd AuthServer
Cấu hình Database:
Mở file appsettings.json và cập nhật Connection String phù hợp với máy của bạn:

JSON
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=AuthServerDb;Trusted_Connection=True;"
}
Cập nhật Database (Migrations):

PowerShell
dotnet ef database update
Chạy ứng dụng:

PowerShell
dotnet run
Sau khi chạy, truy cập: https://localhost:7166/swagger để xem giao diện API.
