using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.DTOs;
using MyStore.Application.IRepositories;
using MyStore.Application.IRepositories.Products;
using MyStore.Application.IStorage;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using System.Linq.Expressions;

namespace MyStore.Application.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductSizeRepository _productSizeRepository;
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductMaterialRepository _productMaterialRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ITransactionRepository _transactionRepository;

        private readonly IFileStorage _fileStorage;
        private readonly IMapper _mapper;

        private readonly string path = "assets/images/products";

        public ProductService(IProductRepository productRepository,
            IProductSizeRepository productSizeRepository,
            IProductColorRepository productColorRepository,
            IProductMaterialRepository productMaterialRepository,
            IImageRepository imageRepository, IFileStorage fileStorage, 
            ITransactionRepository transactionRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _productSizeRepository = productSizeRepository;
            _productColorRepository = productColorRepository;
            _productMaterialRepository = productMaterialRepository;
            _imageRepository = imageRepository;
            _fileStorage = fileStorage;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<ProductDTO> CreateProductAsync(ProductRequest request, IFormFileCollection images)
        {
            using var transaction = await _transactionRepository.BeginTransactionAsync();
            try
            {
                var product = _mapper.Map<Product>(request);
                await _productRepository.AddAsync(product);
                var productPath = path + "/" + product.Id;

                List<string> colorFileNames = new();
                List<IFormFile> colorImages = new();
                List<ProductSize> productSizes = new();

                foreach (var color in request.ColorSizes.ToHashSet())
                {
                    var name = "";
                    if (color.Image != null)
                    {
                        name = Guid.NewGuid().ToString() + Path.GetExtension(color.Image.FileName);
                        colorFileNames.Add(name);
                        colorImages.Add(color.Image);
                    }

                    var productColor = new ProductColor
                    {
                        ColorName = color.ColorName,
                        ProductId = product.Id,
                        ImageUrl = Path.Combine(productPath, name)
                    };

                    await _productColorRepository.AddAsync(productColor);

                    var sizes = color.SizeInStocks.Select(size =>
                    {
                        return new ProductSize
                        {
                            ProductColorId = productColor.Id,
                            SizeId = size.SizeId,
                            InStock = size.InStock,
                        };
                    });
                    productSizes.AddRange(sizes);
                }
                await _productSizeRepository.AddAsync(productSizes);

                var materials = request.MaterialIds.Select(id => new ProductMaterial
                {
                    MaterialId = id,
                    ProductId = product.Id,
                });
                await _productMaterialRepository.AddAsync(materials);

                List<string> commonFileNames = new();
                var imgs = images.Select(file =>
                {
                    var name = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    commonFileNames.Add(name);
                    var image = new Image()
                    {
                        ProductId = product.Id,
                        ImageUrl = Path.Combine(productPath, name),
                    };
                    return image;
                });
                await _imageRepository.AddAsync(imgs);

                colorImages.AddRange(images);
                colorFileNames.AddRange(commonFileNames);

                await _fileStorage.SaveAsync(productPath, colorImages, colorFileNames);

                await transaction.CommitAsync();

                var res = _mapper.Map<ProductDTO>(product);

                var image = imgs.FirstOrDefault();
                if (image != null)
                {
                    res.ImageUrl = image.ImageUrl; ;
                }
                return res;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }
        
        public async Task<PagedResponse<ProductDTO>> GetProductsAsync(int page, int pageSize, string? keySearch)
        {
            try
            {
                int totalProduct;
                IEnumerable<Product> products;
                if (string.IsNullOrEmpty(keySearch))
                {
                    totalProduct = await _productRepository.CountAsync();
                    products = await _productRepository.GetPagedOrderByDescendingAsync(page, pageSize, null, e => e.CreatedAt);
                }
                else
                {
                    Expression<Func<Product, bool>> expression = e =>
                        e.Name.Contains(keySearch)
                        || e.Sold.ToString().Equals(keySearch)
                        || e.Price.ToString().Equals(keySearch);

                    totalProduct = await _productRepository.CountAsync(expression);
                    products = await _productRepository.GetPagedOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
                }

                var res = _mapper.Map<IEnumerable<ProductDTO>>(products).Select(x =>
                {
                    var image = products.Single(e => e.Id == x.Id).Images.FirstOrDefault();
                    if (image != null)
                    {
                        x.ImageUrl = image.ImageUrl;
                    }
                    return x;
                });

                return new PagedResponse<ProductDTO>
                {
                    Items = res,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalProduct
                };
            }
            catch(Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        private Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = expr1.Parameters[0];
            var body = Expression.AndAlso(expr1.Body, Expression.Invoke(expr2, parameter));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public async Task<PagedResponse<ProductDTO>> GetFilterProductsAsync(ProductFiltersRequest filters)
        {
            try
            {
                int totalProduct = 0;
                IEnumerable<Product> products = [];
                Expression<Func<Product, bool>> expression = e => e.Enable;
                
                if(filters.MinPrice != null && filters.MaxPrice != null)
                {
                    expression = CombineExpressions(expression, e =>
                        (e.Price - (e.Price * (e.DiscountPercent / 100.0))) >= filters.MinPrice
                        &&
                        (e.Price - (e.Price * (e.DiscountPercent / 100.0))) <= filters.MaxPrice
                    );
                }
                else
                {
                    if (filters.MinPrice != null)
                    {
                        expression = CombineExpressions(expression, e => (e.Price - (e.Price * (e.DiscountPercent / 100.0))) >= filters.MinPrice);
                    }
                    else if (filters.MaxPrice != null)
                    {
                        expression = CombineExpressions(expression, e => (e.Price - (e.Price * (e.DiscountPercent / 100.0))) <= filters.MaxPrice);
                    }
                }
                
                if(filters.Discount != null && filters.Discount == true)
                {
                    expression = CombineExpressions(expression, e => e.DiscountPercent > 0);
                }
                if (filters.Rating != null)
                {
                    expression = CombineExpressions(expression, e => e.ProductReviews.Average(e => e.Star) >= filters.Rating);
                }
                if (filters.CategoryIds != null && filters.CategoryIds.Any())
                {
                    expression = CombineExpressions(expression, e => filters.CategoryIds.Contains(e.CategoryId));
                }
                if (filters.BrandIds != null && filters.BrandIds.Any())
                {
                    expression = CombineExpressions(expression, e => filters.BrandIds.Contains(e.BrandId));
                }
                if (filters.MaterialIds != null && filters.MaterialIds.Any())
                {
                    expression = CombineExpressions(expression,
                        e => filters.MaterialIds.Any(id => e.Materials.Any(m => m.MaterialId == id)));
                }
                if (filters.Genders != null && filters.Genders.Any())
                {
                    expression = CombineExpressions(expression, e => filters.Genders.Contains(e.Gender));
                }

                totalProduct = await _productRepository.CountAsync(expression);
                Expression<Func<Product, double>> priceExp = e => e.Price - (e.Price * (e.DiscountPercent / 100.0));

                products = filters.Sorter switch
                {
                    SortEnum.SOLD => await _productRepository
                                               .GetPagedOrderByDescendingAsync(filters.Page, filters.PageSize, expression, e => e.Sold),
                    SortEnum.PRICE_ASC => await _productRepository
                                                .GetPagedAsync(filters.Page, filters.PageSize, expression, priceExp),
                    SortEnum.PRICE_DESC => await _productRepository
                                               .GetPagedOrderByDescendingAsync(filters.Page, filters.PageSize, expression, priceExp),
                    SortEnum.NEWEST => await _productRepository
                                               .GetPagedOrderByDescendingAsync(filters.Page, filters.PageSize, expression, e => e.CreatedAt),
                    _ => await _productRepository
                                               .GetPagedOrderByDescendingAsync(filters.Page, filters.PageSize, expression, e => e.CreatedAt),
                };
                var res = _mapper.Map<IEnumerable<ProductDTO>>(products).Select(x =>
                {
                    var image = products.Single(e => e.Id == x.Id).Images.FirstOrDefault();
                    if (image != null)
                    {
                        x.ImageUrl = image.ImageUrl;
                    }
                    return x;
                });

                return new PagedResponse<ProductDTO>
                {
                    Items = res,
                    Page = filters.Page,
                    PageSize = filters.PageSize,
                    TotalItems = totalProduct
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<ProductDetailsResponse> GetProductAsync(int id)
        {
            var product = await _productRepository.SingleOrDefaultAsyncInclude(e => e.Id == id);
            if (product != null)
            {
                var res = _mapper.Map<ProductDetailsResponse>(product);
                res.MaterialIds = product.Materials.Select(e => e.MaterialId);

                res.ColorSizes = _mapper.Map<IEnumerable<ColorSizeResponse>>(product.ProductColors);

                res.ImageUrls = product.Images.Select(e => e.ImageUrl);

                return res;
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<ProductDTO> UpdateProductAsync(int id, ProductRequest request, IFormFileCollection images)
        {
            var product = await _productRepository.SingleOrDefaultAsyncInclude(e => e.Id == id);
            if (product != null)
            {
                using var transaction = await _transactionRepository.BeginTransactionAsync();
                try
                {
                    product.Name = request.Name;
                    product.Description = request.Description;
                    product.Price = request.Price;
                    product.Gender = request.Gender;
                    product.CategoryId = request.CategoryId;
                    product.BrandId = request.BrandId;
                    product.Enable = request.Enable;
                    product.DiscountPercent = request.DiscountPercent;

                    var productPath = path + "/" + product.Id;

                    List<string> colorFileNames = new();
                    List<IFormFile> colorImages = new();
                    List<ProductSize> productSizes = new();

                    List<ProductColor> pColorDelete = new();
                    var oldProductColors = await _productColorRepository.GetAsync(e => e.ProductId == product.Id);

                    var oldColor = request.ColorSizes.Select(e => e.Id);
                    if (oldColor == null || !oldColor.Any())
                    {
                        pColorDelete.AddRange(oldProductColors);
                    }
                    else
                    {
                        var colorDel = oldProductColors.Where(old => !request.ColorSizes.Select(e => e.Id).Contains(old.Id));
                        pColorDelete.AddRange(colorDel);

                        //cập nhật số lượng size cũ
                        var oldIds = request.ColorSizes.Where(e => e.Id != null).Select(e => e.Id);
                        var colorUpdate = oldProductColors.Where(old => oldIds.Contains(old.Id));

                        foreach (var color in colorUpdate)
                        {
                            var matchingColor = request.ColorSizes.Single(e => e.Id == color.Id);
                            foreach (var size in color.ProductSizes)
                            {
                                var matchingSize = matchingColor.SizeInStocks.Single(s => s.SizeId == size.SizeId);
                                size.InStock = matchingSize.InStock;
                            }
                        }

                    }
                    //xóa màu
                    _fileStorage.Delete(pColorDelete.Select(e => e.ImageUrl));
                    await _productColorRepository.DeleteRangeAsync(pColorDelete);

                    //thêm màu
                    var newColorImage = request.ColorSizes.Where(e => e.Image != null && e.Id == null);
                    if (newColorImage.Any())
                    {
                        foreach (var color in newColorImage)
                        {
                            var name = "";
                            if (color.Image != null)
                            {
                                name = Guid.NewGuid().ToString() + Path.GetExtension(color.Image.FileName);
                                colorFileNames.Add(name);
                                colorImages.Add(color.Image);
                            }

                            var productColor = new ProductColor
                            {
                                ColorName = color.ColorName,
                                ProductId = product.Id,
                                ImageUrl = Path.Combine(productPath, name)
                            };

                            await _productColorRepository.AddAsync(productColor);

                            var sizes = color.SizeInStocks.Select(size =>
                            {
                                return new ProductSize
                                {
                                    ProductColorId = productColor.Id,
                                    SizeId = size.SizeId,
                                    InStock = size.InStock,
                                };
                            });
                            productSizes.AddRange(sizes);
                        }
                        await _productSizeRepository.AddAsync(productSizes);
                        await _fileStorage.SaveAsync(productPath, colorImages, colorFileNames);
                    }

                    var pMaterials = await _productMaterialRepository.GetAsync(e => e.ProductId == product.Id);
                    await _productMaterialRepository.DeleteRangeAsync(pMaterials);

                    var productMaterials = request.MaterialIds.Select(e => new ProductMaterial
                    {
                        ProductId = id,
                        MaterialId = e
                    });
                    await _productMaterialRepository.AddAsync(productMaterials);

                    List<Image> imageDelete = new();
                    var oldImgs = await _imageRepository.GetImageByProductIdAsync(id);
                    if (request.ImageUrls == null || !request.ImageUrls.Any())
                    {
                        imageDelete.AddRange(oldImgs);
                    }
                    else
                    {
                        var imgsToDelete = oldImgs.Where(old => !request.ImageUrls.Contains(old.ImageUrl));
                        imageDelete.AddRange(imgsToDelete);
                    }
                    _fileStorage.Delete(imageDelete.Select(e => e.ImageUrl));
                    await _imageRepository.DeleteRangeAsync(imageDelete);

                    if (images.Count > 0)
                    {
                        List<string> fileNames = new();
                        var imgs = images.Select(file =>
                        {
                            var name = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            fileNames.Add(name);
                            var image = new Image()
                            {
                                ProductId = id,
                                ImageUrl = Path.Combine(productPath, name),
                            };
                            return image;
                        });
                        await _imageRepository.AddAsync(imgs);
                        await _fileStorage.SaveAsync(productPath, images, fileNames);
                    }

                    await _productRepository.UpdateAsync(product);
                    await transaction.CommitAsync();
                    return _mapper.Map<ProductDTO>(product);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.InnerException?.Message ?? ex.Message);
                }
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<bool> UpdateProductEnableAsync(int id, UpdateEnableRequest request)
        {
            var product = await _productRepository.FindAsync(id);
            if (product != null)
            {
                product.Enable = request.Enable;
                await _productRepository.UpdateAsync(product);
                return product.Enable;
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.FindAsync(id);
            if (product != null)
            {
                var images = await _imageRepository.GetImageByProductIdAsync(id);
                var colorImages = await _productColorRepository.GetAsync(e => e.ProductId == id);

                var deleteList = colorImages.Select(e => e.ImageUrl).ToList();
                deleteList.AddRange(images.Select(e => e.ImageUrl));

                _fileStorage.Delete(deleteList);
                await _productRepository.DeleteAsync(product);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }
    }
}
