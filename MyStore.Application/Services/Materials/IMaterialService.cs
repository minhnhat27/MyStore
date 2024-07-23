using MyStore.Application.DTO;

namespace MyStore.Application.Services.Materials
{
    public interface IMaterialService
    {
        Task<IEnumerable<MaterialDTO>> GetMaterialsAsync();
        Task<MaterialDTO> AddMaterialAsync(string name);
        Task<MaterialDTO>  UpdateMaterialAsync(int id, string name);
        Task DeleteMaterialAsync(int id);
    }
}
