using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Services.FlashSales
{
    public interface IFlashSaleService
    {
        Task<IEnumerable<ProductDTO>> GetFlashSaleProductsThisTime();
        DiscountTimeFrame? IsFlashSaleActive();
        Task<float?> GetDiscountByProductIdThisTime(long productId);
        Task<IEnumerable<ProductDiscountPercentWithId>> GetFlashSaleProductsWithDiscountThisTime();
        Task<IEnumerable<ProductDTO>> GetProductsByFlashSale(string id);
        Task<IEnumerable<ProductDTO>> GetProductsByTimeFrame(DiscountTimeFrame timeFrame);
        Task<PagedResponse<FlashSaleDTO>> GetFlashSales(PageRequest request);
        Task<FlashSaleDTO> CreateFlashSale(FlashSaleRequest request);
        Task<FlashSaleDTO> UpdateFlashSale(string id, FlashSaleRequest request);
        Task DeleteFlashSale(string id);
    }
}
