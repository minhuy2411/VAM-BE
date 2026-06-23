# API Endpoints

Base URL: `http://localhost:10000/api`

> **Scalar API Reference:** Truy cập `http://localhost:10000/scalar/v1` (hoặc `/scalar`) để xem tài liệu API và test trực tiếp bằng giao diện Scalar UI.

## Tổng quan

Tất cả controllers đều theo chuẩn RESTful với pattern `api/[controller]`.

| Resource | Base Route | Các Phương Thức Hỗ Trợ |
|---|---|---|
| Auth | `/api/Auth` | POST (login, google-login, register, forgot-password, reset-password, change-password, logout) |
| Profiles | `/api/Profiles` | POST (seller, business), GET (seller/me, business/me) |
| Admin Profiles | `/api/admin/AdminProfiles` | GET (seller/pending, business/pending), PUT (seller/:id/approve, business/:id/approve) |
| Users | `/api/Users` | GET, GET/:id, POST, PUT, DELETE/:id |
| Categories | `/api/Categorys` | GET, GET/:id, POST, PUT, DELETE/:id |
| Farms | `/api/Farms` | GET, GET/:id, POST, PUT, DELETE/:id |
| Products | `/api/Products` | GET, GET/:id, POST, PUT, DELETE/:id |
| Orders | `/api/Orders` | GET, GET/:id, POST, PUT, DELETE/:id |
| OrderItems | `/api/OrderItems` | GET, GET/:id, POST, PUT, DELETE/:id |
| Payments | `/api/Payments` | GET, GET/:id, POST, PUT, DELETE/:id |
| Reviews | `/api/Reviews` | GET, GET/:id, POST, PUT, DELETE/:id |

---

## Chi tiết Endpoints

### 1. Authentication (Xác thực) - `/api/Auth`

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| POST | `/api/Auth/login` | Đăng nhập hệ thống | `LoginDto` | `200 OK` + Token |
| POST | `/api/Auth/google-login` | Đăng nhập bằng Google | `GoogleLoginDto` | `200 OK` + Token |
| POST | `/api/Auth/register` | Đăng ký tài khoản mới | `RegisterDto` | `200 OK` + `UserDto` |
| POST | `/api/Auth/forgot-password` | Yêu cầu quên mật khẩu | `ForgotPasswordDto` | `200 OK` + Message |
| POST | `/api/Auth/reset-password` | Đặt lại mật khẩu mới | `ResetPasswordDto` | `200 OK` + Message |
| POST | `/api/Auth/change-password` | Đổi mật khẩu (Yêu cầu Token) | `ChangePasswordDto` | `200 OK` + Message |
| POST | `/api/Auth/logout` | Đăng xuất (Yêu cầu Token) | — | `200 OK` + Message |

### 2. User Profiles (Hồ sơ người dùng) - `/api/Profiles`

*Yêu cầu Token xác thực trong Header (`Authorization: Bearer <token>`)*

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| POST | `/api/Profiles/seller` | Tạo hồ sơ người bán (Seller) | `CreateSellerProfileDto` | `200 OK` + Profile |
| POST | `/api/Profiles/business` | Tạo hồ sơ doanh nghiệp (Business) | `CreateBusinessProfileDto` | `200 OK` + Profile |
| GET | `/api/Profiles/seller/me` | Lấy hồ sơ Seller của tôi | — | `200 OK` + Profile / `404` |
| GET | `/api/Profiles/business/me` | Lấy hồ sơ Business của tôi | — | `200 OK` + Profile / `404` |

### 3. Admin Profile Management - `/api/admin/AdminProfiles`

*Yêu cầu Token xác thực của Admin (`Roles = "admin"`)*

| Method | Endpoint | Mô tả | Query Params / Body | Response |
|---|---|---|---|---|
| GET | `/api/admin/AdminProfiles/seller/pending` | Danh sách hồ sơ Seller chờ duyệt | `page`, `pageSize` | `200 OK` + Phân trang |
| GET | `/api/admin/AdminProfiles/business/pending`| Danh sách hồ sơ Business chờ duyệt| `page`, `pageSize` | `200 OK` + Phân trang |
| PUT | `/api/admin/AdminProfiles/seller/{id}/approve` | Phê duyệt/Từ chối hồ sơ Seller | `ApproveProfileDto` | `200 OK` + Message |
| PUT | `/api/admin/AdminProfiles/business/{id}/approve`| Phê duyệt/Từ chối hồ sơ Business| `ApproveProfileDto` | `200 OK` + Message |

### 4. Users - `/api/Users`

| Method | Endpoint | Mô tả | Request Body / Query Params | Response |
|---|---|---|---|---|
| GET | `/api/Users` | Lấy danh sách users | `pageNumber`, `pageSize`, `search` | `200 OK` + Phân trang |
| GET | `/api/Users/{id}` | Lấy chi tiết user | — | `200 OK` + `UserDto` |
| POST | `/api/Users` | Tạo user mới | `CreateUserDto` | `200 OK` + `UserDto` |
| PUT | `/api/Users` | Cập nhật thông tin user | `UpdateUserDto` (chứa ID bên trong) | `204 No Content` |
| DELETE | `/api/Users/{id}` | Xóa user (Soft delete) | — | `204 No Content` |

### 5. Categories - `/api/Categorys`

| Method | Endpoint | Mô tả | Request Body / Query Params | Response |
|---|---|---|---|---|
| GET | `/api/Categorys` | Lấy danh sách categories | `pageNumber`, `pageSize`, `search` | `200 OK` + Phân trang |
| GET | `/api/Categorys/{id}` | Lấy chi tiết category | — | `200 OK` + `CategoryDto` |
| POST | `/api/Categorys` | Tạo category mới | `CreateCategoryDto` | `200 OK` + `CategoryDto` |
| PUT | `/api/Categorys` | Cập nhật category | `UpdateCategoryDto` | `204 No Content` |
| DELETE | `/api/Categorys/{id}` | Xóa category | — | `204 No Content` |

### 6. Farms - `/api/Farms`

| Method | Endpoint | Mô tả | Request Body / Query Params | Response |
|---|---|---|---|---|
| GET | `/api/Farms` | Lấy danh sách các trang trại | `pageNumber`, `pageSize`, `search` | `200 OK` + Phân trang |
| GET | `/api/Farms/{id}` | Lấy chi tiết trang trại | — | `200 OK` + `FarmDto` |
| POST | `/api/Farms` | Tạo trang trại mới | `CreateFarmDto` | `200 OK` + `FarmDto` |
| PUT | `/api/Farms` | Cập nhật trang trại | `UpdateFarmDto` | `204 No Content` |
| DELETE | `/api/Farms/{id}` | Xóa trang trại | — | `204 No Content` |

### 7. Products - `/api/Products`

| Method | Endpoint | Mô tả | Request Body / Query Params | Response |
|---|---|---|---|---|
| GET | `/api/Products` | Lấy danh sách sản phẩm | `pageNumber`, `pageSize`, `search` | `200 OK` + Phân trang |
| GET | `/api/Products/{id}` | Lấy chi tiết sản phẩm | — | `200 OK` + `ProductDto` |
| POST | `/api/Products` | Tạo sản phẩm mới | `CreateProductDto` | `200 OK` + `ProductDto` |
| PUT | `/api/Products` | Cập nhật sản phẩm | `UpdateProductDto` | `204 No Content` |
| DELETE | `/api/Products/{id}` | Xóa sản phẩm | — | `204 No Content` |

### 8. Orders - `/api/Orders`

| Method | Endpoint | Mô tả | Request Body / Query Params | Response |
|---|---|---|---|---|
| GET | `/api/Orders` | Lấy danh sách đơn hàng | `pageNumber`, `pageSize`, `search` | `200 OK` + Phân trang |
| GET | `/api/Orders/{id}` | Lấy chi tiết đơn hàng | — | `200 OK` + `OrderDto` |
| POST | `/api/Orders` | Tạo đơn hàng mới | `CreateOrderDto` | `200 OK` + `OrderDto` |
| PUT | `/api/Orders` | Cập nhật đơn hàng | `UpdateOrderDto` | `204 No Content` |
| DELETE | `/api/Orders/{id}` | Xóa đơn hàng | — | `204 No Content` |

### 9. OrderItems - `/api/OrderItems`

| Method | Endpoint | Mô tả | Request Body / Query Params | Response |
|---|---|---|---|---|
| GET | `/api/OrderItems` | Lấy danh sách chi tiết đơn hàng | `pageNumber`, `pageSize`, `search` | `200 OK` + Phân trang |
| GET | `/api/OrderItems/{id}` | Lấy chi tiết dòng đơn hàng | — | `200 OK` + `OrderItemDto` |
| POST | `/api/OrderItems` | Tạo mới dòng đơn hàng | `CreateOrderItemDto` | `200 OK` + `OrderItemDto` |
| PUT | `/api/OrderItems` | Cập nhật dòng đơn hàng | `UpdateOrderItemDto` | `204 No Content` |
| DELETE | `/api/OrderItems/{id}` | Xóa dòng đơn hàng | — | `204 No Content` |

### 10. Payments - `/api/Payments`

| Method | Endpoint | Mô tả | Request Body / Query Params | Response |
|---|---|---|---|---|
| GET | `/api/Payments` | Lấy danh sách thanh toán | `pageNumber`, `pageSize`, `search` | `200 OK` + Phân trang |
| GET | `/api/Payments/{id}` | Lấy chi tiết thanh toán | — | `200 OK` + `PaymentDto` |
| POST | `/api/Payments` | Tạo thanh toán mới | `CreatePaymentDto` | `200 OK` + `PaymentDto` |
| PUT | `/api/Payments` | Cập nhật thanh toán | `UpdatePaymentDto` | `204 No Content` |
| DELETE | `/api/Payments/{id}` | Xóa thanh toán | — | `204 No Content` |

### 11. Reviews - `/api/Reviews`

| Method | Endpoint | Mô tả | Request Body / Query Params | Response |
|---|---|---|---|---|
| GET | `/api/Reviews` | Lấy danh sách đánh giá | `pageNumber`, `pageSize`, `search` | `200 OK` + Phân trang |
| GET | `/api/Reviews/{id}` | Lấy chi tiết đánh giá | — | `200 OK` + `ReviewDto` |
| POST | `/api/Reviews` | Tạo đánh giá mới | `CreateReviewDto` | `200 OK` + `ReviewDto` |
| PUT | `/api/Reviews` | Cập nhật đánh giá | `UpdateReviewDto` | `204 No Content` |
| DELETE | `/api/Reviews/{id}` | Xóa đánh giá | — | `204 No Content` |

---

## Ghi chú Quan trọng

- **Soft Delete:** Tất cả thao tác `DELETE` đều thực hiện soft delete (set `IsDeleted = true`), bản ghi đã xóa tự động được lọc ở tầng Database nhờ Global Query Filter.
- **Phân trang & Tìm kiếm:** Các API `GET` lấy danh sách đều hỗ trợ các query parameters mặc định:
  - `pageNumber` (mặc định: `1`)
  - `pageSize` (mặc định: `10`)
  - `search` (tìm kiếm theo từ khóa)
- **PUT Endpoints:** Nhận DTO trực tiếp trong Request Body (chứa cả ID của đối tượng cần cập nhật), không truyền ID lên URL parameter.
