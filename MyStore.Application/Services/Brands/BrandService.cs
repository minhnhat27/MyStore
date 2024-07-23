using AutoMapper;
using Microsoft.AspNetCore.Http;
using MyStore.Application.DTO;
using MyStore.Application.IRepository;
using MyStore.Application.IStorage;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Brands
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly ITransactionRepository _transactionRepository;
        private readonly string path = "assets/images/brands";
        public BrandService(IBrandRepository brandRepository, IMapper mapper, IFileStorage fileStorage, 
            ITransactionRepository transactionRepository)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _fileStorage = fileStorage;
            _transactionRepository = transactionRepository;
        }
        public async Task<IEnumerable<BrandDTO>> GetBrandsAsync()
        {
            var brands = await _brandRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BrandDTO>>(brands);
        }

        public async Task<BrandDTO> AddBrandAsync(string name, IFormFile image)
        {
            using (var transaction = await _transactionRepository.BeginTransactionAsync())
            {
                try
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    Brand brand = new()
                    {
                        Name = name,
                        ImageUrl = Path.Combine(path, fileName),
                    };
                    await _brandRepository.AddAsync(brand);
                    await _fileStorage.SaveAsync(path, image, fileName);

                    await transaction.CommitAsync();

                    return _mapper.Map<BrandDTO>(brand);
                }
                catch(Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.InnerException?.Message ?? ex.Message);
                }
            }
        }

        public async Task DeleteBrandAsync(int id)
        {
            using (var transaction = await _transactionRepository.BeginTransactionAsync())
            {
                try
                {
                    var brand = await _brandRepository.FindAsync(id);
                    if (brand != null)
                    {
                        _fileStorage.Delete(brand.ImageUrl);
                        await _brandRepository.DeleteAsync(id);
                        await transaction.CommitAsync();
                    }
                    else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.InnerException?.Message ?? ex.Message);
                }
            }
        }

        public async Task<BrandDTO> UpdateBrandAsync(int id, string name, IFormFile? image)
        {
            var brand = await _brandRepository.FindAsync(id);
            if(brand != null)
            {
                brand.Name = name;
                if(image != null)
                {
                    _fileStorage.Delete(brand.ImageUrl);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    brand.ImageUrl = Path.Combine(path, fileName);

                    await _fileStorage.SaveAsync(path, image, fileName);
                }
                await _brandRepository.UpdateAsync(brand);

                return _mapper.Map<BrandDTO>(brand);
            }
            else throw new ArgumentException(ErrorMessage.NOT_FOUND);
        }
    }
}
