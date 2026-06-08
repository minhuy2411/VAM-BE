using AutoMapper;
using VAM.DTOs;
using VAM.Entities;
using VAM.Repositories;

namespace VAM.Services
{
    public class PaymentService : ServiceBase<Payment, PaymentDto, CreatePaymentDto, UpdatePaymentDto>, IPaymentService
    {
        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper) 
            : base(unitOfWork, unitOfWork.Payments, mapper)
        {
        }
    }
}