using MyStore.Application.Response;

namespace MyStore.Application.Services.Carts
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemsResponse>> GetAllByUserId(string userId);
    }
}
