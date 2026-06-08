# VAM - Vietnamese Agricultural Marketplace

## Tổng quan dự án

**VAM** là một RESTful API xây dựng trên nền tảng **ASP.NET Core (.NET 10)**, phục vụ thị trường nông sản Việt Nam. Hệ thống cho phép:

- Quản lý người dùng (Buyer / Seller)
- Quản lý trang trại (Farm)
- Quản lý danh mục & sản phẩm nông sản
- Đặt hàng, thanh toán tích hợp **PayOS**
- Đánh giá sản phẩm

## Tech Stack

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (Npgsql) |
| Mapping | AutoMapper 16 |
| Payment | PayOS SDK 2.1 |
| API Docs | Swagger (Swashbuckle 10.2.1) + OpenAPI |

## Tài liệu

| File | Nội dung |
|---|---|
| [getting-started.md](./getting-started.md) | Hướng dẫn cài đặt, migrate, chạy project |
| [codebase-overview.md](./codebase-overview.md) | Tổng quan kiến trúc & cấu trúc code |
| [api-endpoints.md](./api-endpoints.md) | Danh sách API endpoints |
| [database-schema.md](./database-schema.md) | Sơ đồ database & entities |
