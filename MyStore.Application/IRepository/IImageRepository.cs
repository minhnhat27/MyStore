using Microsoft.AspNetCore.Http;

namespace MyStore.Application.IRepository
{
    public interface IImageRepository
    {
        Task SaveImageAsync(string type, IFormFile image, string fileName);
        Task SaveImagesAsync(string type, IFormFileCollection images, List<string> fileNames);
        Task<string> GetImageBase64Async(string type, string fileName);
        Task<List<string>> GetImagesBase64Async(string type, List<string> fileNames);
        void DeleteImage(string type, string fileName);
        void DeleteImages(string type, List<string> fileNames);
    }
}
