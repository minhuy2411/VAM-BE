using System.Threading.Tasks;
using VAM.DTOs;

namespace VAM.Services
{
    public interface IProductService : IServiceBase<ProductDto, CreateProductDto, UpdateProductDto>
    {
        Task<PaginatedResult<ProductDto>> GetFilteredAsync(ProductFilterDto filter);
        Task<ProductDto> CreateProductWithImagesAsync(CreateProductDto dto);
        Task UpdateProductWithImagesAsync(UpdateProductDto dto);
        Task ApproveProductAsync(int id, ApproveProductDto dto);
    }
}