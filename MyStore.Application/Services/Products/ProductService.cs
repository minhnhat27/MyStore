using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.Admin.Request;
using MyStore.Application.Admin.Response;
using MyStore.Application.DTO;
using MyStore.Application.ICaching;
using MyStore.Application.IRepository;
using MyStore.Application.IRepository.Products;
using MyStore.Application.IStorage;
using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductSizeRepository _productSizeRepository;
        private readonly IProductMaterialRepository _productMaterialRepository;
        private readonly IImageRepository _imageRepository;
        private readonly IFileStorage _fileStorage;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        private readonly string path = "assets/images/products";

        public ProductService(IProductRepository productRepository, IProductSizeRepository productSizeRepository,
            IProductMaterialRepository productMaterialRepository, IImageRepository imageRepository, IFileStorage fileStorage, 
            ITransactionRepository transactionRepository, ICache productCache, IMapper mapper)
        {
            _productRepository = productRepository;
            _productSizeRepository = productSizeRepository;
            _productMaterialRepository = productMaterialRepository;
            _imageRepository = imageRepository;
            _fileStorage = fileStorage;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<ProductDTO> CreateProductAsync(ProductRequest request, IFormFileCollection images)
        {
            using (var transaction = await _transactionRepository.BeginTransactionAsync())
            {
                try
                {
                    var product = _mapper.Map<Product>(request);
                    await _productRepository.AddAsync(product);

                    var sizes = _mapper.Map<IEnumerable<ProductSize>>(request.SizesAndQuantities)
                        .Select(size =>
                        {
                            size.ProductId = product.Id;
                            return size;
                        });
                    await _productSizeRepository.AddAsync(sizes);

                    var materials = request.MaterialIds.Select(id => new ProductMaterial
                    {
                        MaterialId = id,
                        ProductId = product.Id,
                    });
                    await _productMaterialRepository.AddAsync(materials);

                    IList<string> fileNames = new List<string>();
                    var imgs = images.Select(file =>
                    {
                        var name = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        fileNames.Add(name);
                        var image = new Image()
                        {
                            ProductId = product.Id,
                            ImageUrl = Path.Combine(path, name),
                        };
                        return image;
                    });
                    await _imageRepository.AddAsync(imgs);
                    await _fileStorage.SaveAsync(path, images, fileNames);

                    await transaction.CommitAsync();

                    var res = _mapper.Map<ProductDTO>(product);
                    var image = await _imageRepository.GetFirstImageByProductIdAsync(product.Id);
                    if (image != null)
                    {
                        res.ImageUrl = image.ImageUrl;
                    }
                    return res;
                }
                catch(Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.InnerException?.Message ?? ex.Message);
                }
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
                    products = await _productRepository.GetPagedAsync(page, pageSize);
                }
                else
                {
                    totalProduct = await _productRepository.CountAsync(keySearch);
                    products = await _productRepository.GetPagedAsync(page, pageSize, keySearch);
                }

                var res = _mapper.Map<IEnumerable<ProductDTO>>(products);
                foreach (var product in res)
                {
                    var image = await _imageRepository.GetFirstImageByProductIdAsync(product.Id);
                    if (image != null)
                    {
                        product.ImageUrl = image.ImageUrl;
                    }
                }

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

        //public async Task<PagedResponse<ProductDTO>> GetFilterProductsAsync(Filters filters)
        //{
        //    try
        //    {
        //        var totalProduct = await _productRepository.CountAsync(keySearch);
        //        var products = await _productRepository.GetPagedAsync(page, pageSize, keySearch);

        //        var res = _mapper.Map<IEnumerable<ProductDTO>>(products);
        //        foreach (var product in res)
        //        {
        //            var image = await _imageRepository.GetFirstImageByProductIdAsync(product.Id);
        //            if (image != null)
        //            {
        //                product.ImageUrl = image.ImageUrl;
        //            }
        //        }

        //        return new PagedResponse<ProductDTO>
        //        {
        //            Items = res,
        //            Page = page,
        //            PageSize = pageSize,
        //            TotalItems = totalProduct
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.InnerException?.Message ?? ex.Message);
        //    }
        //}


        public async Task<ProductDetailResponse> GetProductAsync(int id)
        {
            var product = await _productRepository.SingleOrDefaultAsync(id);
            if (product != null)
            {
                var res = _mapper.Map<ProductDetailResponse>(product);
                res.MaterialIds = product.Materials.Select(e => e.MaterialId);
                res.SizesAndQuantities = _mapper.Map<IEnumerable<SizeAndQuantity>>(product.Sizes);
                res.SizeIds = product.Sizes.Select(e => e.SizeId);
                res.ImageUrls = product.Images.Select(e => e.ImageUrl);

                return res;
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<ProductDTO> UpdateProductAsync(int id, ProductRequest request, IFormFileCollection images)
        {
            var product = await _productRepository.FindAsync(id);
            if (product != null)
            {
                using (var transaction = await _transactionRepository.BeginTransactionAsync())
                {
                    try
                    {
                        product.Name = request.Name;
                        product.Description = request.Description;
                        product.Price = request.Price;
                        product.Gender = request.Gender;
                        product.CategoryId = request.CategoryId;
                        product.BrandId = request.BrandId;
                        product.Enable = request.Enable;

                        var productSizes = _mapper.Map<IEnumerable<ProductSize>>(request.SizesAndQuantities)
                            .Select(size =>
                            {
                                size.ProductId = id;
                                return size;
                            });
                        await _productSizeRepository.DeleteAllByProductIdAsync(id);
                        await _productSizeRepository.AddAsync(productSizes);

                        var productMaterials = request.MaterialIds.Select(e => new ProductMaterial
                        {
                            ProductId = id,
                            MaterialId = e
                        });

                        await _productMaterialRepository.DeleteAllByProductIdAsync(id);
                        await _productMaterialRepository.AddAsync(productMaterials);


                        var oldImgs = await _imageRepository.GetImageByProductIdAsync(id);
                        
                        List<Image> imageDelete = new();
                        if(request.ImageUrls == null || request.ImageUrls.Count() < 1)
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

                        if(images.Count > 0)
                        {
                            List<string> fileNames = new();
                            var imgs = images.Select(file =>
                            {
                                var name = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                fileNames.Add(name);
                                var image = new Image()
                                {
                                    ProductId = id,
                                    ImageUrl = Path.Combine(path, name),
                                };
                                return image;
                            });
                            await _imageRepository.AddAsync(imgs);
                            await _fileStorage.SaveAsync(path, images, fileNames);
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
                _fileStorage.Delete(images.Select(e => e.ImageUrl));
                await _productRepository.DeleteAsync(product);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }
    }
}
