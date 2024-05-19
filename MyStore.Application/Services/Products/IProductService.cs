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
        Task<List<ProductResponse>> GetProductsAsync();
        Task<PageResponse<ProductResponse>> GetProductsAsync(int page, int pageSize, string? keySearch);
        Task<ProductDetailResponse?> GetProductAsync(int id);
        Task<bool> UpdateProductAsync(UpdateProductRequest request, IFormFileCollection images);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateProductEnableAsync(UpdateProductEnableRequest request);

        Task<List<BrandResponse>> GetBrandsAsync();
        Task<List<BrandResponse>> GetBrandNamesAsync();
        Task AddBrandAsync(CreateBrandRequest brand);
        Task<bool> DeleteBrandAsync(int id);

        Task<List<CategoryResponse>> GetCategoriesAsync();
        Task AddCategoryAsync(NameRequest category);
        Task<bool> DeleteCategoryAsync(int id);

        Task<List<MaterialResponse>> GetMaterialsAsync();
        Task AddMaterialAsync(NameRequest material);
        Task<bool> DeleteMaterialAsync(int id);

        Task<IList<SizeResponse>> GetSizesAsync();
        Task AddSizeAsync(CreateSizeRequest request);
        Task<bool> DeleteSizeAsync(int id);
    }
}
