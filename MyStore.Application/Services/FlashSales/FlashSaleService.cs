using AutoMapper;
using MyStore.Application.DTOs;
using MyStore.Application.IRepositories;
using MyStore.Application.IRepositories.Products;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using System.Linq.Expressions;

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
                .GetPagedAsync(request.Page, request.PageSize, null, e => e.CreatedAt);
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
            var productFlashSales = await _productFlashSaleRepository.GetAsync(e => e.FlashSaleId == id);
            var products = productFlashSales.Select(e => e.Product);

            var res = _mapper.Map<IEnumerable<ProductDTO>>(products).
                Select(e =>
                {
                    var productFlashSale = productFlashSales
                        .SingleOrDefault(p => p.ProductId == e.Id && p.FlashSaleId == id);

                    e.FlashSaleDiscountPercent = productFlashSale?.DiscountPercent ?? 0;
                    return e;
                });
            return res;
        }
        public Task<IEnumerable<ProductDTO>> GetProductsByTimeFrame(DiscountTimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<ProductDTO>?> GetFlashSaleThisTime()
        {
            var date = DateTime.Now.Date;
            var hours = DateTime.Now.Hour;

            DiscountTimeFrame? timeFrame = null;

            if (hours is >= 0 and < 2)
            {
                timeFrame = DiscountTimeFrame.TO00FROM2;
            }
            else if (hours is >= 10 and < 12)
            {
                timeFrame = DiscountTimeFrame.TO10FROM12;
            }
            else if (hours is >= 19 and < 24)
            {
                timeFrame = DiscountTimeFrame.TO19FROM22;
            }
            if(timeFrame != null)
            {
                var flashSale = await _flashSaleRepository
                    .SingleOrDefaultAsync(e => e.Date == date && e.DiscountTimeFrame == timeFrame);
                if (flashSale != null)
                {
                    var productFlashSales = await _productFlashSaleRepository.GetAsync(e => e.FlashSaleId == flashSale.Id);
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
            }
            return null;
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
    }
}
