using AutoMapper;
using MyStore.Application.DTO;
using MyStore.Application.IRepository;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Materials
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly IMapper _mapper;
        public MaterialService(IMaterialRepository materialRepository, IMapper mapper)
        {
            _materialRepository = materialRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MaterialDTO>> GetMaterialsAsync()
        {
            var materials = await _materialRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MaterialDTO>>(materials);
        }

        public async Task<MaterialDTO> AddMaterialAsync(string name)
        {
            var material = new Material
            {
                Name = name
            };
            await _materialRepository.AddAsync(material);
            return _mapper.Map<MaterialDTO>(material);
        }

        public async Task DeleteMaterialAsync(int id) => await _materialRepository.DeleteAsync(id);

        public async Task<MaterialDTO> UpdateMaterialAsync(int id, string name)
        {
            var material = await _materialRepository.FindAsync(id);
            if (material != null)
            {
                material.Name = name;
                await _materialRepository.UpdateAsync(material);
                return _mapper.Map<MaterialDTO>(material);
            }
            else throw new ArgumentException($"Id {id}" + ErrorMessage.NOT_FOUND);
        }
    }
}
