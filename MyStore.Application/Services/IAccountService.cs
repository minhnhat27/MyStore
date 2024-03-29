using MyStore.Application.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Services
{
    public interface IAccountService
    {
        Task Login(LoginRequest request);
        Task Register();
    }
}
