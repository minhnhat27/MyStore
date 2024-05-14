using Microsoft.AspNetCore.Http;
using MyStore.Application.Admin.Request;
using MyStore.Application.Admin.Response;
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
        public ProductService(IProductRepository productRepository, 
            IImageRepository imageRepository, ITransactionRepository transactionRepository)
        {
            _productRepository = productRepository;
            _imageRepository = imageRepository;
            _transactionRepository = transactionRepository;
        }

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
        public async Task<List<ProductResponse>> GetProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetProductsWithBrandAndCategoryAndImagesAsync();
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

                List<Base64Response> images = new();
                for (int i = 0; i < products.Count; i++)
                {
                    var image = await _productRepository.GetFirstImageByProductIdAsync(products[i].Id);
                    if(image != null)
                    {
                        var base64 = await _imageRepository.GetImageBase64Async(ImageType.Products.ToString(), image.ImageName);
                        res[i].base64String = base64;
                    }
                }
                return res;
            }
            catch
            {
                return new List<ProductResponse>();
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
                };

                var images = await _productRepository.GetProductImagesAsync(product.Id);
                if (images != null)
                {
                    var lst = images.Select(e => e.ImageName).ToList();
                    var base64 = await _imageRepository.GetImagesBase64Async(ImageType.Products.ToString(), lst);
                    res.Images = base64;
                }
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
                        foreach(var old in oldProductSizes)
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
                        await _productRepository.DeleteAllImageByProductId(product.Id);
                        var nameFiles = new List<string>();
                        List<Image> imgs = new();
                        foreach (var image in images)
                        {
                            var name = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            Image img = new()
                            {
                                ProductId = product.Id,
                                ImageName = name
                            };
                            nameFiles.Add(name);
                            imgs.Add(img);
                        }
                        await _productRepository.AddProductImagesAsync(imgs);

                        _imageRepository.DeleteImages(ImageType.Products.ToString(), olgImgs.Select(e => e.ImageName).ToList());
                        await _imageRepository.SaveImagesAsync(ImageType.Products.ToString(), images, nameFiles);

                        await _productRepository.UpdateProductAsync(product);

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch(Exception ex)
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
                using(var transaction = await _transactionRepository.BeginTransactionAsync())
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


        //--//
        public async Task<List<BrandResponse>> GetBrandsAsync()
        {
            var brands = await _productRepository.GetBrandsAsync();
            var res = brands.Select(e => new BrandResponse
            {
                Id = e.Id,
                Name = e.Name
            }).ToList();
            for(var i = 0; i < res.Count; i++)
            {
                res[i].Base64String = await _imageRepository.GetImageBase64Async(ImageType.Brands.ToString(), brands[i].ImageName);
            }
            return res;
        }
        public async Task<List<BrandResponse>> GetBrandNamesAsync()
        {
            var brands = await _productRepository.GetBrandsAsync();
            var res = brands.Select(e => new BrandResponse
            {
                Id = e.Id,
                Name = e.Name
            }).ToList();
            return res;
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
                _imageRepository.DeleteImage(ImageType.Brands.ToString(), brand.ImageName);
                return true;
            }
            else
            {
                return false;
            }
        }

        //--//
        public async Task<List<CategoryResponse>> GetCategoriesAsync()
        {
            var categories = await _productRepository.GetCategoriesAsync();
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
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _productRepository.FindCategoryByIdAsync(id);
            if (category != null)
            {
                await _productRepository.DeleteCategoryAsync(category);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        //--//
        public async Task<List<MaterialResponse>> GetMaterialsAsync()
        {
            var materials = await _productRepository.GetMaterialsAsync();
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
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            var material = await _productRepository.FindMaterialByIdAsync(id);
            if (material != null)
            {
                await _productRepository.DeleteMaterialAsync(material);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<IList<SizeResponse>> GetSizesAsync()
        {
            var result = await _productRepository.GetSizesAsync();
            return result.Select(e => new SizeResponse
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
        }

        public async Task<bool> DeleteSizeAsync(int id)
        {
            var size = await _productRepository.FindSizeByIdAsync(id);
            if (size != null)
            {
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
