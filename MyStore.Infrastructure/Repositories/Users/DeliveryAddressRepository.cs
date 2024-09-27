using MyStore.Application.IRepositories.Users;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Users
{
    public class DeliveryAddressRepository(MyDbContext dbcontext) 
        : Repository<DeliveryAddress>(dbcontext), IDeliveryAddressRepository
    {
    }
}
