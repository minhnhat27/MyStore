using AutoMapper;
using MyStore.Application.IRepositories;
using MyStore.Application.IRepositories.Products;
using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Carts
{
    public class CartService : ICartService
    {
        private readonly ICartItemRepository _cartItemsRepository;
        private readonly IImageRepository _imageRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IProductColorRepository _productColorRepository;
        private readonly IProductSizeRepository _productSizeRepository;

        private readonly ISizeRepository _sizeRepository;
        private readonly IMapper _mapper;
        public CartService(ICartItemRepository cartItemsRepository, IProductColorRepository productColorRepository,
            ISizeRepository sizeRepository, IMapper mapper, IProductSizeRepository productSizeRepository,
            IImageRepository imageRepository, ITransactionRepository transactionRepository)
        {
            _cartItemsRepository = cartItemsRepository;
            _imageRepository = imageRepository;
            _transactionRepository = transactionRepository;
            _productColorRepository = productColorRepository;
            _productSizeRepository = productSizeRepository;
            _sizeRepository = sizeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItemsResponse>> GetAllByUserId(string userId)
        {
            var items = await _cartItemsRepository.GetAsync(e => e.UserId == userId);
            var res = items.Select(cartItem =>
            {
                var color = cartItem.Product.ProductColors.Single(x => x.Id == cartItem.ColorId);
                var size = color.ProductSizes.Single(x => x.SizeId == cartItem.SizeId);

                return new CartItemsResponse
                {
                    Id = cartItem.Id,
                    ProductId = cartItem.ProductId,
                    OriginPrice = cartItem.Product.Price,
                    DiscountPercent = cartItem.Product.DiscountPercent,
                    Quantity = cartItem.Quantity,
                    ProductName = cartItem.Product.Name,
                    ImageUrl = color?.ImageUrl,
                    ColorId = cartItem.ColorId,
                    SizeId = cartItem.SizeId,
                    ColorName = color?.ColorName,
                    InStock = size.InStock,
                    SizeName = size.Size.Name,
                    SizeInStocks = _mapper.Map<IEnumerable<SizeInStock>>(color?.ProductSizes ?? [])
                };
            });
            return res;
        }

        public async Task AddToCart(string userId, CartRequest request)
        {
            try
            {
                //var color = await _productColorRepository.SingleAsync(request.ColorId);
                //var size = color.ProductSizes.Single(e => e.SizeId == request.SizeId);
                var size = await _productSizeRepository
                    .SingleAsync(e => e.ProductColorId == request.ColorId && e.SizeId == request.SizeId);

                if (size.InStock <= 0)
                {
                    throw new Exception(ErrorMessage.SOLDOUT);
                }

                var exist = await _cartItemsRepository.SingleOrDefaultAsync(
                    e => e.ProductId == request.ProductId &&
                         e.UserId == userId &&
                         e.SizeId == request.SizeId &&
                         e.ColorId == request.ColorId);

                if (exist != null)
                {
                    if ((request.Quantity + exist.Quantity) > size.InStock)
                    {
                        throw new Exception(ErrorMessage.CART_MAXIMUM);
                    }

                    exist.Quantity += request.Quantity;
                    await _cartItemsRepository.UpdateAsync(exist);
                }
                else
                {
                    CartItem item = new()
                    {
                        ProductId = request.ProductId,
                        UserId = userId,
                        Quantity = request.Quantity,
                        SizeId = request.SizeId,
                        ColorId = request.ColorId,
                    };

                    await _cartItemsRepository.AddAsync(item);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<CartItemsResponse> UpdateCartItem(string cartId, string userId, UpdateCartItemRequest cartRequest)
        {
            try
            {
                var cartItem = await _cartItemsRepository.SingleOrDefaultAsyncInclude(e => e.Id == cartId && e.UserId == userId);
                if(cartItem != null)
                {
                    var color = cartItem.Product.ProductColors.Single(x => x.Id == cartItem.ColorId);
                    var size = color.ProductSizes.Single(x => x.SizeId == cartRequest.SizeId);

                    if (cartRequest.SizeId.HasValue)
                    {
                        cartItem.SizeId = cartRequest.SizeId.Value;
                    }
                    if(cartRequest.Quantity.HasValue)
                    {
                        if (size.InStock > 0 && cartRequest.Quantity.Value <= size.InStock)
                        {
                            cartItem.Quantity = cartRequest.Quantity.Value;
                        }
                        else throw new Exception(ErrorMessage.SOLDOUT);
                    }
                    await _cartItemsRepository.UpdateAsync(cartItem);

                    return new CartItemsResponse
                    {
                        Id = cartItem.Id,
                        ProductId = cartItem.ProductId,
                        OriginPrice = cartItem.Product.Price,
                        DiscountPercent = cartItem.Product.DiscountPercent,
                        Quantity = cartItem.Quantity,
                        ProductName = cartItem.Product.Name,
                        ImageUrl = color?.ImageUrl,
                        ColorId = cartItem.ColorId,
                        SizeId = cartItem.SizeId,
                        ColorName = color?.ColorName,
                        InStock = size.InStock,
                        SizeName = size.Size.Name,
                        SizeInStocks = _mapper.Map<IEnumerable<SizeInStock>>(color?.ProductSizes ?? [])
                    };
                }
                throw new ArgumentException(ErrorMessage.NOT_FOUND + " sản phẩm");
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteCartItem(string cartId, string userId)
        {
            var cartItem = await _cartItemsRepository.SingleOrDefaultAsync(e => e.Id == cartId && e.UserId == userId);
            if (cartItem != null)
            {
                await _cartItemsRepository.DeleteAsync(cartItem);
            }
            else throw new ArgumentException($"Id {cartId} " + ErrorMessage.NOT_FOUND);
        }
    }
}
