using Microsoft.AspNetCore.Http;
using MyStore.Application.IStorage;
using MyStore.Domain.Constants;
using System.IO;

namespace MyStore.Infrastructure.Storage
{
    public class FileStorage : IFileStorage
    {
        public async Task<string> GetBase64Async(string path)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(filePath))
            {
                return Convert.ToBase64String(await File.ReadAllBytesAsync(filePath));
            }
            else return string.Empty;
        }

        public async Task<IEnumerable<string>> GetBase64Async(IEnumerable<string> paths)
        {
            var tasks = paths.Select(async path =>
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
                if (File.Exists(filePath))
                {
                    var fileBytes = await File.ReadAllBytesAsync(filePath);
                    return Convert.ToBase64String(fileBytes);
                }
                return string.Empty;
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(result => result != string.Empty);
        }


        public async Task SaveAsync(string path, IFormFile file, string fileName)
        {
            var p = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path);
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }

            var filePath = Path.Combine(p, fileName);
            using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                await file.CopyToAsync(stream);
            }
        }

        public async Task SaveAsync(string path, IFormFileCollection files, IList<string> fileNames)
        {
            var p = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path);
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }

            var tasks = files.Select(async (file, index) =>
            {
                var filePath = Path.Combine(p, fileNames[index]);
                using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task SaveAsync(string path, IEnumerable<IFormFile> files, IList<string> fileNames)
        {
            var p = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path);
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }

            var tasks = files.Select(async (file, index) =>
            {
                var filePath = Path.Combine(p, fileNames[index]);
                using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }
            });

            await Task.WhenAll(tasks);
        }

        public void Delete(string path)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        public void Delete(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }


    }
}
