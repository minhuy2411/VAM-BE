using AutoMapper;
using VAM.DTOs;
using VAM.Entities;
using VAM.Repositories;

namespace VAM.Services
{
    public class OrderService : ServiceBase<Order, OrderDto, CreateOrderDto, UpdateOrderDto>, IOrderService
    {
        public OrderService(IUnitOfWork unitOfWork, IMapper mapper) 
            : base(unitOfWork, unitOfWork.Orders, mapper)
        {
        }
    }
}