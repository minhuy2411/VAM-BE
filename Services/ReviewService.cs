using AutoMapper;
using VAM.DTOs;
using VAM.Entities;
using VAM.Repositories;

namespace VAM.Services
{
    public class ReviewService : ServiceBase<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>, IReviewService
    {
        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper) 
            : base(unitOfWork, unitOfWork.Reviews, mapper)
        {
        }
    }
}