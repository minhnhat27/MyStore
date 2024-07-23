using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        private readonly MyDbContext _dbContext;
        public ImageRepository(MyDbContext dbcontext) : base(dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task<Image?> GetFirstImageByProductIdAsync(int id)
        {
            return await _dbContext.Images.FirstOrDefaultAsync(e => e.ProductId == id);
        }

        public async Task<IEnumerable<Image>> GetImageByProductIdAsync(int productId) 
            => await _dbContext.Images.Where(e => e.ProductId == productId).ToListAsync();
    }
}
