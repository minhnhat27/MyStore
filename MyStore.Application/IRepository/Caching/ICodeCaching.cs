using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.IRepository.Caching
{
    public interface ICodeCaching
    {
        void SetCodeForEmail(string email);
        int GetCodeFromEmail(string email);
        void RemoveCode(string email);
    }
}
