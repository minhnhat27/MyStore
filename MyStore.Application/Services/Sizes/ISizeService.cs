using Microsoft.AspNetCore.Http;
using MyStore.Application.DTO;

namespace MyStore.Application.Services.Sizes
{
    public interface ISizeService
    {
        Task<IEnumerable<SizeDTO>> GetSizesAsync();
        Task<SizeDTO> AddSizeAsync(string name);
        Task<SizeDTO> UpdateSizeAsync(int id, string name);
        Task DeleteSizeAsync(int id);
    }
}
