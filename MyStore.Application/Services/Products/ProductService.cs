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
using OpenCvSharp;
using MyStore.Application.Services.FlashSales;

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

        private readonly string path = "assets/images/products";

        public ProductService(IProductRepository productRepository,
                              IProductSizeRepository productSizeRepository,
                              IFlashSaleService flashSaleService,
                              IProductColorRepository productColorRepository,
                              IProductReviewRepository productReviewRepository,
                              IProductMaterialRepository productMaterialRepository,
                              IImageRepository imageRepository,
                              IFileStorage fileStorage,
                              ITransactionRepository transactionRepository,
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
                var productFlashSale = (await _flashSaleService.GetFlashSaleProductsThisTime()).Products;

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
                    expression = CombineExpressions(expression, e => e.DiscountPercent > 0
                        || productFlashSale.Select(e => e.Id).Any(x => x == e.Id));

                }
                else
                {
                    if (filters.Discount != null && filters.Discount.Value)
                    {
                        expression = CombineExpressions(expression, e => e.DiscountPercent > 0 ||
                        productFlashSale.Select(e => e.Id).Any(x => x == e.Id));

                    }
                    else if (filters.FlashSale != null && filters.FlashSale.Value && productFlashSale.Any())
                    {
                        expression = CombineExpressions(expression, e => productFlashSale.Select(e => e.Id).Any(x => x == e.Id));
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

        private bool CompareImages(Mat grayInputImage, Mat compareImage, double threshold = 1000.0)
        {
            var gray1 = new Mat();
            var gray2 = new Mat();
            Cv2.CvtColor(compareImage, gray2, ColorConversionCodes.BGR2GRAY);

            return true;
        }

        public async Task<IEnumerable<ProductDTO>> GetSearchProducts(string tempFilePath, string rootPath)
        {
            var imagesPath = Path.Combine(rootPath, path);

            var resultList = new List<string>();
            var inputImage = Cv2.ImRead(tempFilePath);

            if (inputImage.Empty())
            {
                throw new FileNotFoundException(ErrorMessage.NOT_FOUND + " ảnh");
            }
            var grayInputImage = new Mat();
            Cv2.CvtColor(inputImage, grayInputImage, ColorConversionCodes.BGR2GRAY);

            //var gray1 = new Mat();
            //var blur = new Mat();
            //Cv2.CvtColor(inputImage, gray1, ColorConversionCodes.BGR2GRAY);
            //Cv2.GaussianBlur(gray1, blur, new OpenCvSharp.Size(5, 5), 0);
            //var canny = new Mat();
            //Cv2.Canny(blur, canny, 15, 120);
            //Cv2.Dilate(canny, canny, new Mat(), null, 3);
            //Point[][] contours;
            //HierarchyIndex[] hierarchyIndices;

            //Cv2.FindContours(canny, out contours, out hierarchyIndices,
            //    RetrievalModes.External, method: ContourApproximationModes.ApproxSimple);

            //foreach(var contour in contours)
            //{
            //    var rect = Cv2.BoundingRect(contour);
            //    Cv2.Rectangle(inputImage, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height), Scalar.DarkRed, 2);
            //}
            //gray1.Release();
            //canny.Release();
            //blur.Release();
            //try
            //{
            //    Window.ShowImages(inputImage);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

            //var productImagePaths = Directory.GetDirectories(imagesPath)
            //    .SelectMany(dir => Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly))
            //    .ToList();

            //if(productImagePaths != null)
            //{
            //    foreach (var productImagePath in productImagePaths)
            //    {
            //        var productImage = Cv2.ImRead(productImagePath);
            //        if (productImage.Empty())
            //            continue;
            //        if (CompareImages(grayInputImage, productImage))
            //        {
            //            resultList.Add(productImagePath.Split(Path.DirectorySeparatorChar)[^2]);
            //        }
            //    }
            //}

            //var products = await _productRepository.GetPagedAsync(1, 10, e => resultList.Any(x => x.Equals(e.Id)), e => e.Name);
            //var products = await _productRepository
            //    .GetPagedAsync(1, 10, e => resultList.Contains(e.Id.ToString()), e => e.Name);

            //return _mapper.Map<IEnumerable<ProductDTO>>(products);
            throw new NotImplementedException();
        }
        public async Task<ProductDetailsResponse> GetProductAsync(long id)
        {
            var product = await _productRepository.SingleOrDefaultAsyncInclude(e => e.Id == id);
            if (product != null)
            {
                var res = _mapper.Map<ProductDetailsResponse>(product);
                res.ImageUrls = product.Images.Select(e => e.ImageUrl);
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
                    var oldIds = request.ColorSizes.Where(e => e.Id != null).Select(e => e.Id);
                    var colorUpdate = oldProductColors.Where(old => oldIds.Contains(old.Id));

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

                        foreach (var size in color.ProductSizes)
                        {
                            var matchingSize = matchingColor.SizeInStocks.SingleOrDefault(s => s.SizeId == size.SizeId);
                            if (matchingSize == null)
                            {
                                await _productSizeRepository.DeleteAsync(matchingColor.Id, size.SizeId);
                            }
                            else
                            {
                                size.InStock = matchingSize.InStock;
                            }
                        }
                    }
                    if (colorUpdate.Any())
                    {
                        await _productColorRepository.UpdateAsync(colorUpdate);
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
                await _productReviewRepository.DeleteAsync(rv);
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
