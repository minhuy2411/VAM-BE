using VAM.DTOs;
namespace VAM.Services
{
    public interface ICategoryService : IServiceBase<CategoryDto, CreateCategoryDto, UpdateCategoryDto> { }
}