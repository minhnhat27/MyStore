using AutoMapper;
using MyStore.Application.Admin.Request;
using MyStore.Application.Admin.Response;
using MyStore.Application.DTO;
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
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<Order, OrderDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<Material, MaterialDTO>().ReverseMap();
            CreateMap<Brand, BrandDTO>().ReverseMap();
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Size, SizeDTO>().ReverseMap();

            CreateMap<OrderRequest, Order>();
            CreateMap<ProductRequest, Product>();
            CreateMap<SizeAndQuantity, ProductSize>().ReverseMap();

            CreateMap<Product, ProductResponse>();
            CreateMap<User, UserResponse>();

            CreateMap<Order, OrderDetailsResponse>();
            CreateMap<OrderDetail, ProductsOrderDetail>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
        }
    }
}
