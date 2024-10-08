﻿using Microsoft.AspNetCore.Http;
using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Products
{
    public interface IProductService
    {
        Task<ProductDTO> CreateProductAsync(ProductRequest request, IFormFileCollection images);
        Task<PagedResponse<ProductDTO>> GetProductsAsync(int page, int pageSize, string? keySearch);
        Task<PagedResponse<ProductDTO>> GetGetFeaturedProductsAsync(int page, int pageSize);
        Task<PagedResponse<ProductDTO>> GetFilterProductsAsync(ProductFiltersRequest filters);

        Task<IEnumerable<ProductDTO>> GetSearchProducts(string key);

        Task<ProductDetailsResponse> GetProductAsync(int id);
        Task<ProductDTO> UpdateProductAsync(int id, ProductRequest request, IFormFileCollection images);
        Task<bool> UpdateProductEnableAsync(int id, UpdateEnableRequest request);
        Task DeleteProductAsync(int id);
    }
}
