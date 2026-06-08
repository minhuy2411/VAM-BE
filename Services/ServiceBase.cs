using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAM.Repositories;

namespace VAM.Services
{
    public class ServiceBase<TEntity, TDto, TCreateDto, TUpdateDto> : IServiceBase<TDto, TCreateDto, TUpdateDto> 
        where TEntity : class
        where TDto : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IRepositoryBase<TEntity> _repository;
        protected readonly IMapper _mapper;

        public ServiceBase(IUnitOfWork unitOfWork, IRepositoryBase<TEntity> repository, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<VAM.DTOs.PaginatedResult<TDto>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string search = null)
        {
            var filter = BuildFilter(search);
            var (items, totalCount) = await _repository.GetAllPaginatedAsync(pageNumber, pageSize, filter);
            return new VAM.DTOs.PaginatedResult<TDto>
            {
                Items = _mapper.Map<IEnumerable<TDto>>(items),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        protected virtual System.Linq.Expressions.Expression<System.Func<TEntity, bool>> BuildFilter(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return null;

            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(TEntity), "x");
            var stringProperties = typeof(TEntity).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanRead);

            System.Linq.Expressions.Expression body = null;
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
            var searchConstant = System.Linq.Expressions.Expression.Constant(search.ToLower(), typeof(string));

            foreach (var property in stringProperties)
            {
                var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, property);
                var nullCheck = System.Linq.Expressions.Expression.NotEqual(propertyAccess, System.Linq.Expressions.Expression.Constant(null, typeof(string)));
                var toLowerCall = System.Linq.Expressions.Expression.Call(propertyAccess, toLowerMethod);
                var containsCall = System.Linq.Expressions.Expression.Call(toLowerCall, containsMethod, searchConstant);
                var condition = System.Linq.Expressions.Expression.AndAlso(nullCheck, containsCall);

                body = body == null ? condition : System.Linq.Expressions.Expression.OrElse(body, condition);
            }

            if (body == null)
                return null;

            return System.Linq.Expressions.Expression.Lambda<System.Func<TEntity, bool>>(body, parameter);
        }

        public async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<TDto>(entity);
        }

        public async Task<TDto> CreateAsync(TCreateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            await _repository.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<TDto>(entity);
        }

        public async Task UpdateAsync(TUpdateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            _repository.Update(entity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity != null)
            {
                _repository.Delete(entity);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}