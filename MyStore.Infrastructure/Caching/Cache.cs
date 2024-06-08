using Microsoft.Extensions.Caching.Memory;
using MyStore.Application.ICaching;

namespace MyStore.Infrastructure.Caching
{
    public class Cache : ICache
    {
        private readonly IMemoryCache _memoryCache;
        public Cache(IMemoryCache memoryCache) => _memoryCache = memoryCache;
       
        public T? Get<T>(string cacheKey)
        {
            _memoryCache.TryGetValue(cacheKey, out T? value);
            return value;
        }

        public void Set<T>(string cacheKey, T value)
        {
            _memoryCache.Set(cacheKey, value, TimeSpan.FromHours(6));
        }

        public void Remove(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }
    }
}
