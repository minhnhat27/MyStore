using MyStore.Application.DTOs;

namespace MyStore.Application.Services.Categories
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetCategoriesAsync();
        Task<CategoryDTO> AddCategoryAsync(string name);
        Task<CategoryDTO> UpdateCategoryAsync(int id, string name);
        Task DeleteCategoryAsync(int id);
    }
}
