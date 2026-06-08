# Tổng quan Codebase

## Kiến trúc tổng thể

Project VAM sử dụng kiến trúc **N-Layer** (Clean Architecture đơn giản):

```
Controller → Service → Repository → Database (EF Core → PostgreSQL)
```

Mô hình **Unit of Work + Repository Pattern** được áp dụng để quản lý data access.

## Cấu trúc thư mục

```
VAM/
├── Controllers/           # API Controllers (nhận request, trả response)
│   ├── UsersController.cs
│   ├── CategorysController.cs
│   ├── FarmsController.cs
│   ├── ProductsController.cs
│   ├── OrdersController.cs
│   ├── OrderItemsController.cs
│   ├── PaymentsController.cs
│   └── ReviewsController.cs
│
├── DTOs/                  # Data Transfer Objects (request/response models)
│   ├── UserDto.cs         # UserDto, CreateUserDto, UpdateUserDto
│   ├── CategoryDto.cs
│   ├── FarmDto.cs
│   ├── ProductDto.cs
│   ├── OrderDto.cs
│   ├── OrderItemDto.cs
│   ├── PaymentDto.cs
│   └── ReviewDto.cs
│
├── Entities/              # Database Entities (ánh xạ với bảng DB)
│   ├── BaseEntity.cs      # Abstract base: IsDeleted, CreatedAt, UpdatedAt, etc.
│   ├── User.cs
│   ├── Category.cs
│   ├── Farm.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Payment.cs
│   └── Review.cs
│
├── Data/                  # DbContext & Database configuration
│   └── ApplicationDbContext.cs
│
├── Repositories/          # Data Access Layer
│   ├── IRepositoryBase.cs # Generic repository interface
│   ├── RepositoryBase.cs  # Generic repository implementation
│   ├── IUnitOfWork.cs     # Unit of Work interface
│   └── UnitOfWork.cs      # Unit of Work implementation
│
├── Services/              # Business Logic Layer
│   ├── IServiceBase.cs    # Generic service interface
│   ├── ServiceBase.cs     # Generic service implementation
│   ├── IUserService.cs    # + UserService.cs
│   ├── ICategoryService.cs# + CategoryService.cs
│   ├── IFarmService.cs    # + FarmService.cs
│   ├── IProductService.cs # + ProductService.cs
│   ├── IOrderService.cs   # + OrderService.cs
│   ├── IOrderItemService.cs # + OrderItemService.cs
│   ├── IPaymentService.cs # + PaymentService.cs
│   └── IReviewService.cs  # + ReviewService.cs
│
├── Profiles/              # AutoMapper configuration
│   └── AppProfile.cs      # Entity ↔ DTO mapping profiles
│
├── Properties/
│   └── launchSettings.json
│
├── docs/                  # Tài liệu dự án
├── Program.cs             # Application entry point & DI configuration
├── VAM.csproj             # Project file & NuGet dependencies
├── appsettings.json       # Configuration (DB, PayOS, etc.)
└── appsettings.Development.json
```

## Luồng xử lý Request

```
HTTP Request
    ↓
[Controller]     Nhận request, validate input cơ bản
    ↓
[Service]        Xử lý business logic, gọi Repository
    ↓
[Repository]     Truy vấn database qua EF Core DbSet
    ↓
[UnitOfWork]     Quản lý transaction, gọi SaveChangesAsync()
    ↓
[Database]       PostgreSQL
    ↓
[AutoMapper]     Entity → DTO (response)
    ↓
HTTP Response
```

## Các Design Patterns được sử dụng

### 1. Repository Pattern

- **Interface:** `IRepositoryBase<T>` — định nghĩa CRUD operations
- **Implementation:** `RepositoryBase<T>` — sử dụng EF Core `DbSet<T>`
- Hỗ trợ: GetAll, GetById, GetPaginated, Find, Create, Update, Delete (soft delete)

### 2. Unit of Work Pattern

- **Interface:** `IUnitOfWork` — tập hợp tất cả repositories + transaction management
- **Implementation:** `UnitOfWork` — wrap `ApplicationDbContext`, quản lý `SaveChangesAsync()`
- Hỗ trợ: `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()`

### 3. Generic Service Pattern

- **Interface:** `IServiceBase<TDto, TCreateDto, TUpdateDto>`
- **Implementation:** `ServiceBase<TEntity, TDto, TCreateDto, TUpdateDto>`
- Cung cấp CRUD operations với AutoMapper tự động chuyển đổi Entity ↔ DTO

### 4. DTO Pattern (Data Transfer Object)

Mỗi entity có 3 DTOs:
- `XxxDto` — response DTO (trả về cho client)
- `CreateXxxDto` — request DTO cho tạo mới
- `UpdateXxxDto` — request DTO cho cập nhật

### 5. Soft Delete

- `BaseEntity.IsDeleted` — flag đánh dấu xóa mềm
- `RepositoryBase.Delete()` — set `IsDeleted = true` thay vì xóa vật lý
- **Global Query Filter** trong `ApplicationDbContext.OnModelCreating()` — tự động lọc bản ghi đã xóa

## Dependency Injection

Tất cả được đăng ký trong `Program.cs`:

```csharp
// Repository & UnitOfWork
builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
// ... (tất cả services)
```

- Sử dụng **Scoped lifetime** — mỗi HTTP request tạo 1 instance mới.

## AutoMapper Configuration

File `Profiles/AppProfile.cs` định nghĩa mapping:

```
Entity ↔ Dto         (ReverseMap)
CreateDto → Entity   (one-way)
UpdateDto → Entity   (one-way)
```

Áp dụng cho tất cả 8 entities: User, Category, Farm, Product, Order, OrderItem, Payment, Review.
