using MyStore.Application.IRepository;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Carts
{
    public class CartService : ICartService
    {
        private readonly ICartItemRepository _cartItemsRepository;
        private readonly IImageRepository _imageRepository;
        public CartService(ICartItemRepository cartItemsRepository, IImageRepository imageRepository)
        {
            _cartItemsRepository = cartItemsRepository;
            _imageRepository = imageRepository;
        }
        public async Task<IEnumerable<CartItemsResponse>> GetAllByUserId(string userId)
        {
            var items = await _cartItemsRepository.GetAsync(e => e.UserId == userId);
            var res = items.Select(e =>
            {
                var imageUrl = e.Product.Images.FirstOrDefault();
                return new CartItemsResponse
                {
                    ProductId = e.ProductId,
                    Price = e.Product.Price,
                    DiscountPercent = e.Product.DiscountPercent,
                    Quantity = e.Quantity,
                    ProductName = e.Product.Name,
                    ImageUrl = imageUrl != null ? imageUrl.ImageUrl : null,
                };
            });
            //foreach (var product in res)
            //{
            //    var image = await _imageRepository.GetFirstImageByProductIdAsync(product.ProductId);
            //    if (image != null)
            //    {
            //        product.ImageUrl = image.ImageUrl;
            //    }
            //}
            return res;
        }
    }
}
