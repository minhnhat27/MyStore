using Microsoft.Extensions.Caching.Memory;
using MyStore.Application.ICaching;

namespace MyStore.Infrastructure.Caching
{
    public class Cache(IMemoryCache memoryCache) : ICache
    {
        private readonly IMemoryCache _memoryCache = memoryCache;

        public T? Get<T>(string cacheKey)
        {
            _memoryCache.TryGetValue(cacheKey, out T? value);
            return value;
        }

        public void Set<T>(string cacheKey, T value, TimeSpan time)
        {
            _memoryCache.Set(cacheKey, value, time);
        }

        public void Set<T>(string cacheKey, T value, MemoryCacheEntryOptions options)
        {
            _memoryCache.Set(cacheKey, value, options);
        }

        public void Remove(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }
    }
}
