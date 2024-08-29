using AutoMapper;
using MyStore.Application.DTO;
using MyStore.Application.IRepositories;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO> AddCategoryAsync(string name)
        {
            var category = new Category
            {
                Name = name,
            };
            await _categoryRepository.AddAsync(category);
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task DeleteCategoryAsync(int id) => await _categoryRepository.DeleteAsync(id);

        public async Task<CategoryDTO> UpdateCategoryAsync(int id, string name)
        {
            var category = await _categoryRepository.FindAsync(id);
            if (category != null)
            {
                category.Name = name;
                await _categoryRepository.UpdateAsync(category);
                return _mapper.Map<CategoryDTO>(category);
            }
            else throw new ArgumentException($"Id {id}" + ErrorMessage.NOT_FOUND);
        }
    }
}
