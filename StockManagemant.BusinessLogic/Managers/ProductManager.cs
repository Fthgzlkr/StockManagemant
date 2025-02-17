using CsvHelper;
using CsvHelper.Configuration;
using StockManagemant.DataAccess.Repositories;
using StockManagemant.Entities.Models;
using StockManagement.DataAccess.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public class ProductManager
    {
        private readonly ProductRepository _productRepository;

        public ProductManager(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // ✅ Toplam ürün sayısını getir (Silinmemiş olanlar)
        public async Task<int> GetTotalProductCountAsync(string search = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var filter = new ProductFilter
            {
                Search = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            return await _productRepository.GetTotalCountAsync(filter);
        }

        public async Task<List<Product>> GetPagedProductAsyn(int page, int pageSize, string search = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var filter = new ProductFilter
            {
                Search = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var products = await _productRepository.GetPagedProductsAsync(filter, page, pageSize);
            return products.ToList();
        }




        // ✅ Ürün ekleme
        public async Task AddProductAsync(Product product)
        {
            await _productRepository.AddAsync(product);
        }

        // ✅ Ürün güncelleme
        public async Task UpdateProductAsync(Product product)
        {
            await _productRepository.UpdateAsync(product);
        }

        // ✅ Ürün silme (Soft Delete)
        public async Task DeleteProductAsync(int productId)
        {
            await _productRepository.DeleteAsync(productId);
        }

        // ✅ Soft Delete olan ürünü geri getirme (Restore)
        public async Task RestoreProductAsync(int productId)
        {
            await _productRepository.RestoreAsync(productId);
        }

        // ✅ ID’ye göre ürün bulma (Sadece aktif ürünleri getirir)
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        // ✅ ID’ye göre ürün bulma (Silinmiş ürünler de dahil)
        public async Task<Product> GetProductByIdWithDeletedAsync(int productId)
        {
            return await _productRepository.GetByIdWithDeletedAsync(productId);
        }



     
    }
}
