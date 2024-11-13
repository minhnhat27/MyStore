using AutoMapper;
using MyStore.Application.DTOs;
using MyStore.Application.IRepositories;
using MyStore.Application.IRepositories.Products;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Services.FlashSales
{
    public class FlashSaleService : IFlashSaleService
    {
        private readonly IFlashSaleRepository _flashSaleRepository;
        private readonly IProductFlashSaleRepository _productFlashSaleRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        public FlashSaleService(IFlashSaleRepository flashSaleRepository, IMapper mapper,
            IProductRepository productRepository,
            IProductFlashSaleRepository productFlashSaleRepository,
            ITransactionRepository transactionRepository)
        {
            _flashSaleRepository = flashSaleRepository;
            _productFlashSaleRepository = productFlashSaleRepository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<FlashSaleDTO>> GetFlashSales(PageRequest request)
        {
            var flashsales = await _flashSaleRepository
                .GetPagedOrderByDescendingAsync(request.Page, request.PageSize, null, e => e.CreatedAt);
            var total = await _flashSaleRepository.CountAsync();

            var items = _mapper.Map<IEnumerable<FlashSaleDTO>>(flashsales);
            return new PagedResponse<FlashSaleDTO>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalItems = total,
                Items = items
            };
        }
        public async Task<IEnumerable<ProductDTO>> GetProductsByFlashSale(string id)
        {
            var productFlashSales = await _productFlashSaleRepository
                .GetAsync(e => e.FlashSaleId == id && e.Product.Enable);
            var products = productFlashSales.Select(e => e.Product);

            var res = _mapper.Map<IEnumerable<ProductDTO>>(products)
                .Select(e =>
                {
                    var productFlashSale = productFlashSales
                        .SingleOrDefault(p => p.ProductId == e.Id && p.FlashSaleId == id);

                    e.FlashSaleDiscountPercent = productFlashSale?.DiscountPercent ?? 0;
                    return e;
                });
            return res;
        }
        public async Task<IEnumerable<ProductDTO>> GetProductsByTimeFrame(DiscountTimeFrame timeFrame)
        {
            var date = DateTime.Now.Date;

            var flashSale = await _flashSaleRepository
                     .SingleOrDefaultAsync(e => e.Date == date && e.DiscountTimeFrame == timeFrame);
            if (flashSale != null)
            {
                var productFlashSales = await _productFlashSaleRepository
                    .GetAsync(e => e.FlashSaleId == flashSale.Id && e.Product.Enable);
                var products = productFlashSales.Select(e => e.Product);

                var res = _mapper.Map<IEnumerable<ProductDTO>>(products).
                    Select(e =>
                    {
                        var productFlashSale = productFlashSales
                            .SingleOrDefault(p => p.ProductId == e.Id && p.FlashSaleId == flashSale.Id);

                        e.FlashSaleDiscountPercent = productFlashSale?.DiscountPercent ?? 0;
                        return e;
                    });
                return res;
            }
            return [];
        }
        public async Task<FlashSaleResponse> GetFlashSaleProductsThisTime()
        {
            var date = DateTime.Now.Date;
            DiscountTimeFrame? timeFrame = IsFlashSaleActive();
            IEnumerable<ProductDTO> productDTOs = new List<ProductDTO>();

            if (timeFrame != null)
            {
                var flashSale = await _flashSaleRepository
                    .SingleOrDefaultAsync(e => e.Date == date && e.DiscountTimeFrame == timeFrame);
                if (flashSale != null)
                {
                    var productFlashSales = await _productFlashSaleRepository
                        .GetAsync(e => e.FlashSaleId == flashSale.Id && e.Product.Enable);
                    var p = productFlashSales.Select(e => e.Product);

                    productDTOs = _mapper.Map<IEnumerable<ProductDTO>>(p)
                    .Select(e =>
                    {
                        var productFlashSale = productFlashSales
                            .SingleOrDefault(p => p.ProductId == e.Id && p.FlashSaleId == flashSale.Id);

                        e.FlashSaleDiscountPercent = productFlashSale?.DiscountPercent ?? 0;
                        return e;
                    });
                }
            }
            var endFlashSale = GetEndFlashSale();

            return new FlashSaleResponse
            {
                Products = productDTOs,
                EndFlashSale = endFlashSale,
            };
        }

        public async Task<IEnumerable<ProductDiscountPercentWithId>> GetFlashSaleProductsWithDiscountThisTime()
        {
            var date = DateTime.Now.Date;
            DiscountTimeFrame? timeFrame = IsFlashSaleActive();

            if (timeFrame != null)
            {
                var flashSale = await _flashSaleRepository
                    .SingleOrDefaultAsync(e => e.Date == date && e.DiscountTimeFrame == timeFrame);
                if (flashSale != null)
                {
                    var productFlashSales = await _productFlashSaleRepository.GetAsync(e => e.FlashSaleId == flashSale.Id);
                    var products = productFlashSales.Select(e =>
                    {
                        var fs = productFlashSales
                                .SingleOrDefault(p => p.ProductId == e.ProductId && p.FlashSaleId == flashSale.Id);
                        return new ProductDiscountPercentWithId
                        {
                            ProductId = e.ProductId,
                            DiscountPercent = fs?.DiscountPercent ?? 0,
                            FlashSaleId = e.FlashSaleId
                        };
                    });
                    return products;
                }
            }
            return [];
        }

        public DiscountTimeFrame? IsFlashSaleActive()
        {
            var hours = DateTime.Now.Hour;

            //test
            //return DiscountTimeFrame.TO19FROM22;

            return hours switch
            {
                >= 0 and < 2 => DiscountTimeFrame.TO00FROM2,
                >= 10 and < 12 => DiscountTimeFrame.TO10FROM12,
                >= 19 and < 22 => DiscountTimeFrame.TO19FROM22,
                _ => null
            };
        }

        public DateTime? GetEndFlashSale()
        {
            var date = DateTime.Now.Date.AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
            var hours = DateTime.Now.Hour;

            //test
            //return date.AddHours(23);
            //return null;

            return hours switch
            {
                >= 0 and < 2 => date.AddHours(1),
                >= 10 and < 12 => date.AddHours(11),
                >= 19 and < 22 => date.AddHours(21),
                _ => null
            };
        }

        public async Task<FlashSaleDTO> CreateFlashSale(FlashSaleRequest request)
        {
            using var transaction = await _transactionRepository.BeginTransactionAsync();
            try
            {
                var exist = await _flashSaleRepository
                                .GetAsync(e => e.Date.Date == request.Date.Date
                                        && e.DiscountTimeFrame == request.DiscountTimeFrame);
                if (exist.Any())
                {
                    throw new InvalidDataException(ErrorMessage.FLASHSALE_EXISTED);
                }
                var flashSale = new FlashSale
                {
                    Date = request.Date,
                    DiscountTimeFrame = request.DiscountTimeFrame,
                };
                await _flashSaleRepository.AddAsync(flashSale);
                var productFlashSale = request.ProductFlashSales.Select(e => new ProductFlashSale
                {
                    DiscountPercent = e.DiscountPercent,
                    ProductId = e.ProductId,
                    FlashSaleId = flashSale.Id
                });
                await _productFlashSaleRepository.AddAsync(productFlashSale);
                await transaction.CommitAsync();
                var res = _mapper.Map<FlashSaleDTO>(flashSale);
                res.ProductQuantity = productFlashSale.Count();

                return res;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task DeleteFlashSale(string id)
           => await _flashSaleRepository.DeleteAsync(id);

        public async Task<FlashSaleDTO> UpdateFlashSale(string id, FlashSaleRequest request)
        {
            var flashSale = await _flashSaleRepository.FindAsync(id)
                ?? throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);

            if (flashSale.DiscountTimeFrame != request.DiscountTimeFrame || flashSale.Date.Date != request.Date.Date)
            {
                var exist = await _flashSaleRepository
                                    .GetAsync(e => e.Date.Date == request.Date.Date
                                            && e.DiscountTimeFrame == request.DiscountTimeFrame);
                if (exist.Any())
                {
                    throw new InvalidDataException(ErrorMessage.FLASHSALE_EXISTED);
                }
                flashSale.Date = request.Date;
                flashSale.DiscountTimeFrame = request.DiscountTimeFrame;
            }

            var productsFlashSale = await _productFlashSaleRepository.GetAsync(e => e.FlashSaleId == id);
            //var lstProductUpdate = productsFlashSale.Select(e =>
            //{
            //    var newProduct = request.ProductFlashSales
            //        .FirstOrDefault(x => x.ProductId == e.ProductId && x.DiscountPercent != e.DiscountPercent);
            //    if (newProduct != null)
            //    {
            //        e.DiscountPercent = newProduct.DiscountPercent;
            //    }
            //    return e;
            //});

            var lstProductUpdate = productsFlashSale
                .Join(request.ProductFlashSales,
                    e => e.ProductId,
                    r => r.ProductId,
                    (e, r) => new { e, r })
                .Where(joined => joined.e.DiscountPercent != joined.r.DiscountPercent)
                .Select(joined =>
                {
                    joined.e.DiscountPercent = joined.r.DiscountPercent;
                    return joined.e;
                })
                .ToArray();

            await _flashSaleRepository.UpdateAsync(flashSale);
            if (lstProductUpdate.Length > 0)
            {
                await _productFlashSaleRepository.UpdateAsync(lstProductUpdate);
            }
            return _mapper.Map<FlashSaleDTO>(flashSale);
        }

        public async Task<float?> GetDiscountByProductIdThisTime(long productId)
        {
            var date = DateTime.Now.Date;
            DiscountTimeFrame? timeFrame = IsFlashSaleActive();

            if (timeFrame != null)
            {
                var flashSale = await _productFlashSaleRepository
                    .SingleOrDefaultAsync(e => e.FlashSale.Date == date
                        && e.FlashSale.DiscountTimeFrame == timeFrame && e.ProductId == productId);
                if (flashSale != null)
                {
                    return flashSale.DiscountPercent;
                }
            }
            return null;
        }
    }
}
