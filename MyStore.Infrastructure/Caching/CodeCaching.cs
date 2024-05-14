using Microsoft.Extensions.Caching.Memory;
using MyStore.Application.IRepository.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Infrastructure.Caching
{
    public class CodeCaching : ICodeCaching
    {
        private readonly IMemoryCache _memoryCache;
        public CodeCaching(IMemoryCache memoryCache) => _memoryCache = memoryCache;

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

        public void SetCodeForEmail(string email)
        {
            Random random = new Random();
            int val = random.Next(100000, 999999);
            _memoryCache.Set(email, val, TimeSpan.FromMinutes(30));
        }
    }
}
