using Microsoft.EntityFrameworkCore.Storage;
using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository
{
    public interface IProductRepository
    {
        Task<IList<ProductMaterial>> GetProductMaterialsAsync(int id);
        Task<IList<ProductSize>> GetProductSizesAsync(int id);
        Task<IList<Image>> GetProductImagesAsync(int id);
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
        Task<IList<Product>> GetProductsWithBrandAndCategoryAndImagesAsync();
        Task<Product?> FindProductByIdAsync(int id);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Product product);

        Task<IList<Brand>> GetBrandsAsync();
        Task<Brand?> FindBrandByIdAsync(int id);
        Task AddBrandAsync(Brand brand);
        Task DeleteBrandAsync(Brand brand);

        Task<IList<Category>> GetCategoriesAsync();
        Task<Category?> FindCategoryByIdAsync(int id);
        Task AddCategoryAsync(Category category);
        Task DeleteCategoryAsync(Category category);

        Task<IList<Material>> GetMaterialsAsync();
        Task<Material?> FindMaterialByIdAsync(int id);
        Task AddMaterialAsync(Material material);
        Task DeleteMaterialAsync(Material material);

        Task<IList<Size>> GetSizesAsync();
        Task<Size?> FindSizeByIdAsync(int id);
        Task AddSizeAsync(Size size);
        Task DeleteSizeAsync(Size size);
    }
}
