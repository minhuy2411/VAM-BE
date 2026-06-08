using VAM.DTOs;
namespace VAM.Services
{
    public interface IOrderItemService : IServiceBase<OrderItemDto, CreateOrderItemDto, UpdateOrderItemDto> { }
}