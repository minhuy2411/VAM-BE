using AutoMapper;
using VAM.Entities;
using VAM.DTOs;

namespace VAM.Profiles
{
    public class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            CreateMap<Farm, FarmDto>().ReverseMap();
            CreateMap<CreateFarmDto, Farm>();
            CreateMap<UpdateFarmDto, Farm>();

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ImageUrls, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.ImageUrls, opt => opt.Ignore());
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.BuyerName, opt => opt.MapFrom(src => src.Buyer != null ? src.Buyer.Name : null))
                .ReverseMap();
            CreateMap<CreateOrderDto, Order>();
            CreateMap<UpdateOrderDto, Order>();

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ProductImageUrls, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrls : null))
                .ReverseMap();
            CreateMap<CreateOrderItemDto, OrderItem>();
            CreateMap<UpdateOrderItemDto, OrderItem>();

            CreateMap<Payment, PaymentDto>().ReverseMap();
            CreateMap<CreatePaymentDto, Payment>();
            CreateMap<UpdatePaymentDto, Payment>();

            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<CreateReviewDto, Review>();
            CreateMap<UpdateReviewDto, Review>();

            CreateMap<SellerProfile, SellerProfileDto>().ReverseMap();
            CreateMap<CreateSellerProfileDto, SellerProfile>();
            
            CreateMap<BusinessProfile, BusinessProfileDto>().ReverseMap();
            CreateMap<CreateBusinessProfileDto, BusinessProfile>();

            CreateMap<PayoutTransaction, PayoutTransactionDto>();
        }
    }
}