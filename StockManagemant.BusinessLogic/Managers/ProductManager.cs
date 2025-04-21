using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using System.Linq;
using StockManagemant.DataAccess.Repositories.Interfaces;


namespace StockManagemant.Business.Managers
{
    public class ProductManager : IProductManager
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;

        public ProductManager(IProductRepository productRepository,IMapper mapper, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        //  Toplam ürün sayısını getir (Silinmemiş olanlar)
        public async Task<int> GetTotalProductCountAsync(ProductFilter filter)
        {
        
            return await _productRepository.GetTotalCountAsync(filter);
        }

        public async Task<List<ProductDto>> GetPagedProductAsync(int page, int pageSize, ProductFilter filter)
        {
            var products = await _productRepository.GetPagedProductsAsync(filter, page, pageSize); //  Sayfalama işlemi repoda yapılıyor
            return _mapper.Map<List<ProductDto>>(products); //  DTO dönüşümü burada yapılıyor
        }




        public async Task<int> AddProductAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            // ✅ Category nesnesini manuel yükleyelim
            product.Category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (product.Category == null)
            {
                throw new Exception($"Kategori ID {productDto.CategoryId} ile kategori bulunamadı.");
            }

            await _productRepository.AddAsync(product);
            return product.Id;
        }



        public async Task UpdateProductAsync(ProductDto productDto)
        {
            var existingProduct = await _productRepository.GetByIdAsync(productDto.Id ?? 0);
            if (existingProduct == null) throw new Exception("Ürün bulunamadı.");

            _mapper.Map(productDto, existingProduct);

            existingProduct.Category = await _categoryRepository.GetByIdAsync(productDto.CategoryId);
            if (existingProduct.Category == null) throw new Exception("Geçersiz kategori.");

            await _productRepository.UpdateAsync(existingProduct);
        }


       
        public async Task DeleteProductAsync(int productId)
        {
            await _productRepository.DeleteAsync(productId);
        }

        //  Soft Delete olan ürünü geri getirme (Restore)
        public async Task RestoreProductAsync(int productId)
        {
            await _productRepository.RestoreAsync(productId);
        }

        //  ID’ye göre ürün bulma (Sadece aktif ürünleri getirir)
        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }

        // ID’ye göre ürün bulma (Silinmiş ürünler dahil)
        public async Task<ProductDto> GetProductByIdWithDeletedAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithDeletedAsync(productId);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> IsProductInWarehouseAsync(int productId, int warehouseId)
        {
            return await _productRepository.IsProductInWarehouseAsync(productId, warehouseId);
        }



    }
}
