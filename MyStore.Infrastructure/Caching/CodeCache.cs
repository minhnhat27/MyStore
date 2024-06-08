using Microsoft.Extensions.Caching.Memory;
using MyStore.Application.ICaching;

namespace MyStore.Infrastructure.Caching
{
    public class CodeCache : ICodeCache
    {
        private readonly IMemoryCache _memoryCache;
        public CodeCache(IMemoryCache memoryCache) => _memoryCache = memoryCache;

        public int GetCodeFromEmail(string email)
        {
            if(_memoryCache.TryGetValue(email, out int result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }

        public void RemoveCode(string email)
        {
            if (_memoryCache.TryGetValue(email, out int result))
            {
                _memoryCache.Remove(email);
            }
        }

        public int SetCodeForEmail(string email)
        {
            Random random = new Random();
            int val = random.Next(100000, 999999);
            _memoryCache.Set(email, val, TimeSpan.FromMinutes(30));
            return val;
        }
    }
}
