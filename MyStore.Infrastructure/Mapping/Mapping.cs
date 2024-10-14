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
            CreateMap<User, UserInfo>().ReverseMap();
            CreateMap<DeliveryAddress, AddressDTO>().ReverseMap();

            //order
            CreateMap<OrderDTO, Order>().ReverseMap()
                .ForMember(d => d.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethodName));
            //.ForMember(d => d.OrderStatus, opt => opt.MapFrom(src => src.OrderStatus));

            CreateMap<OrderRequest, Order>().ReverseMap();
            CreateMap<OrderDetail, ProductOrderDetails>();
            CreateMap<Order, OrderDetailsResponse>()
                .ForMember(dest => dest.ProductOrderDetails, opt => opt.MapFrom(src => src.OrderDetails));

            //product
            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Images.FirstOrDefault() != null ? src.Images.FirstOrDefault()!.ImageUrl : null))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => Math.Round(src.Rating, 1)));

            CreateMap<ProductRequest, Product>();
            CreateMap<Product, ProductDetailsResponse>();
            CreateMap<ProductColor, ColorSizeResponse>()
                .ForMember(dest => dest.SizeInStocks, opt => opt.MapFrom(src => src.ProductSizes));

            CreateMap<ProductSize, SizeInStock>()
                .ForMember(dest => dest.SizeName, opt => opt.MapFrom(src => src.Size.Name));

            //Voucher
            CreateMap<Voucher, VoucherDTO>().ReverseMap();

            //Payment Method
            CreateMap<PaymentMethod, PaymentMethodDTO>().ReverseMap();
        }
    }
}
