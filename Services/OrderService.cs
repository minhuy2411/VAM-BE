using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAM.Data;
using VAM.DTOs;
using VAM.Entities;
using VAM.Exceptions;
using VAM.Repositories;

namespace VAM.Services
{
    public class OrderService : ServiceBase<Order, OrderDto, CreateOrderDto, UpdateOrderDto>, IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ApplicationDbContext context)
            : base(unitOfWork, unitOfWork.Orders, mapper)
        {
            _context = context;
        }

        /// <summary>
        /// Create order with stock validation, price calculation, and inventory deduction.
        /// TotalPrice is always calculated server-side from Product.Price to prevent manipulation.
        /// </summary>
        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            // 1. Validate non-empty items
            if (dto.OrderItems == null || !dto.OrderItems.Any())
                throw new AppException(ORDER_ERROR.ORDER_EMPTY_ITEMS);

            // 2. Lookup all products in one query
            var productIds = dto.OrderItems.Select(x => x.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync();

            // 3. Validate each item
            foreach (var item in dto.OrderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                    throw new AppException($"Product with ID {item.ProductId} not found", 404, "PRODUCT_NOT_FOUND");
                if (product.Status != "approved")
                    throw new AppException($"Product '{product.Name}' is not available for purchase", 400, "PRODUCT_NOT_AVAILABLE");
                if (product.Quantity < item.Quantity)
                    throw new AppException($"Insufficient stock for '{product.Name}'. Available: {product.Quantity}, Requested: {item.Quantity}", 400, "INSUFFICIENT_STOCK");
            }

            // 4. Begin transaction for atomicity
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    BuyerId = dto.BuyerId,
                    ShippingAddress = dto.ShippingAddress,
                    Status = "pending",
                    TotalPrice = 0,
                    OrderDate = DateTimeOffset.UtcNow
                };

                decimal totalPrice = 0;

                foreach (var item in dto.OrderItems)
                {
                    var product = products.First(p => p.Id == item.ProductId);

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = product.Price * item.Quantity // Snapshot price at order time
                    };
                    order.OrderItems.Add(orderItem);
                    totalPrice += orderItem.Price;

                    // 5. Deduct stock
                    product.Quantity -= item.Quantity;
                    if (product.Quantity <= 0)
                        product.Status = "out_of_stock";
                    _unitOfWork.Products.Update(product);
                }

                order.TotalPrice = totalPrice;

                await _unitOfWork.Orders.CreateAsync(order);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Reload with includes for response
                var createdOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.Buyer)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                return MapOrderToDto(createdOrder!);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// State machine for order status transitions with role-based permission checks.
        /// Valid transitions:
        ///   pending   -> confirmed (Seller)
        ///   pending   -> cancelled (Buyer or Seller)
        ///   confirmed -> shipping  (Seller)
        ///   confirmed -> cancelled (Buyer or Seller)
        ///   shipping  -> completed (Buyer)
        /// Cancellation restores inventory.
        /// </summary>
        public async Task UpdateOrderStatusAsync(int orderId, int userId, string userRole, UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null)
                throw new AppException(ORDER_ERROR.ORDER_NOT_FOUND);

            var newStatus = dto.Status.ToLower();
            var currentStatus = order.Status;

            // Validate permission based on role
            ValidateStatusChangePermission(order, userId, userRole, newStatus);

            // Validate status transition
            if (!IsValidTransition(currentStatus, newStatus))
                throw new AppException($"Cannot transition from '{currentStatus}' to '{newStatus}'", 400, "INVALID_STATUS_TRANSITION");

            // If cancelling, restore stock
            if (newStatus == "cancelled")
            {
                await RestoreStock(order.OrderItems);
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            order.UpdatedBy = userId;
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResult<OrderDto>> GetOrdersByBuyerAsync(int buyerId, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.BuyerId == buyerId && !o.IsDeleted)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Buyer)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<OrderDto>
            {
                Items = items.Select(MapOrderToDto),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResult<OrderDto>> GetOrdersBySellerAsync(int sellerId, int pageNumber, int pageSize)
        {
            // Find orders that contain at least one product from this seller
            var query = _context.Orders
                .Where(o => !o.IsDeleted && o.OrderItems.Any(oi => oi.Product != null && oi.Product.SellerId == sellerId))
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Buyer)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<OrderDto>
            {
                Items = items.Select(MapOrderToDto),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // ========== Private Helpers ==========

        private static bool IsValidTransition(string from, string to)
        {
            return (from, to) switch
            {
                ("pending", "confirmed") => true,
                ("pending", "cancelled") => true,
                ("confirmed", "shipping") => true,
                ("confirmed", "cancelled") => true,
                ("shipping", "completed") => true,
                _ => false
            };
        }

        private void ValidateStatusChangePermission(Order order, int userId, string userRole, string newStatus)
        {
            // Admin can do anything
            if (userRole == "admin") return;

            bool isBuyer = order.BuyerId == userId;
            bool isSeller = order.OrderItems.Any(oi => oi.Product != null && oi.Product.SellerId == userId);

            switch (newStatus)
            {
                case "confirmed":
                case "shipping":
                    // Only seller of the products can confirm/ship
                    if (!isSeller)
                        throw new AppException(ORDER_ERROR.ORDER_FORBIDDEN);
                    break;

                case "completed":
                    // Only buyer can mark as completed
                    if (!isBuyer)
                        throw new AppException(ORDER_ERROR.ORDER_FORBIDDEN);
                    break;

                case "cancelled":
                    // Buyer can cancel if pending or confirmed (not yet shipped)
                    // Seller can cancel if pending or confirmed
                    if (!isBuyer && !isSeller)
                        throw new AppException(ORDER_ERROR.ORDER_FORBIDDEN);
                    break;

                default:
                    throw new AppException(ORDER_ERROR.INVALID_STATUS_TRANSITION);
            }
        }

        private async Task RestoreStock(ICollection<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                if (item.Product == null) continue;

                var product = await _context.Products.FindAsync(item.Product.Id);
                if (product == null || product.IsDeleted) continue;

                product.Quantity += item.Quantity;
                if (product.Status == "out_of_stock" && product.Quantity > 0)
                    product.Status = "approved";
            }
        }

        private OrderDto MapOrderToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                BuyerId = order.BuyerId,
                BuyerName = order.Buyer?.Name,
                TotalPrice = order.TotalPrice,
                ShippingAddress = order.ShippingAddress,
                Status = order.Status,
                OrderDate = order.OrderDate,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    ProductImageUrls = oi.Product?.ImageUrls,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };
        }
    }
}