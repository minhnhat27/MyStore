using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.IRepository.Caching
{
    public interface ICodeCaching
    {
        int SetCodeForEmail(string email);
        int GetCodeFromEmail(string email);
        void RemoveCode(string email);
    }
}
