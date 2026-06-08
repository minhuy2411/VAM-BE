using AutoMapper;
using VAM.DTOs;
using VAM.Entities;
using VAM.Repositories;

namespace VAM.Services
{
    public class ProductService : ServiceBase<Product, ProductDto, CreateProductDto, UpdateProductDto>, IProductService
    {
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper) 
            : base(unitOfWork, unitOfWork.Products, mapper)
        {
        }
    }
}