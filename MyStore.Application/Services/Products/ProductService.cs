using Microsoft.AspNetCore.Http;
using MyStore.Application.Admin.Request;
using MyStore.Application.Admin.Response;
using MyStore.Application.ICaching;
using MyStore.Application.IRepository;
using MyStore.Application.Model;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICache _productCache;
        public ProductService(IProductRepository productRepository, IImageRepository imageRepository, 
            ITransactionRepository transactionRepository,
            ICache productAttributesCache)
        {
            _productRepository = productRepository;
            _imageRepository = imageRepository;
            _transactionRepository = transactionRepository;
            _productCache = productAttributesCache;
        }

        //-Products-//
        public async Task CreateProductAsync(CreateProductRequest request, IFormFileCollection images)
        {
            using (var transaction = await _transactionRepository.BeginTransactionAsync())
            {
                try
                {
                    Product model = new()
                    {
                        Name = request.Name,
                        Gender = request.Gender,
                        Description = request.Description,
                        Enable = request.Enable,
                        BrandId = request.Brand,
                        CategoryId = request.Category,
                    };
                    await _productRepository.AddProductAsync(model);

                    List<ProductSize> sizes = new();
                    foreach (var s in request.Sizes)
                    {
                        ProductSize productSize = new()
                        {
                            ProductId = model.Id,
                            SizeId = s.Id,
                            InStock = s.Quantity,
                            Price = s.Price,
                            DiscountPercent = s.Discount,
                        };
                        sizes.Add(productSize);
                    }
                    await _productRepository.AddProductSizesAsync(sizes);

                    List<ProductMaterial> materials = new();
                    foreach (var m in request.Materials)
                    {
                        ProductMaterial productMaterial = new()
                        {
                            MaterialId = m,
                            ProductId = model.Id,
                        };
                        materials.Add(productMaterial);
                    }
                    await _productRepository.AddProductMaterialsAsync(materials);

                    var nameFiles = new List<string>();
                    List<Image> imgs = new();
                    foreach (var image in images)
                    {
                        var name = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        Image img = new()
                        {
                            ProductId = model.Id,
                            ImageName = name
                        };
                        nameFiles.Add(name);
                        imgs.Add(img);
                    }
                    await _productRepository.AddProductImagesAsync(imgs);
                    await _imageRepository.SaveImagesAsync(ImageType.Products.ToString(), images, nameFiles);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                }
            }
        }
        
        public async Task<PageResponse<ProductResponse>> GetProductsAsync(int page, int pageSize, string? keySearch)
        {
            try
            {
                int totalProduct;
                IList<Product> products;
                if (keySearch == null)
                {
                    totalProduct = await _productRepository.CountAsync();
                    products = await _productRepository.GetProductsWithProductAttributesAsync(page, pageSize);
                }
                else
                {
                    totalProduct = await _productRepository.CountAsync(keySearch);
                    products = await _productRepository.GetProductsWithProductAttributesAsync(page, pageSize, keySearch);
                }
                var res = products.Select(e => new ProductResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    Enable = e.Enable,
                    Gender = e.Gender,
                    Sold = e.Sold,
                    BrandName = e.Brand.Name,
                    CategoryName = e.Category.Name,
                }).ToList();

                for (int i = 0; i < products.Count; i++)
                {
                    var image = await _productRepository.GetFirstImageByProductIdAsync(products[i].Id);
                    if (image != null)
                    {
                        //var base64 = await _imageRepository.GetImageBase64Async(ImageType.Products.ToString(), image.ImageName);
                        //res[i].base64String = base64;
                        res[i].ImageUrl = image.ImageName;
                    }
                }

                return new PageResponse<ProductResponse>
                {
                    Items = res,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalProduct
                };
            }
            catch
            {
                return new PageResponse<ProductResponse>();
            }
        }

        public async Task<ProductDetailResponse?> GetProductAsync(int id)
        {
            var product = await _productRepository.GetProductWithProductAttributesAsync(id);
            if (product != null)
            {
                var res = new ProductDetailResponse
                {
                    Id = product.Id,
                    Name = product.Name,
                    Enable = product.Enable,
                    Gender = product.Gender,
                    Sold = product.Sold,
                    Brand = product.BrandId,
                    Category = product.CategoryId,
                    Materials = product.Materials.Select(e => e.MaterialId).ToList(),
                    SizeQuantity = product.Sizes.Select(e => new SizeAndQuantity
                    {
                        Id = e.SizeId,
                        Price = e.Price,
                        Discount = e.DiscountPercent,
                        Quantity = e.InStock
                    }).ToList(),
                    Description = product.Description,
                    Sizes = product.Sizes.Select(e => e.SizeId).ToList(),
                    Images = product.Images.Select(e => e.ImageName).ToList(),
                };

                return res;
            }
            else return null;
        }

        public async Task<bool> UpdateProductAsync(UpdateProductRequest request, IFormFileCollection images)
        {
            var product = await _productRepository.FindProductByIdAsync(request.Id);
            if (product != null)
            {
                using (var transaction = await _transactionRepository.BeginTransactionAsync())
                {
                    try
                    {
                        product.Name = request.Name;
                        product.Description = request.Description;
                        product.Gender = request.Gender;
                        product.CategoryId = request.Category;
                        product.BrandId = request.Brand;
                        product.Enable = request.Enable;

                        List<ProductSize> sizes = new();
                        foreach (var s in request.Sizes)
                        {
                            var exist = await _productRepository.GetProductSizeAsync(product.Id, s.Id);
                            if (exist != null)
                            {
                                exist.InStock = s.Quantity;
                                exist.Price = s.Price;
                                exist.DiscountPercent = s.Discount;
                                await _productRepository.UpdateProductSizeAsync(exist);
                            }
                            else
                            {
                                ProductSize productSize = new()
                                {
                                    ProductId = product.Id,
                                    SizeId = s.Id,
                                    InStock = s.Quantity,
                                    Price = s.Price,
                                    DiscountPercent = s.Discount,
                                };
                                sizes.Add(productSize);
                            }
                        }

                        var oldProductSizes = await _productRepository.GetProductSizesAsync(product.Id);
                        var sizeId = request.Sizes.Select(e => e.Id);
                        foreach (var old in oldProductSizes)
                        {
                            if (!sizeId.Contains(old.SizeId))
                            {
                                await _productRepository.DeleteProductSizeAsync(old);
                            }
                        }
                        await _productRepository.AddProductSizesAsync(sizes);

                        List<ProductMaterial> materials = new();
                        foreach (var mId in request.Materials)
                        {
                            var exist = await _productRepository.GetProductMaterialAsync(product.Id, mId);
                            if (exist == null)
                            {
                                ProductMaterial productMaterial = new()
                                {
                                    MaterialId = mId,
                                    ProductId = product.Id,
                                };
                                materials.Add(productMaterial);
                            }
                        }
                        var oldProductMaterials = await _productRepository.GetProductMaterialsAsync(product.Id);
                        foreach (var old in oldProductMaterials)
                        {
                            if (!request.Materials.Contains(old.MaterialId))
                            {
                                await _productRepository.DeleteProductMaterialAsync(old);
                            }
                        }
                        await _productRepository.AddProductMaterialsAsync(materials);


                        var olgImgs = await _productRepository.GetProductImagesAsync(product.Id);
                        List<Image> lstImaDel = new();

                        if(request.ImagesUrl == null)
                        {
                            lstImaDel.AddRange(olgImgs);
                        }
                        else
                        {
                            foreach (var image in olgImgs)
                            {
                                if (!request.ImagesUrl.Contains(image.ImageName))
                                {
                                    lstImaDel.Add(image);
                                }
                            }
                        }
                        _imageRepository.DeleteImages(ImageType.Products.ToString(), lstImaDel.Select(e => e.ImageName).ToList());
                        await _productRepository.DeleteProductImagesAsync(lstImaDel);

                        var fileNames = new List<string>();
                        List<Image> newImages = new();
                        foreach (var image in images)
                        {
                            var name = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            Image img = new()
                            {
                                ProductId = product.Id,
                                ImageName = name
                            };
                            fileNames.Add(name);
                            newImages.Add(img);
                        }
                        await _productRepository.AddProductImagesAsync(newImages);

                        await _imageRepository.SaveImagesAsync(ImageType.Products.ToString(), images, fileNames);

                        await _productRepository.UpdateProductAsync(product);

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception(ex.Message);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductEnableAsync(UpdateProductEnableRequest request)
        {
            var product = await _productRepository.FindProductByIdAsync(request.Id);
            if (product != null)
            {
                product.Enable = request.Enable;
                await _productRepository.UpdateProductAsync(product);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.FindProductByIdAsync(id);
            if (product != null)
            {
                using (var transaction = await _transactionRepository.BeginTransactionAsync())
                {
                    try
                    {
                        var images = await _productRepository.GetProductImagesAsync(id);
                        _imageRepository.DeleteImages(ImageType.Products.ToString(), images.Select(e => e.ImageName).ToList());

                        await _productRepository.DeleteProductAsync(product);

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception(ex.Message);
                    }
                }
            }
            else return false;
        }

        //-Cache-//
        private async Task UpdateCachedBrands() 
            => _productCache.Set("Brands", await _productRepository.GetBrandsAsync());
        private async Task UpdateCachedCategories()
            => _productCache.Set("Categories", await _productRepository.GetCategoriesAsync());
        private async Task UpdateCachedMaterials()
            => _productCache.Set("Materials", await _productRepository.GetMaterialsAsync());
        private async Task UpdateCachedSizes()
            => _productCache.Set("Sizes", await _productRepository.GetSizesAsync());

        //-Brands-//
        public async Task<List<BrandResponse>> GetBrandsAsync()
        {
            var brands = _productCache.Get<IList<Brand>>("Brands");
            if(brands == null)
            {
                brands = await _productRepository.GetBrandsAsync();
                _productCache.Set("Brands", brands);
            }

            return brands.Select(e => new BrandResponse
            {
                Id = e.Id,
                Name = e.Name,
                ImageUrl = e.ImageName,
            }).ToList();
        }

        public async Task AddBrandAsync(CreateBrandRequest brand)
        {
            using (var transaction = await _transactionRepository.BeginTransactionAsync())
            {
                try
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(brand.Image.FileName);
                    Brand model = new() { Name = brand.Name, ImageName = fileName };
                    await _productRepository.AddBrandAsync(model);
                    await _imageRepository.SaveImageAsync(ImageType.Brands.ToString(), brand.Image, fileName);

                    await UpdateCachedBrands();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                }
            }
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            var brand = await _productRepository.FindBrandByIdAsync(id);
            if (brand != null)
            {
                await _productRepository.DeleteBrandAsync(brand);
                await UpdateCachedBrands();
                _imageRepository.DeleteImage(ImageType.Brands.ToString(), brand.ImageName);
                return true;
            }
            else
            {
                return false;
            }
        }

        //-Categories-//
        public async Task<List<CategoryResponse>> GetCategoriesAsync()
        {
            var categories = _productCache.Get<IList<Category>>("Categories");
            if(categories == null)
            {
                categories = await _productRepository.GetCategoriesAsync();
                _productCache.Set("Categories", categories);
            }
            return categories.Select(e => new CategoryResponse
            {
                Id = e.Id,
                Name = e.Name
            }).ToList();
        }

        public async Task AddCategoryAsync(NameRequest category)
        {
            Category model = new() { Name = category.Name };
            await _productRepository.AddCategoryAsync(model);
            await UpdateCachedCategories();
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _productRepository.FindCategoryByIdAsync(id);
            if (category != null)
            {
                await UpdateCachedCategories();
                await _productRepository.DeleteCategoryAsync(category);
                return true;
            }
            else
            {
                return false;
            }
        }

        //-Materials-//
        public async Task<List<MaterialResponse>> GetMaterialsAsync()
        {
            var materials = _productCache.Get<IList<Material>>("Materials");
            if (materials == null)
            {
                materials = await _productRepository.GetMaterialsAsync();
                _productCache.Set("Materials", materials);
            }

            return materials.Select(e => new MaterialResponse
            {
                Id = e.Id,
                Name = e.Name
            }).ToList();
        }

        public async Task AddMaterialAsync(NameRequest material)
        {
            Material model = new() { Name = material.Name };
            await _productRepository.AddMaterialAsync(model);
            await UpdateCachedMaterials();
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            var material = await _productRepository.FindMaterialByIdAsync(id);
            if (material != null)
            {
                await UpdateCachedMaterials();
                await _productRepository.DeleteMaterialAsync(material);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        //-Sizes-//
        public async Task<IList<SizeResponse>> GetSizesAsync()
        {
            var sizes = _productCache.Get<IList<Size>>("Sizes");
            if(sizes == null)
            {
                sizes = await _productRepository.GetSizesAsync();
                _productCache.Set("Size", sizes);
            }

            return sizes.Select(e => new SizeResponse
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description
            }).ToList();
        }

        public async Task AddSizeAsync(CreateSizeRequest request)
        {
            Size model = new() { Name = request.Name, Description = request.Description };
            await _productRepository.AddSizeAsync(model);
            await UpdateCachedSizes();
        }

        public async Task<bool> DeleteSizeAsync(int id)
        {
            var size = await _productRepository.FindSizeByIdAsync(id);
            if (size != null)
            {
                await UpdateCachedSizes();
                await _productRepository.DeleteSizeAsync(size);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
