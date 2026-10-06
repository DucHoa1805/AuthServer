# AuthServer — Bruno API Tests

Bộ test API cho AuthServer, dùng [Bruno](https://www.usebruno.com/).

## Cấu trúc

- `01–17*.bru` — 17 request test theo thứ tự một kịch bản hoàn chỉnh:
  đăng ký → validation → trùng lặp → login → profile → đổi mật khẩu
  (token cũ bị thu hồi) → xoá tài khoản (soft delete) → đăng ký lại.
- `environments/Local.bru` — env chạy local (không commit, chứa biến runtime do Bruno ghi lại sau mỗi lần chạy).

## Chuẩn bị lần đầu

```bash
npm install -g @usebruno/cli          # cài Bruno CLI
cp bruno/environments/Local.example.bru bruno/environments/Local.bru
```

## Chạy

```bash
# 1. Khởi động AuthServer (cần Jwt:Key trong user secrets + SQL Server)
dotnet run --project AuthServer/AuthServer.csproj

# 2. Chạy toàn bộ collection (terminal khác)
cd bruno
bru run --env Local
```

Kết quả pass: `18 (18 Passed)`. Mỗi request có test script assert status code
và nội dung response; biến `token`/`newToken` được truyền tự động giữa các bước.
