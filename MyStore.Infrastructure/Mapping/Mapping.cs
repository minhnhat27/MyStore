using AutoMapper;
using MyStore.Application.DTOs;
using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Entities;

namespace MyStore.Infrastructure.Mapping
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Material, MaterialDTO>().ReverseMap();
            CreateMap<Brand, BrandDTO>().ReverseMap();
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Size, SizeDTO>().ReverseMap();

            //user
            CreateMap<User, UserResponse>();
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<User, UserInfo>();
            CreateMap<DeliveryAddress, AddressDTO>().ReverseMap();

            //order
            CreateMap<Order, OrderDTO>().ReverseMap();
            CreateMap<OrderDetail, ProductOrderDetail>();

            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.ProductOrderDetail, opt => opt.MapFrom(src => src.OrderDetails.FirstOrDefault()));

            CreateMap<OrderRequest, Order>().ReverseMap();
            //--details--
            CreateMap<Order, OrderDetailsResponse>()
                .ForMember(dest => dest.ProductOrderDetails, opt => opt.MapFrom(src => src.OrderDetails));

            //product
            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Images.FirstOrDefault() != null ? src.Images.FirstOrDefault()!.ImageUrl : null))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => Math.Round(src.Rating, 1)));

            CreateMap<ProductRequest, Product>();

            CreateMap<ProductColor, ColorSizeResponse>()
                .ForMember(dest => dest.SizeInStocks, opt => opt.MapFrom(src => src.ProductSizes));

            CreateMap<ProductSize, SizeInStock>();
            CreateMap<Product, ProductDetailsResponse>()
                .ForMember(dest => dest.MaterialIds, opt => opt.MapFrom(src => src.Materials.Select(e => e.MaterialId)))
                .ForMember(dest => dest.ColorSizes, opt => opt.MapFrom(src => src.ProductColors))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => Math.Round(src.Rating, 1)));


            //Voucher
            CreateMap<Voucher, VoucherDTO>().ReverseMap();

            //Payment Method
            CreateMap<PaymentMethod, PaymentMethodDTO>().ReverseMap();

            //review
            CreateMap<ProductReview, ReviewDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Fullname : null));
        }
    }
}
