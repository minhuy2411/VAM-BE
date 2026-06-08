using VAM.DTOs;
namespace VAM.Services
{
    public interface IPaymentService : IServiceBase<PaymentDto, CreatePaymentDto, UpdatePaymentDto> { }
}