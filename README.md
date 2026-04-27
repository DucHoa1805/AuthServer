# AuthServer

AuthServer là Web API xác thực người dùng viết bằng **ASP.NET Core (.NET 10)**, dùng **EF Core + SQL Server**, hỗ trợ:
- Đăng ký tài khoản
- Đăng nhập và trả về JWT token
- Swagger để test API

## 1) Yêu cầu môi trường

- .NET SDK 10
- SQL Server (local hoặc remote)
- (Khuyến nghị) EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

## 2) Clone và vào thư mục dự án

```bash
git clone https://github.com/DucHoa1805/AuthServer.git
cd AuthServer
```

## 3) Cấu hình ứng dụng

File chính: `/home/runner/work/AuthServer/AuthServer/AuthServer/appsettings.json`

### Connection String
Sửa `ConnectionStrings:DefaultConnection` theo máy của bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=AuthServerDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### JWT
Điền `Jwt:Key` (không để rỗng). Ví dụ key đủ dài:

```json
"Jwt": {
  "Key": "your-very-strong-secret-key-at-least-32-characters",
  "Issuer": "AuthServer",
  "Audience": "AuthClient"
}
```

## 4) Khởi tạo/cập nhật database

```bash
dotnet ef database update --project AuthServer/AuthServer.csproj
```

## 5) Chạy ứng dụng

```bash
dotnet run --project AuthServer/AuthServer.csproj
```

Mặc định chạy ở:
- `http://localhost:5266`
- `https://localhost:7166`

Swagger:
- `http://localhost:5266/swagger`
- `https://localhost:7166/swagger`

## 6) API hiện có

Base route: `/api/auth`

### POST `/api/auth/register`
Đăng ký tài khoản mới.

Body mẫu:

```json
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Abcdef@123"
}
```

Ràng buộc:
- `username`: 3-20 ký tự
- `email`: đúng định dạng email
- `password`: tối thiểu 8 ký tự, có chữ hoa, chữ thường, số, ký tự đặc biệt (`@$!%*?&`)

### POST `/api/auth/login`
Đăng nhập và nhận JWT token.

Body mẫu:

```json
{
  "username": "testuser",
  "password": "Abcdef@123"
}
```

Response mẫu:

```json
{
  "token": "<jwt-token>",
  "username": "testuser"
}
```

## 7) Cấu trúc thư mục chính

```text
AuthServer/
├── Controllers/      # API endpoints
├── DTOs/             # Request models
├── Entities/         # Entity classes
├── Data/             # DbContext
├── Migrations/       # EF Core migrations
├── Properties/       # launchSettings
└── appsettings.json  # cấu hình ứng dụng
```
