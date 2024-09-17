using Microsoft.AspNetCore.Http;
using MyStore.Application.Admin.Request;
using MyStore.Application.Admin.Response;
using MyStore.Application.DTO;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Products
{
    public interface IProductService
    {
        Task<ProductDTO> CreateProductAsync(ProductRequest request, IFormFileCollection images);
        Task<PagedResponse<ProductDTO>> GetProductsAsync(int page, int pageSize, string? keySearch);
        Task<PagedResponse<ProductDTO>> GetFilterProductsAsync(Filters filters);

        Task<Admin.Response.ProductDetailsResponse> GetProductAsync(int id);
        Task<ProductDTO> UpdateProductAsync(int id, ProductRequest request, IFormFileCollection images);
        Task<bool> UpdateProductEnableAsync(int id, UpdateEnableRequest request);
        Task DeleteProductAsync(int id);
    }
}
