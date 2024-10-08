﻿using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories
{
    public interface IImageRepository : IRepository<Image>
    {
        Task<Image?> GetFirstImageByProductIdAsync(int id);
        Task<IEnumerable<Image>> GetImageByProductIdAsync(int productId);
    }
}
