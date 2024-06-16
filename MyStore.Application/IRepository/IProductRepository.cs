using Microsoft.EntityFrameworkCore.Storage;
using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductMaterial>> GetProductMaterialsAsync(int id);
        Task<IEnumerable<ProductSize>> GetProductSizesAsync(int id);
        Task<IEnumerable<Image>> GetProductImagesAsync(int id);
        Task<Image?> GetFirstImageByProductIdAsync(int id);
        Task AddProductImagesAsync(IList<Image> images);
        Task AddProductSizesAsync(IList<ProductSize> sizes);
        Task AddProductMaterialsAsync(IList<ProductMaterial> materials);

        Task<ProductSize?> GetProductSizeAsync(int productId, int sizeId);
        Task<ProductMaterial?> GetProductMaterialAsync(int productId, int materialId);
        Task UpdateProductSizeAsync(ProductSize size);
        Task DeleteProductSizeAsync(ProductSize size);
        Task UpdateProductMaterialAsync(ProductMaterial productMaterial);
        Task DeleteProductMaterialAsync(ProductMaterial productMaterial);
        Task DeleteAllImageByProductId(int productId);

        Task DeleteProductImagesAsync(IList<Image> images);
        Task DeleteProductSizesAsync(IList<ProductSize> sizes);
        Task DeleteProductMaterialsAsync(IList<ProductMaterial> materials);

        Task AddProductAsync(Product product);
        Task<Product?> GetProductWithProductAttributesAsync(int id);
        Task<IEnumerable<Product>> GetProductsWithProductAttributesAsync();
        Task<IEnumerable<Product>> GetProductsWithProductAttributesAsync(int page, int pageSize);
        Task<IEnumerable<Product>> GetProductsWithProductAttributesAsync(int page, int pageSize, string key);
        Task<int> CountAsync();
        Task<int> CountAsync(string key);
        Task<Product?> FindProductByIdAsync(int id);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Product product);

        Task<IEnumerable<Brand>> GetBrandsAsync();
        Task<Brand?> FindBrandByIdAsync(int id);
        Task AddBrandAsync(Brand brand);
        Task DeleteBrandAsync(Brand brand);

        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<Category?> FindCategoryByIdAsync(int id);
        Task AddCategoryAsync(Category category);
        Task DeleteCategoryAsync(Category category);

        Task<IEnumerable<Material>> GetMaterialsAsync();
        Task<Material?> FindMaterialByIdAsync(int id);
        Task AddMaterialAsync(Material material);
        Task DeleteMaterialAsync(Material material);

        Task<IEnumerable<Size>> GetSizesAsync();
        Task<Size?> FindSizeByIdAsync(int id);
        Task AddSizeAsync(Size size);
        Task DeleteSizeAsync(Size size);
    }
}
