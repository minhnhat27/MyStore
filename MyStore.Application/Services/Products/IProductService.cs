using Microsoft.AspNetCore.Http;
using MyStore.Application.Admin.Request;
using MyStore.Application.Admin.Response;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Products
{
    public interface IProductService
    {
        Task CreateProductAsync(CreateProductRequest request, IFormFileCollection images);
        Task<PagedResponse<ProductResponse>> GetProductsAsync(int page, int pageSize, string? keySearch);
        Task<ProductDetailResponse?> GetProductAsync(int id);
        Task<bool> UpdateProductAsync(UpdateProductRequest request, IFormFileCollection images);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateProductEnableAsync(UpdateProductEnableRequest request);

        Task<IEnumerable<BrandResponse>> GetBrandsAsync();
        Task AddBrandAsync(CreateBrandRequest brand);
        Task<bool> DeleteBrandAsync(int id);

        Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
        Task AddCategoryAsync(NameRequest category);
        Task<bool> DeleteCategoryAsync(int id);

        Task<IEnumerable<MaterialResponse>> GetMaterialsAsync();
        Task AddMaterialAsync(NameRequest material);
        Task<bool> DeleteMaterialAsync(int id);

        Task<IEnumerable<SizeResponse>> GetSizesAsync();
        Task AddSizeAsync(CreateSizeRequest request);
        Task<bool> DeleteSizeAsync(int id);
    }
}
