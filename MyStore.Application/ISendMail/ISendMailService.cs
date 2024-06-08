using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.ISendMail
{
    public interface ISendMailService
    {
        Task SendMailToOne(string email, string subject, string htmlMessage);
        Task SendMailToMany(List<string> email, string subject, string htmlMessage);
    }
}
