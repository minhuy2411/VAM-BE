# API Endpoints

Base URL: `http://localhost:5137/api` hoặc `https://localhost:7272/api`

> **Swagger UI:** Truy cập `http://localhost:5137/swagger` để test trực tiếp.

## Tổng quan

Tất cả controllers đều theo chuẩn RESTful với pattern `api/[controller]`.

| Resource | Base Route | CRUD Operations |
|---|---|---|
| Users | `/api/Users` | GET, GET/:id, POST, PUT, DELETE/:id |
| Categories | `/api/Categorys` | GET, GET/:id, POST, PUT, DELETE/:id |
| Farms | `/api/Farms` | GET, GET/:id, POST, PUT, DELETE/:id |
| Products | `/api/Products` | GET, GET/:id, POST, PUT, DELETE/:id |
| Orders | `/api/Orders` | GET, GET/:id, POST, PUT, DELETE/:id |
| OrderItems | `/api/OrderItems` | GET, GET/:id, POST, PUT, DELETE/:id |
| Payments | `/api/Payments` | GET, GET/:id, POST, PUT, DELETE/:id |
| Reviews | `/api/Reviews` | GET, GET/:id, POST, PUT, DELETE/:id |

## Chi tiết Endpoints

### Users

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| GET | `/api/Users` | Lấy tất cả users | — | `200 OK` + `UserDto[]` |
| GET | `/api/Users/{id}` | Lấy user theo ID | — | `200 OK` + `UserDto` / `404` |
| POST | `/api/Users` | Tạo user mới | `CreateUserDto` | `200 OK` + `UserDto` |
| PUT | `/api/Users` | Cập nhật user | `UpdateUserDto` | `204 No Content` |
| DELETE | `/api/Users/{id}` | Xóa user (soft delete) | — | `204 No Content` |

### Categories

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| GET | `/api/Categorys` | Lấy tất cả categories | — | `200 OK` + `CategoryDto[]` |
| GET | `/api/Categorys/{id}` | Lấy category theo ID | — | `200 OK` + `CategoryDto` / `404` |
| POST | `/api/Categorys` | Tạo category mới | `CreateCategoryDto` | `200 OK` + `CategoryDto` |
| PUT | `/api/Categorys` | Cập nhật category | `UpdateCategoryDto` | `204 No Content` |
| DELETE | `/api/Categorys/{id}` | Xóa category | — | `204 No Content` |

### Farms

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| GET | `/api/Farms` | Lấy tất cả farms | — | `200 OK` + `FarmDto[]` |
| GET | `/api/Farms/{id}` | Lấy farm theo ID | — | `200 OK` + `FarmDto` / `404` |
| POST | `/api/Farms` | Tạo farm mới | `CreateFarmDto` | `200 OK` + `FarmDto` |
| PUT | `/api/Farms` | Cập nhật farm | `UpdateFarmDto` | `204 No Content` |
| DELETE | `/api/Farms/{id}` | Xóa farm | — | `204 No Content` |

### Products

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| GET | `/api/Products` | Lấy tất cả products | — | `200 OK` + `ProductDto[]` |
| GET | `/api/Products/{id}` | Lấy product theo ID | — | `200 OK` + `ProductDto` / `404` |
| POST | `/api/Products` | Tạo product mới | `CreateProductDto` | `200 OK` + `ProductDto` |
| PUT | `/api/Products` | Cập nhật product | `UpdateProductDto` | `204 No Content` |
| DELETE | `/api/Products/{id}` | Xóa product | — | `204 No Content` |

### Orders

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| GET | `/api/Orders` | Lấy tất cả orders | — | `200 OK` + `OrderDto[]` |
| GET | `/api/Orders/{id}` | Lấy order theo ID | — | `200 OK` + `OrderDto` / `404` |
| POST | `/api/Orders` | Tạo order mới | `CreateOrderDto` | `200 OK` + `OrderDto` |
| PUT | `/api/Orders` | Cập nhật order | `UpdateOrderDto` | `204 No Content` |
| DELETE | `/api/Orders/{id}` | Xóa order | — | `204 No Content` |

### OrderItems

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| GET | `/api/OrderItems` | Lấy tất cả order items | — | `200 OK` + `OrderItemDto[]` |
| GET | `/api/OrderItems/{id}` | Lấy order item theo ID | — | `200 OK` + `OrderItemDto` / `404` |
| POST | `/api/OrderItems` | Tạo order item mới | `CreateOrderItemDto` | `200 OK` + `OrderItemDto` |
| PUT | `/api/OrderItems` | Cập nhật order item | `UpdateOrderItemDto` | `204 No Content` |
| DELETE | `/api/OrderItems/{id}` | Xóa order item | — | `204 No Content` |

### Payments

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| GET | `/api/Payments` | Lấy tất cả payments | — | `200 OK` + `PaymentDto[]` |
| GET | `/api/Payments/{id}` | Lấy payment theo ID | — | `200 OK` + `PaymentDto` / `404` |
| POST | `/api/Payments` | Tạo payment mới | `CreatePaymentDto` | `200 OK` + `PaymentDto` |
| PUT | `/api/Payments` | Cập nhật payment | `UpdatePaymentDto` | `204 No Content` |
| DELETE | `/api/Payments/{id}` | Xóa payment | — | `204 No Content` |

### Reviews

| Method | Endpoint | Mô tả | Request Body | Response |
|---|---|---|---|---|
| GET | `/api/Reviews` | Lấy tất cả reviews | — | `200 OK` + `ReviewDto[]` |
| GET | `/api/Reviews/{id}` | Lấy review theo ID | — | `200 OK` + `ReviewDto` / `404` |
| POST | `/api/Reviews` | Tạo review mới | `CreateReviewDto` | `200 OK` + `ReviewDto` |
| PUT | `/api/Reviews` | Cập nhật review | `UpdateReviewDto` | `204 No Content` |
| DELETE | `/api/Reviews/{id}` | Xóa review | — | `204 No Content` |

## Ghi chú

- Tất cả DELETE operations thực hiện **soft delete** (set `IsDeleted = true`)
- Các bản ghi đã xóa sẽ tự động bị lọc bởi **Global Query Filter**
- PUT endpoints nhận DTO trong request body (không qua URL parameter)
