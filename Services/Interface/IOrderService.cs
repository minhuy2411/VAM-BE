using System.Threading.Tasks;
using VAM.DTOs;

namespace VAM.Services
{
    public interface IOrderService : IServiceBase<OrderDto, CreateOrderDto, UpdateOrderDto>
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task UpdateOrderStatusAsync(int orderId, int userId, string userRole, UpdateOrderStatusDto dto);
        Task<PaginatedResult<OrderDto>> GetOrdersByBuyerAsync(int buyerId, int pageNumber, int pageSize);
        Task<PaginatedResult<OrderDto>> GetOrdersBySellerAsync(int sellerId, int pageNumber, int pageSize);
    }
}