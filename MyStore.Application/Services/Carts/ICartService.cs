using MyStore.Application.Request;
using MyStore.Application.Response;
using System.Threading.Tasks;

namespace MyStore.Application.Services.Carts
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemsResponse>> GetAllByUserId(string userId);
        Task<int> CountCart(string userId);
        Task AddToCart(string userId, CartRequest cartRequest);
        Task<CartItemsResponse> UpdateCartItem(string cartId, string userId, UpdateCartItemRequest cartRequest);
        Task DeleteCartItem(string cartId, string userId);
    }
}
