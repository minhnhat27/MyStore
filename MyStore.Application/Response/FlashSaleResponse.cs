using MyStore.Application.DTOs;

namespace MyStore.Application.Response
{
    public class FlashSaleResponse
    {
        public DateTime? EndFlashSale { get; set; }
        public IEnumerable<ProductDTO> Products { get; set; }
    }
}
