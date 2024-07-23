using Microsoft.AspNetCore.Http;

namespace MyStore.Application.IStorage
{
    public interface IFileStorage
    {
        Task<string> GetBase64Async(string path);
        Task<IEnumerable<string>> GetBase64Async(IEnumerable<string> path);

        Task SaveAsync(string path, IFormFile file, string fileName);
        Task SaveAsync(string path, IFormFileCollection files, IList<string> fileNames);
        
        void Delete(string path);
        void Delete(IEnumerable<string> paths);
    }
}
