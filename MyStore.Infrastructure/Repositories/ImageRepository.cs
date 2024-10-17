using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class ImageRepository(MyDbContext dbcontext) : Repository<Image>(dbcontext), IImageRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;

        public async Task<Image?> GetFirstImageByProductIdAsync(long id)
        {
            return await _dbContext.Images.FirstOrDefaultAsync(e => e.ProductId == id);
        }

        public async Task<IEnumerable<Image>> GetImageByProductIdAsync(long productId) 
            => await _dbContext.Images.Where(e => e.ProductId == productId).ToListAsync();
    }
}
