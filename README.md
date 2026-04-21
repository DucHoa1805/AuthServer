# AuthServer (.NET 10 Web API)

AuthServer là API xác thực cơ bản xây bằng ASP.NET Core, Entity Framework Core và SQL Server.

## Tính năng hiện có
- Đăng ký tài khoản qua API `POST /api/auth/register`
- Hash mật khẩu bằng `BCrypt`
- Tích hợp Swagger để test API

## Công nghệ sử dụng
- .NET 10 (ASP.NET Core Web API)
- Entity Framework Core + SQL Server
- BCrypt.Net-Next
- Swashbuckle (Swagger/OpenAPI)

## Cấu trúc thư mục chính
```text
AuthServer/
├── Controllers/      # API endpoints
├── DTOs/             # Request/response models
├── Entities/         # Entity classes
├── Data/             # DbContext
├── Migrations/       # EF Core migrations
├── Properties/       # launchSettings
└── appsettings.json  # cấu hình ứng dụng + connection string
```

## Hướng dẫn chạy project (từng bước)

### 1) Yêu cầu môi trường
- Cài .NET SDK 10
- Cài SQL Server (local hoặc remote)
- (Khuyên dùng) Cài EF CLI:
```bash
dotnet tool install --global dotnet-ef
```

### 2) Clone repository
```bash
git clone https://github.com/DucHoa1805/AuthServer.git
cd AuthServer
```

### 3) Cấu hình database
Mở file:
- `AuthServer/appsettings.json`

Sửa `ConnectionStrings:DefaultConnection` cho phù hợp máy của bạn, ví dụ:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=AuthServerDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 4) Tạo/cập nhật database từ migration
Chạy tại thư mục gốc repository:
```bash
dotnet ef database update --project AuthServer/AuthServer.csproj
```

### 5) Chạy ứng dụng
```bash
dotnet run --project AuthServer/AuthServer.csproj
```

### 6) Test API bằng Swagger
Mở trình duyệt:
- https://localhost:7166/swagger
hoặc
- http://localhost:5266/swagger

## API hiện có
- `POST /api/auth/register`

Ví dụ body:
```json
{
  "username": "testuser",
  "password": "123456"
}
```
