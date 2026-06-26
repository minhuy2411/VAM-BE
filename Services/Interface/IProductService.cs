using VAM.DTOs;
namespace VAM.Services
{
    public interface IProductService : IServiceBase<ProductDto, CreateProductDto, UpdateProductDto> { }
}