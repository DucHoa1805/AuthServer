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

File chính: `AuthServer/appsettings.json`

### Connection String
Sửa `ConnectionStrings:DefaultConnection` theo máy của bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=AuthServerDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### JWT
`Jwt:Key` **không được commit lên git**. Dùng user secrets (chạy trong thư mục `AuthServer/` chứa file `.csproj`):

```bash
cd AuthServer
dotnet user-secrets set "Jwt:Key" "your-very-strong-secret-key-at-least-32-characters"
```

Ứng dụng sẽ **từ chối khởi động** nếu `Jwt:Key` thiếu hoặc ngắn hơn 32 ký tự. `Issuer`/`Audience` đã có sẵn trong `appsettings.json`:

```json
"Jwt": {
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

Lưu ý: thông báo lỗi đăng nhập là chung ("Username hoặc mật khẩu không chính xác") cho cả trường hợp user không tồn tại, sai mật khẩu hay tài khoản bị vô hiệu hoá.

### GET `/api/auth/me` 🔒
Xem thông tin tài khoản hiện tại. Cần header `Authorization: Bearer <token>`.

Response mẫu:

```json
{
  "id": "3f2b8c64-1a2e-4c5d-9e8f-0a1b2c3d4e5f",
  "username": "testuser",
  "email": "test@example.com",
  "createdAt": "2026-04-20T12:00:00Z"
}
```

### POST `/api/auth/change-password` 🔒
Đổi mật khẩu. **Mọi token cũ sẽ hết hiệu lực ngay sau khi đổi** — phải đăng nhập lại.

Body mẫu:

```json
{
  "currentPassword": "Abcdef@123",
  "newPassword": "Xyzuvw@456",
  "confirmPassword": "Xyzuvw@456"
}
```

### DELETE `/api/auth/me` 🔒
Xoá tài khoản (soft delete — vô hiệu hoá, có thể khôi phục ở tầng database). Cần mật khẩu xác nhận trong body. Sau khi xoá, mọi token hết hiệu lực và username/email có thể được đăng ký lại.

Body mẫu:

```json
{
  "password": "Abcdef@123"
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

🔒 = cần JWT token (đăng nhập rồi lấy token từ `/api/auth/login`)
