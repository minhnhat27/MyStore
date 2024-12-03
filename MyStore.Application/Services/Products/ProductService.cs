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
using System.Text.RegularExpressions;
using System.Text;
using MyStore.Application.Services.FlashSales;
using MyStore.Application.ILibrary;
using Microsoft.Extensions.DependencyInjection;

namespace MyStore.Application.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductSizeRepository _productSizeRepository;
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductMaterialRepository _productMaterialRepository;
        private readonly IProductReviewRepository _productReviewRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ITransactionRepository _transactionRepository;

        private readonly IFlashSaleService _flashSaleService;

        private readonly IFileStorage _fileStorage;
        private readonly IMapper _mapper;

        private readonly string path = Path.Combine("assets", "images", "products");
        private readonly string reviewsPath = Path.Combine("assets", "images", "reviews");
        private readonly string rootPath = Path.Combine(Environment.CurrentDirectory, "wwwroot");

        private readonly IImageFeatureExtractor _imageFeatureExtractor;
        private readonly IProductFeatureRepository _productFeatureRepository;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ProductService(IProductRepository productRepository, IImageFeatureExtractor imageFeatureExtractor,
                              IProductSizeRepository productSizeRepository,
                              IFlashSaleService flashSaleService,
                              IProductColorRepository productColorRepository,
                              IProductReviewRepository productReviewRepository,
                              IProductMaterialRepository productMaterialRepository,
                              IImageRepository imageRepository,
                              IFileStorage fileStorage,
                              ITransactionRepository transactionRepository,
                              IProductFeatureRepository productFeatureRepository,
                              IServiceScopeFactory serviceScopeFactory,
                              IMapper mapper)
        {
            _productRepository = productRepository;
            _productSizeRepository = productSizeRepository;
            _productColorRepository = productColorRepository;
            _productMaterialRepository = productMaterialRepository;
            _productReviewRepository = productReviewRepository;
            _imageRepository = imageRepository;
            _fileStorage = fileStorage;
            _transactionRepository = transactionRepository;
            _mapper = mapper;

            _flashSaleService = flashSaleService;

            _imageFeatureExtractor = imageFeatureExtractor;
            _productFeatureRepository = productFeatureRepository;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<ProductDTO> CreateProductAsync(ProductRequest request, IFormFileCollection images)
        {
            using var transaction = await _transactionRepository.BeginTransactionAsync();
            try
            {
                var product = _mapper.Map<Product>(request);
                await _productRepository.AddAsync(product);
                var productPath = Path.Combine(path, product.Id.ToString());

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
                var features = colorFileNames.Select(name =>
                {
                    var fullPath = Path.Combine(rootPath, productPath, name);
                    var feature = _imageFeatureExtractor.ImageClassificationPrediction(fullPath);
                    if (feature != null )
                    {
                        return new ProductFeature
                        {
                            Label = feature.PredictedLabelValue,
                            Green = feature.Green,
                            Red = feature.Red,
                            Blue = feature.Blue,
                            ProductId = product.Id
                        };
                    }
                    return null;
                });
                var lstProductFeature = features.Where(item => item != null).Distinct();
                if (lstProductFeature != null && lstProductFeature.Any())
                {
                    await _productFeatureRepository.AddAsync(lstProductFeature!);
                }
                await transaction.CommitAsync();

                var res = _mapper.Map<ProductDTO>(product);
                var image = imgs.FirstOrDefault();
                if (image != null)
                {
                    res.ImageUrl = image.ImageUrl;
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
                    var isLong = long.TryParse(keySearch, out long longSearch);
                    keySearch = keySearch.ToLower();

                    Expression<Func<Product, bool>> expression = e =>
                        isLong && e.Id.Equals(longSearch)
                        || e.Name.Contains(keySearch)
                        || e.Brand.Name.ToLower().Contains(keySearch)
                        || e.Category.Name.ToLower().Contains(keySearch)
                        || e.Price.ToString().Equals(keySearch);

                    totalProduct = await _productRepository.CountAsync(expression);
                    products = await _productRepository.GetPagedOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
                }

                var res = _mapper.Map<IEnumerable<ProductDTO>>(products);

                var productFlashSale = (await _flashSaleService.GetFlashSaleProductsThisTime()).Products;
                if (productFlashSale.Any())
                {
                    res = res.Select(e =>
                    {
                        var saleProduct = productFlashSale.FirstOrDefault(s => s.Id == e.Id);
                        if (saleProduct != null)
                        {
                            e.FlashSaleDiscountPercent = saleProduct.FlashSaleDiscountPercent;
                        }
                        return e;
                    });
                }

                return new PagedResponse<ProductDTO>
                {
                    Items = res,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalProduct
                };
            }
            catch (Exception ex)
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
                var fsThisTime = await _flashSaleService.GetFlashSaleProductsThisTime();
                var productFlashSale = fsThisTime.Products ?? [];

                var date = DateTime.Now.Date;

                if (filters.MinPrice != null && filters.MaxPrice != null)
                {
                    var flashsale = _flashSaleService.IsFlashSaleActive();
                    if (productFlashSale != null)
                    {
                        expression = CombineExpressions(expression, e =>
                            (e.Price - (e.Price *
                                (e.ProductFlashSales
                                    .Where(x => x.FlashSale.Date.Date == date 
                                        && x.FlashSale.DiscountTimeFrame == flashsale
                                        && x.ProductId == e.Id)
                                    .FirstOrDefault() != null ?

                                    e.ProductFlashSales
                                    .Where(x => x.FlashSale.Date.Date == date
                                        && x.FlashSale.DiscountTimeFrame == flashsale
                                        && x.ProductId == e.Id)
                                    .First().DiscountPercent

                                    : e.DiscountPercent) / 100.0
                            )) >= filters.MinPrice
                            &&
                            (e.Price - (e.Price *
                                (e.ProductFlashSales
                                    .Where(x => x.FlashSale.Date.Date == date
                                        && x.FlashSale.DiscountTimeFrame == flashsale
                                        && x.ProductId == e.Id)
                                    .FirstOrDefault() != null ?

                                    e.ProductFlashSales
                                    .Where(x => x.FlashSale.Date.Date == date
                                        && x.FlashSale.DiscountTimeFrame == flashsale
                                        && x.ProductId == e.Id)
                                    .First().DiscountPercent

                                    : e.DiscountPercent) / 100.0
                            )) <= filters.MaxPrice
                        );
                    }
                    else
                    {
                        expression = CombineExpressions(expression, e =>
                            (e.Price - (e.Price * (e.DiscountPercent / 100.0))) >= filters.MinPrice
                            &&
                            (e.Price - (e.Price * (e.DiscountPercent / 100.0))) <= filters.MaxPrice
                        );
                    }

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

                if (!string.IsNullOrEmpty(filters.Key))
                {
                    var inputWords = filters.Key.Trim().Split(' ').Select(word => word.ToLower());

                    expression = CombineExpressions(expression, e => inputWords.All(word => e.Name.ToLower().Contains(word)));
                }

                if (filters.Discount != null && filters.Discount.Value 
                    && filters.FlashSale != null && filters.FlashSale.Value)
                {
                    if(productFlashSale != null)
                    {
                        expression = CombineExpressions(expression, e => e.DiscountPercent > 0
                        || productFlashSale.Select(e => e.Id).Any(x => x == e.Id));
                    }
                    else expression = CombineExpressions(expression, e => e.DiscountPercent > 0);

                }
                else
                {
                    if (filters.Discount != null && filters.Discount.Value)
                    {
                        if (productFlashSale != null)
                        {
                            expression = CombineExpressions(expression, e => e.DiscountPercent > 0
                            || productFlashSale.Select(e => e.Id).Any(x => x == e.Id));
                        }
                        else expression = CombineExpressions(expression, e => e.DiscountPercent > 0);

                    }
                    else if (filters.FlashSale != null && filters.FlashSale.Value)
                    {
                        if (productFlashSale != null && productFlashSale.Any())
                        {
                            expression = CombineExpressions(expression, e => productFlashSale.Select(e => e.Id).Any(x => x == e.Id));
                        }
                        else expression = CombineExpressions(expression, e => false);
                    }
                }

                

                if (filters.Rating != null)
                {
                    expression = CombineExpressions(expression, e => e.Rating >= filters.Rating);
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
                var res = _mapper.Map<IEnumerable<ProductDTO>>(products);

                if (productFlashSale != null && productFlashSale.Any())
                {
                    res = res.Select(e =>
                    {
                        var saleProduct = productFlashSale.FirstOrDefault(s => s.Id == e.Id);
                        if (saleProduct != null)
                        {
                            e.FlashSaleDiscountPercent = saleProduct.FlashSaleDiscountPercent;
                        }
                        return e;
                    });
                }

                return new PagedResponse<ProductDTO>
                {
                    Items = res,
                    Page = filters.Page,
                    PageSize = filters.PageSize,
                    TotalItems = totalProduct
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PagedResponse<ProductDTO>> GetGetFeaturedProductsAsync(int page, int pageSize)
        {
            var products = await _productRepository
                .GetPagedOrderByDescendingAsync(page, pageSize, e => e.Enable,
                //x => x.ProductReviews.Count != 0 ? x.ProductReviews.Select(pR => pR.Star).Average() : 0);
                x => x.ProductFavorites.Count);
            var total = await _productRepository.CountAsync(e => e.Enable);

            var res = _mapper.Map<IEnumerable<ProductDTO>>(products);

            var productFlashSale = (await _flashSaleService.GetFlashSaleProductsThisTime()).Products;
            if (productFlashSale.Any())
            {
                res = res.Select(e =>
                {
                    var saleProduct = productFlashSale.FirstOrDefault(s => s.Id == e.Id);
                    if (saleProduct != null)
                    {
                        e.FlashSaleDiscountPercent = saleProduct.FlashSaleDiscountPercent;
                    }
                    return e;
                });
            }

            return new PagedResponse<ProductDTO>
            {
                Items = res,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public static string RemoveVietnameseTones(string str)
        {
            string normalizedString = str.Normalize(NormalizationForm.FormD);

            var regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string withoutTones = regex.Replace(normalizedString, string.Empty);

            return withoutTones.Replace('đ', 'd').Replace('Đ', 'D');
        }

        private bool IsMatchingSearchCriteriaWithoutTones(string productName, IEnumerable<string> inputWords)
        {
            var normalizedProductName = RemoveVietnameseTones(productName).ToLower();
            return inputWords
                .Select(word => RemoveVietnameseTones(word.ToLower()))
                .All(normalizedProductName.Contains);
        }

        public async Task<IEnumerable<ProductDTO>> GetSearchProducts(string key)
        {
            //áo thun co tron
            var inputWords = key.Trim().Split(' ').Select(word => word.ToLower());

            var products = await _productRepository.GetPagedOrderByDescendingAsync(1, 5,
                e => inputWords.All(word => e.Name.ToLower().Contains(word)), e => e.Sold);

            if (!products.Any())
            {
                var productList = await _productRepository.GetPagedOrderByDescendingAsync(1, 20, null, e => e.Sold);
                products = productList.Where(e => IsMatchingSearchCriteriaWithoutTones(e.Name, inputWords)).Take(5);
            }
            var res = _mapper.Map<IEnumerable<ProductDTO>>(products);

            var productFlashSale = (await _flashSaleService.GetFlashSaleProductsThisTime()).Products;
            if (productFlashSale.Any())
            {
                res = res.Select(e =>
                {
                    var saleProduct = productFlashSale.FirstOrDefault(s => s.Id == e.Id);
                    if (saleProduct != null)
                    {
                        e.FlashSaleDiscountPercent = saleProduct.FlashSaleDiscountPercent;
                    }
                    return e;
                });
            }

            return res;
        }

        public async Task<IEnumerable<ProductDTO>> GetSearchProductsByImage(string tempFilePath)
        {
            var features = _imageFeatureExtractor.ImageClassificationPrediction(tempFilePath);
            IEnumerable<Product> products = [];
            if(features != null)
            {
                double maxColorDeviation = 10.0;

                var productFeatures = await _productFeatureRepository.GetAsync(e =>
                     e.Label == features.PredictedLabelValue &&
                     Math.Abs(e.Red - features.Red) <= maxColorDeviation &&
                     Math.Abs(e.Green - features.Green) <= maxColorDeviation &&
                     Math.Abs(e.Blue - features.Blue) <= maxColorDeviation
                 );

                if (!productFeatures.Any())
                {
                    maxColorDeviation = 15.0;
                    productFeatures = await _productFeatureRepository.GetAsync(e =>
                         //e.Label == features.PredictedLabelValue 
                         (Math.Abs(e.Red - features.Red) <= maxColorDeviation &&
                         Math.Abs(e.Green - features.Green) <= maxColorDeviation &&
                         Math.Abs(e.Blue - features.Blue) <= maxColorDeviation)
                     );
                }
                var productFeatureIds = productFeatures.Select(e => e.ProductId);
                products = await _productRepository
                    .GetPagedAsync(1, 10, product => productFeatureIds.Contains(product.Id), e => e.Name);
            }
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }
        
        public async Task<ProductDetailsResponse> GetProductAsync(long id)
        {
            var product = await _productRepository.SingleOrDefaultAsyncInclude(e => e.Id == id);
            if (product != null)
            {
                var res = _mapper.Map<ProductDetailsResponse>(product);
                res.ImageUrls = product.Images.OrderBy(e => e.CreatedAt).Select(e => e.ImageUrl);
                res.MaterialNames = product.Materials.Select(e => e.Material.Name);

                var newDiscount = await _flashSaleService.GetDiscountByProductIdThisTime(id);
                if (newDiscount != null)
                {
                    res.FlashSaleDiscountPercent = newDiscount.Value;
                    res.EndFlashSale = _flashSaleService.GetEndFlashSale();
                }

                return res;
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<ProductDTO> UpdateProductAsync(long id, ProductRequest request, IFormFileCollection images)
        {
            var product = await _productRepository.SingleOrDefaultAsyncInclude(e => e.Id == id)
                ?? throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
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

                var productPath = Path.Combine(path, product.Id.ToString());

                List<string> colorFileNames = new();
                List<IFormFile> colorImages = new();

                List<string> listImageDelete = new();

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

                    //cập nhật số lượng size cũ, màu cũ
                    var oldColorIds = request.ColorSizes.Where(e => e.Id != null).Select(e => e.Id);
                    var colorUpdate = oldProductColors.Where(old => oldColorIds.Contains(old.Id));

                    foreach (var color in colorUpdate)
                    {
                        var matchingColor = request.ColorSizes.Single(e => e.Id == color.Id);
                        var newImage = request.ColorSizes.SingleOrDefault(e => e.Id == color.Id);
                        if (newImage != null)
                        {
                            color.ColorName = newImage.ColorName;
                            if (newImage.Image != null)
                            {
                                var name = "";
                                name = Guid.NewGuid().ToString() + Path.GetExtension(newImage.Image.FileName);
                                colorFileNames.Add(name);
                                colorImages.Add(newImage.Image);
                                listImageDelete.Add(color.ImageUrl);

                                color.ImageUrl = Path.Combine(productPath, name);
                            }
                        }


                        var newSizes = matchingColor.SizeInStocks.Select(s => s.SizeId);
                        var oldSizes = color.ProductSizes.Select(s => s.SizeId);

                        var lstNewSize = newSizes.Except(oldSizes).Select(sizeId =>
                        {
                            var matchingSize = matchingColor.SizeInStocks.Single(s => s.SizeId == sizeId);
                            return new ProductSize
                            {
                                ProductColorId = color.Id,
                                SizeId = sizeId,
                                InStock = matchingSize.InStock
                            };
                        });

                        foreach (var size in color.ProductSizes)
                        {
                            if (!newSizes.Contains(size.SizeId))
                            {
                                await _productSizeRepository.DeleteAsync(matchingColor.Id, size.SizeId);
                            }
                            else
                            {
                                var matchingSize = matchingColor.SizeInStocks.SingleOrDefault(s => s.SizeId == size.SizeId);
                                if (matchingSize != null)
                                {
                                    size.InStock = matchingSize.InStock;
                                }
                            }
                        }
                        if (lstNewSize.Any())
                        {
                            await _productSizeRepository.AddAsync(lstNewSize);
                        }
                    }
                    if (colorUpdate.Any())
                    {
                        await _productColorRepository.UpdateAsync(colorUpdate);
                    }
                }
                //xóa màu
                //listImageDelete.AddRange(pColorDelete.Select(e => e.ImageUrl));
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
                if (imageDelete.Any())
                {
                    listImageDelete.AddRange(imageDelete.Select(e => e.ImageUrl));
                    await _imageRepository.DeleteRangeAsync(imageDelete);
                }

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

                if (listImageDelete.Any())
                {
                    _fileStorage.Delete(listImageDelete);
                }

                if (colorImages.Any())
                {
                    await _fileStorage.SaveAsync(productPath, colorImages, colorFileNames);
                }

                await _productRepository.UpdateAsync(product);
                await transaction.CommitAsync();
                return _mapper.Map<ProductDTO>(product);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateProductEnableAsync(long id, UpdateEnableRequest request)
        {
            var product = await _productRepository.FindAsync(id)
                ?? throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);

            product.Enable = request.Enable;
            await _productRepository.UpdateAsync(product);
            return product.Enable;
        }

        public async Task DeleteProductAsync(long id)
        {
            var product = await _productRepository.FindAsync(id)
                ?? throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);

            var images = await _imageRepository.GetImageByProductIdAsync(id);
            List<string> imageDeletes = images.Select(e => e.ImageUrl).ToList();

            var reviewImagesPath = Path.Combine(reviewsPath, product.Id.ToString());

            //var reviewImages = await _productReviewRepository.GetAsync(e => e.ProductId == id);
            //imageDeletes.AddRange(reviewImages
            //    .Where(e => e.ImagesUrls != null && e.ImagesUrls.Any())
            //    .SelectMany(e => e.ImagesUrls));

            //ko xoa ảnh của màu do detail của khách hàng
            //var colorImages = await _productColorRepository.GetAsync(e => e.ProductId == id);
            //var deleteList = colorImages.Select(e => e.ImageUrl).ToList();
            //deleteList.AddRange(images.Select(e => e.ImageUrl));

            await _productRepository.DeleteAsync(product);
            _fileStorage.Delete(imageDeletes);
            _fileStorage.DeleteDirectory(reviewImagesPath);
        }

        private string MaskUsername(string username)
        {
            var words = username.Split(" ");
            return string.Join(" ", words.Select(x =>
            {
                var trimmedWord = x.Trim();
                if (trimmedWord.Length > 1)
                {
                    return $"{trimmedWord[0]}{new string('*', trimmedWord.Length - 1)}";
                }
                return trimmedWord;
            }));
        }

        public async Task<PagedResponse<ReviewDTO>> GetReviews(long id, ReviewFiltersRequest request)
        {
            Expression<Func<ProductReview, bool>> expression = e => e.ProductId == id;

            expression = request.Rate switch
            {
                ReviewFiltersEnum.ALL => expression,
                ReviewFiltersEnum.HAVEPICTURE
                    => CombineExpressions(expression, e => e.ImagesUrlsJson != null),
                ReviewFiltersEnum.HAVECOMMENT
                    => CombineExpressions(expression, e => e.Description != null && e.Description.Length > 0),

                _ => CombineExpressions(expression, e => e.Star == (int)request.Rate)
            };

            var reviews = await _productReviewRepository
                .GetPagedOrderByDescendingAsync(request.Page, request.PageSize, expression, e => e.CreatedAt);

            var total = await _productReviewRepository.CountAsync(expression);

            var items = _mapper.Map<IEnumerable<ReviewDTO>>(reviews);

            return new PagedResponse<ReviewDTO>
            {
                Items = items,
                TotalItems = total,
                Page = request.Page,
                PageSize = request.PageSize,
            };
        }

        public async Task DeleteReview(string id)
        {
            var rv = await _productReviewRepository.FindAsync(id)
                 ?? throw new ArgumentException("Id " + ErrorMessage.NOT_FOUND);

            using var transaction = await _transactionRepository.BeginTransactionAsync();
            try
            {
                var product = await _productRepository.FindAsync(rv.ProductId);
                if (product != null)
                {
                    var currentStar = product.Rating * product.RatingCount;
                    product.Rating = (currentStar - rv.Star) / (product.RatingCount - 1);
                    product.RatingCount -= 1;

                    await _productRepository.UpdateAsync(product);
                }
                if(rv.ImagesUrls != null && rv.ImagesUrls.Any())
                {
                    _fileStorage.Delete(rv.ImagesUrls);
                }
                await _productReviewRepository.DeleteAsync(rv);
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RetrainForAllProduct()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
            var productFeatureRepository = scope.ServiceProvider.GetRequiredService<IProductFeatureRepository>();
            var imageRepository = scope.ServiceProvider.GetRequiredService<IImageRepository>();
            //var productColorRepository = scope.ServiceProvider.GetRequiredService<IProductColorRepository>();
            var imageFeatureExtractor = scope.ServiceProvider.GetRequiredService<IImageFeatureExtractor>();

            using var scope1 = _serviceScopeFactory.CreateScope();
            var productColorRepository = scope1.ServiceProvider.GetRequiredService<IProductColorRepository>();

            var transaction = await transactionRepository.BeginTransactionAsync();
            try
            {
                var taskImages = imageRepository.GetAllAsync();
                var taskProductColors = productColorRepository.GetAllAsync();
                await Task.WhenAll(taskImages, taskProductColors);

                var imageColors = (await taskProductColors).Select(e => e.ImageUrl);
                var images = (await taskImages).Select(e => e.ImageUrl).ToList();
                images.AddRange(imageColors);

                var features = images.Select(url =>
                {
                    var fullPath = Path.Combine(rootPath, url);
                    var feature = imageFeatureExtractor.ImageClassificationPrediction(fullPath);

                    if (feature != null)
                    {
                        string id = url.Split(['/', '\\'])[3];
                        return new ProductFeature
                        {
                            Label = feature.PredictedLabelValue,
                            Green = feature.Green,
                            Red = feature.Red,
                            Blue = feature.Blue,
                            ProductId = long.Parse(id),
                        };
                    }
                    return null;
                });

                var lstProductFeature = features.Where(item => item != null).Distinct();
                if (lstProductFeature != null && lstProductFeature.Any())
                {
                    await productFeatureRepository.DeleteAll();
                    await productFeatureRepository.AddAsync(lstProductFeature!);
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Transaction failed: {ex.Message}");
            }
        }
    }
}
