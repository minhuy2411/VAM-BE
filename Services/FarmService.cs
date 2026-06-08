using AutoMapper;
using VAM.DTOs;
using VAM.Entities;
using VAM.Repositories;

namespace VAM.Services
{
    public class FarmService : ServiceBase<Farm, FarmDto, CreateFarmDto, UpdateFarmDto>, IFarmService
    {
        public FarmService(IUnitOfWork unitOfWork, IMapper mapper) 
            : base(unitOfWork, unitOfWork.Farms, mapper)
        {
        }
    }
}