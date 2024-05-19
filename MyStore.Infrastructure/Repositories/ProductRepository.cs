using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using MyStore.Application.IRepository;
using MyStore.Application.Request;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.Paging;

namespace MyStore.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationContext _Dbcontext;
        public ProductRepository(ApplicationContext dbcontext)
        {
            _Dbcontext = dbcontext;
        }
        public async Task AddProductAsync(Product product)
        {
            await _Dbcontext.AddAsync(product);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task<Product?> GetProductWithProductAttributesAsync(int id)
        {
            return await _Dbcontext.Products
                .Include(e => e.Images)
                .Include(e => e.Sizes)
                .Include(e => e.Materials)
                .SingleOrDefaultAsync(e => e.Id == id);
        }
        public async Task<IList<Product>> GetProductsWithProductAttributesAsync()
        {
            return await _Dbcontext.Products
                .Include(e => e.Brand)
                .Include(e => e.Category)
                .Include(e => e.Images)
                .ToListAsync();
        }
        public async Task<IList<Product>> GetProductsWithProductAttributesAsync(int page, int pageSize)
        {
            return await _Dbcontext.Products
                    .Paginate(page, pageSize)
                    .Include(e => e.Brand)
                    .Include(e => e.Category)
                    .Include(e => e.Images)
                    .ToListAsync();
        }

        public async Task<IList<Product>> GetProductsWithProductAttributesAsync(int page, int pageSize, string key)
        {
            return await _Dbcontext.Products
                    .Where(e => e.Name.Contains(key) || e.Id.ToString().Contains(key))
                    .Paginate(page, pageSize)
                    .Include(e => e.Brand)
                    .Include(e => e.Category)
                    .Include(e => e.Images)
                    .ToListAsync();
        }
        public async Task<int> CountAsync()
        {
            return await _Dbcontext.Products.CountAsync();
        }
        public async Task<int> CountAsync(string key)
        {
            return await _Dbcontext.Products
                .Where(e => e.Name.Contains(key) || e.Id.ToString().Equals(key))
                .CountAsync();
        }
        public async Task<Product?> FindProductByIdAsync(int id)
        {
            return await _Dbcontext.Products.FindAsync(id);
        }
        public async Task DeleteProductAsync(Product product)
        {
            _Dbcontext.Remove(product);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task UpdateProductAsync(Product product)
        {
            _Dbcontext.Update(product);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task AddBrandAsync(Brand brand)
        {
            await _Dbcontext.AddAsync(brand);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task DeleteBrandAsync(Brand brand)
        {
            _Dbcontext.Remove(brand);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task<Brand?> FindBrandByIdAsync(int id)
        {
            return await _Dbcontext.Brands.FindAsync(id);
        }
        public async Task<IList<Brand>> GetBrandsAsync()
        {
            return await _Dbcontext.Brands.ToListAsync();
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            _Dbcontext.Remove(category);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task AddCategoryAsync(Category category)
        {
            await _Dbcontext.AddAsync(category);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task<Category?> FindCategoryByIdAsync(int id)
        {
            return await _Dbcontext.Categories.FindAsync(id);
        }
        public async Task<IList<Category>> GetCategoriesAsync()
        {
            return await _Dbcontext.Categories.ToListAsync();
        }

        public async Task<IList<Material>> GetMaterialsAsync()
        {
            return await _Dbcontext.Materials.ToListAsync();
        }
        public async Task<Material?> FindMaterialByIdAsync(int id)
        {
            return await _Dbcontext.Materials.FindAsync(id);
        }
        public async Task AddMaterialAsync(Material material)
        {
            await _Dbcontext.AddAsync(material);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task DeleteMaterialAsync(Material material)
        {
            _Dbcontext.Remove(material);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task<Image?> GetFirstImageByProductIdAsync(int id)
        {
            return await _Dbcontext.Images.FirstOrDefaultAsync(e => e.ProductId == id);
        }

        public async Task AddProductImagesAsync(IList<Image> images)
        {
            await _Dbcontext.AddRangeAsync(images);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task AddProductSizesAsync(IList<ProductSize> sizes)
        {
            await _Dbcontext.AddRangeAsync(sizes);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task AddProductMaterialsAsync(IList<ProductMaterial> materials)
        {
            await _Dbcontext.AddRangeAsync(materials);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task<IList<Size>> GetSizesAsync()
        {
            return await _Dbcontext.Sizes.ToListAsync();
        }

        public async Task<Size?> FindSizeByIdAsync(int id)
        {
            return await _Dbcontext.Sizes.FindAsync(id);
        }

        public async Task AddSizeAsync(Size size)
        {
            await _Dbcontext.AddAsync(size);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task DeleteSizeAsync(Size size)
        {
            _Dbcontext.Remove(size);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task DeleteProductSizesAsync(IList<ProductSize> sizes)
        {
            _Dbcontext.RemoveRange(sizes);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task DeleteProductMaterialsAsync(IList<ProductMaterial> materials)
        {
            _Dbcontext.RemoveRange(materials);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task DeleteProductImagesAsync(IList<Image> images)
        {
            _Dbcontext.RemoveRange(images);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task<IList<ProductMaterial>> GetProductMaterialsAsync(int id)
        {
            return await _Dbcontext.ProductMaterials.Where(e => e.ProductId == id).ToListAsync();
        }

        public async Task<IList<ProductSize>> GetProductSizesAsync(int id)
        {
            return await _Dbcontext.ProductSizes.Where(e => e.ProductId == id).ToListAsync();
        }

        public async Task<IList<Image>> GetProductImagesAsync(int id)
        {
            return await _Dbcontext.Images.Where(e => e.ProductId == id).ToListAsync();
        }

        public async Task<ProductSize?> GetProductSizeAsync(int productId, int sizeId)
        {
            return await _Dbcontext.ProductSizes.FindAsync(productId, sizeId);
        }

        public async Task UpdateProductSizeAsync(ProductSize size)
        {
            _Dbcontext.Update(size);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task DeleteProductSizeAsync(ProductSize size)
        {
            _Dbcontext.Remove(size);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task<ProductMaterial?> GetProductMaterialAsync(int productId, int materialId)
        {
            return await _Dbcontext.ProductMaterials.FindAsync(productId, materialId);
        }

        public async Task UpdateProductMaterialAsync(ProductMaterial productMaterial)
        {
            _Dbcontext.Update(productMaterial);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task DeleteProductMaterialAsync(ProductMaterial productMaterial)
        {
            _Dbcontext.Remove(productMaterial);
            await _Dbcontext.SaveChangesAsync();
        }
        public async Task DeleteAllImageByProductId(int productId)
        {
            var images = await GetProductImagesAsync(productId);
            _Dbcontext.RemoveRange(images);
            await _Dbcontext.SaveChangesAsync();
        }
    }
}
