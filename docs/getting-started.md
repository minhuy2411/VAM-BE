# Hướng dẫn chạy dự án VAM

## Yêu cầu hệ thống

| Phần mềm       | Phiên bản                            |
| -------------- | ------------------------------------ |
| .NET SDK       | 10.0+                                |
| PostgreSQL     | 14+                                  |
| IDE (tùy chọn) | Visual Studio 2022 / VS Code / Rider |

## 1. Clone & Restore packages

```bash
# Clone project (nếu chưa có)
git clone <repo-url>
cd VAM

# Restore NuGet packages
dotnet restore
```

## 2. Cấu hình Database

Mở file `appsettings.json` và cập nhật connection string PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=VAMDb;Username=postgres;Password=123456"
  }
}
```

> **Lưu ý:** Đảm bảo PostgreSQL đang chạy và user/password khớp với cấu hình.

## 3. Chạy Migration

### Cài đặt EF Core Tools (nếu chưa có)

```bash
dotnet tool install --global dotnet-ef
```

### Tạo Migration (lần đầu hoặc khi thay đổi Entities)

```bash
dotnet ef migrations add InitialCreate
```

### Cập nhật Database

```bash
dotnet ef database update
```

### Các lệnh Migration hữu ích khác

```bash
# Xem danh sách migrations
dotnet ef migrations list

# Rollback migration cuối
dotnet ef migrations remove

# Tạo SQL script từ migrations (cho production)
dotnet ef migrations script -o migration.sql

# Reset database (xóa & tạo lại)
dotnet ef database drop --force
dotnet ef database update
```

## 4. Cấu hình PayOS (tùy chọn)

Cập nhật thông tin PayOS trong `appsettings.json`:

```json
{
  "PayOS": {
    "ClientId": "YOUR_PAYOS_CLIENT_ID",
    "ApiKey": "YOUR_PAYOS_API_KEY",
    "ChecksumKey": "YOUR_PAYOS_CHECKSUM_KEY"
  }
}
```

## 5. Build Project

```bash
dotnet build
```

## 6. Chạy Project

```bash
# Development mode (hot reload)
dotnet watch run

# Hoặc chạy bình thường
dotnet run
```

Sau khi chạy, project sẽ khởi động tại:

| Protocol | URL                      |
| -------- | ------------------------ |
| HTTP     | `http://localhost:5137`  |
| HTTPS    | `https://localhost:7272` |

## 7. Truy cập Swagger UI

Mở trình duyệt và truy cập:

```
http://localhost:10000/scalar/v1
```

Tại đây bạn có thể:

- Xem danh sách tất cả API endpoints
- Test trực tiếp từng API endpoint
- Xem request/response schema

## 8. Các lệnh thường dùng

| Lệnh                              | Mô tả                       |
| --------------------------------- | --------------------------- |
| `dotnet restore`                  | Restore NuGet packages      |
| `dotnet build`                    | Build project               |
| `dotnet run`                      | Chạy project                |
| `dotnet watch run`                | Chạy project với hot reload |
| `dotnet ef migrations add <Name>` | Tạo migration mới           |
| `dotnet ef database update`       | Áp dụng migrations vào DB   |
| `dotnet ef migrations list`       | Xem danh sách migrations    |
| `dotnet ef database drop --force` | Xóa database                |
| `dotnet clean`                    | Xóa build artifacts         |
| `dotnet publish -c Release`       | Build production            |

## Xử lý lỗi thường gặp

### Lỗi kết nối PostgreSQL

```
Npgsql.NpgsqlException: Failed to connect to ...
```

**Giải pháp:** Kiểm tra PostgreSQL đang chạy, port, username/password đúng.

### Lỗi Migration

```
The entity type 'X' requires a primary key to be defined.
```

**Giải pháp:** Đảm bảo Entity có `[Key]` attribute hoặc property tên `Id`.

### Lỗi EF Tools chưa cài

```
Could not execute because the specified command or file was not found.
```

**Giải pháp:** Chạy `dotnet tool install --global dotnet-ef`
