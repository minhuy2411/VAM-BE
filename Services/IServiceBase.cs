using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAM.Services
{
    public interface IServiceBase<TDto, TCreateDto, TUpdateDto>
    {
        Task<VAM.DTOs.PaginatedResult<TDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string search = null);
        Task<TDto?> GetByIdAsync(int id);
        Task<TDto> CreateAsync(TCreateDto dto);
        Task UpdateAsync(TUpdateDto dto);
        Task DeleteAsync(int id);
    }
}