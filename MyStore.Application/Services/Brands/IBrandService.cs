using Microsoft.AspNetCore.Http;
using MyStore.Application.DTOs;

namespace MyStore.Application.Services.Brands
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandDTO>> GetBrandsAsync();
        Task<BrandDTO> AddBrandAsync(string name, IFormFile image);
        Task<BrandDTO> UpdateBrandAsync(int id, string name, IFormFile? image);
        Task DeleteBrandAsync(int id);
    }
}
