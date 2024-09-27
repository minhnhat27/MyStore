using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class VoucherRepository(MyDbContext dbcontext) : Repository<Voucher>(dbcontext), IVoucherRepository
    {

    }
}
