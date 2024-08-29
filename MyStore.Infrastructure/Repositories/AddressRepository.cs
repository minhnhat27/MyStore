using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class AddressRepository : Repository<Address>, IAddressRepository
    {
        public AddressRepository(MyDbContext dbcontext) : base(dbcontext)
        {
        }
    }
}
