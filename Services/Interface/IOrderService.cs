using VAM.DTOs;
namespace VAM.Services
{
    public interface IOrderService : IServiceBase<OrderDto, CreateOrderDto, UpdateOrderDto> { }
}