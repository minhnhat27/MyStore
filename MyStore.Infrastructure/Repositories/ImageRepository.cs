using Microsoft.AspNetCore.Http;
using MyStore.Application.IRepository;
using System.Reflection;

namespace MyStore.Infrastructure.Repositories
{
    public class ImageRepository : IImageRepository
    {
        public void DeleteImage(string type, string fileName)
        {
            var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Images", type);
            var filePath = Path.Combine(path, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void DeleteImages(string type, List<string> fileNames)
        {
            var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Images", type);
            foreach (var file in fileNames)
            {
                var filePath = Path.Combine(path, file);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        public async Task<string> GetImageBase64Async(string type, string fileName)
        {
            var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Images", type);

            var filePath = Path.Combine(path, fileName);
            if(File.Exists(filePath))
            {
                return Convert.ToBase64String(await File.ReadAllBytesAsync(filePath));
            }
            else
            {
                return String.Empty;
            }
        }

        public async Task<List<string>> GetImagesBase64Async(string type, List<string> fileNames)
        {
            var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Images", type);
            Directory.CreateDirectory(path);

            List<string> images = new();
            foreach (var file in fileNames)
            {

                var filePath = Path.Combine(path, file);
                if (File.Exists(filePath))
                {
                    images.Add(Convert.ToBase64String(await File.ReadAllBytesAsync(filePath)));
                }
            }
            return images;
        }

        public async Task SaveImageAsync(string type, IFormFile image, string fileName)
        {
            var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Images", type);
            Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
        }

        public async Task SaveImagesAsync(string type, IFormFileCollection images, List<string> fileNames)
        {
            var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Images", type);
            Directory.CreateDirectory(path);

            for (var i = 0; i < images.Count; i++)
            {

                var filePath = Path.Combine(path, fileNames[i]);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await images[i].CopyToAsync(stream);
                }
            }
        }
    }
}
