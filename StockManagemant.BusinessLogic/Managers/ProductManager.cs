using StockManagemant.DataAccess.Repositories;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using System.Linq;


namespace StockManagemant.Business.Managers
{
    public class ProductManager : IProductManager
    {
        private readonly ProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly CategoryRepository _categoryRepository;

        public ProductManager(ProductRepository productRepository,IMapper mapper, CategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        // ✅ Toplam ürün sayısını getir (Silinmemiş olanlar)
        public async Task<int> GetTotalProductCountAsync(ProductFilter filter)
        {
        
            return await _productRepository.GetTotalCountAsync(filter);
        }

        public async Task<List<ProductDto>> GetPagedProductAsync(int page, int pageSize, ProductFilter filter)
        {
            var products = await _productRepository.GetPagedProductsAsync(filter, page, pageSize);
            var productDtos = _mapper.Map<List<ProductDto>>(products);

            // **Kategori bilgisi eksikse, boş olarak ayarla!**
            foreach (var dto in productDtos)
            {
                dto.CategoryName = dto.CategoryName ?? "Uncategorized"; // Eğer boşsa, default değer ata
            }

            return productDtos;
        }





        // ✅ Ürün ekleme
        public async Task<int> AddProductAsync(CreateProductDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            await _productRepository.AddAsync(product);
            return product.Id; // ID otomatik olarak atanır
        }



        // ✅ Ürün güncelleme
     public async Task UpdateProductAsync(UpdateProductDto dto)
{
    var existingProduct = await _productRepository.GetByIdAsync(dto.Id);
    if (existingProduct == null) throw new Exception("Ürün bulunamadı.");

    _mapper.Map(dto, existingProduct);

    
    existingProduct.Category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
    if (existingProduct.Category == null) throw new Exception("Geçersiz kategori.");

    await _productRepository.UpdateAsync(existingProduct);
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
        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }



        // ✅ **ID’ye göre ürün bulma (Silinmiş ürünler dahil)**
        public async Task<ProductDto> GetProductByIdWithDeletedAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithDeletedAsync(productId);
            if (product == null) return null;

            return _mapper.Map<ProductDto>(product);
        }




    }
}
