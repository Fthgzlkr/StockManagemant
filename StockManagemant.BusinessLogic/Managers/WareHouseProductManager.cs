using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories.Filters;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using AutoMapper;
using StockManagemant.DataAccess.Repositories;

namespace StockManagemant.Business.Managers
{
    public class WarehouseProductManager : IWarehouseProductManager
    {
        private readonly WarehouseProductRepository _warehouseProductRepository;
        private readonly ProductRepository _productRepository;
        private readonly IMapper _mapper;

        public WarehouseProductManager(WarehouseProductRepository warehouseProductRepository,
                                       ProductRepository productRepository,
                                       IMapper mapper)
        {
            _warehouseProductRepository = warehouseProductRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<List<WarehouseProductDto>> GetProductsByWarehouseIdAsync(int warehouseId)
        {
            var warehouseProducts = await _warehouseProductRepository.GetProductsByWarehouseIdAsync(warehouseId);
            return _mapper.Map<List<WarehouseProductDto>>(warehouseProducts);
        }

        public async Task<int> GetTotalStockByProductIdAsync(int productId)
        {
            return await _warehouseProductRepository.GetTotalStockByProductIdAsync(productId);
        }

        public async Task İncreaseStockAsync(UpdateWarehouseProductStockDto dto)
        {
            var warehouseProduct = await _warehouseProductRepository
                .FindAsync(wp => wp.ProductId == dto.ProductId && wp.WarehouseId == dto.WarehouseId);

            if (!warehouseProduct.Any()) throw new Exception("Ürün bu depoda bulunamadı.");

            var entity = warehouseProduct.FirstOrDefault();
            if (dto.StockQuantity < 0 && entity.StockQuantity < Math.Abs(dto.StockQuantity))
                throw new Exception("Yetersiz stok!");

            entity.StockQuantity += dto.StockQuantity;
            await _warehouseProductRepository.UpdateAsync(entity);
        }

        public async Task DecreaseStockAsync(UpdateWarehouseProductStockDto dto)
        {
            var warehouseProduct = await _warehouseProductRepository
                .FindAsync(wp => wp.ProductId == dto.ProductId && wp.WarehouseId == dto.WarehouseId);

            if (!warehouseProduct.Any()) throw new Exception("Ürün bu depoda bulunamadı.");

            var entity = warehouseProduct.FirstOrDefault();

            if (entity.StockQuantity < dto.StockQuantity) // ✅ Stok 0’ın altına düşmemeli
                throw new Exception("Yetersiz stok!");

            entity.StockQuantity -= dto.StockQuantity; // ✅ Stok azaltma işlemi

            await _warehouseProductRepository.UpdateAsync(entity);
        }

        public async Task AddProductToWarehouseAsync(AddExistingProductToWarehouseDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null) throw new Exception("Ürün bulunamadı.");

            var existingWarehouseProduct = await _warehouseProductRepository
                .FindAsync(wp => wp.ProductId == dto.ProductId && wp.WarehouseId == dto.WarehouseId);

            if (existingWarehouseProduct.Any())
                throw new Exception("Bu ürün zaten bu depoda mevcut!");

            var warehouseProduct = new WarehouseProduct
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                StockQuantity = dto.StockQuantity,
                IsDeleted = false
            };

            await _warehouseProductRepository.AddAsync(warehouseProduct);
        }

        public async Task RemoveProductFromWarehouseAsync(int warehouseProductId)
        {
            await _warehouseProductRepository.DeleteAsync(warehouseProductId);
        }

        public async Task RestoreWarehouseProductAsync(int warehouseProductId)
        {
            await _warehouseProductRepository.RestoreAsync(warehouseProductId);
        }

        
        public async Task<List<WarehouseProductDto>> GetPagedWarehouseProductsAsync(WarehouseProductFilter filter, int warehouseId, int page, int pageSize)
        {
            var warehouseProducts = await _warehouseProductRepository.GetPagedWarehouseProductsWithStockAsync(filter, warehouseId, page, pageSize);

            return warehouseProducts.Select(wp => new WarehouseProductDto
            {
                ProductId = wp.ProductId,
                ProductName = wp.Product.Name,
                CategoryName = wp.Product.Category.Name,
                Price = wp.Product.Price,
                Currency = wp.Product.Currency,
                WarehouseId = wp.WarehouseId,
                WarehouseName = wp.Warehouse.Name,
                StockQuantity = wp.StockQuantity
            }).ToList();
        }

      
        public async Task<int> GetTotalWarehouseProductCountAsync(WarehouseProductFilter filter,int WarehouseId)
        {
            return await _warehouseProductRepository.GetTotalCountAsync(filter,WarehouseId);
        }



        public async Task<WarehouseProductDto> GetProductInWarehouseByIdAsync(int warehouseId, int productId)
        {
            var warehouseProduct = await _warehouseProductRepository.GetProductInWarehouseByIdAsync(warehouseId, productId);

            if (warehouseProduct == null)
                throw new Exception("Ürün bu depoda bulunamadı.");

            return new WarehouseProductDto
            {
                ProductId = warehouseProduct.ProductId,
                ProductName = warehouseProduct.Product.Name,
                Price = warehouseProduct.Product.Price,
                Currency = warehouseProduct.Product.Currency,
                CategoryName = warehouseProduct.Product.Category?.Name,
                WarehouseId = warehouseProduct.WarehouseId,
                WarehouseName = warehouseProduct.Warehouse.Name,
                StockQuantity = warehouseProduct.StockQuantity
            };
        }


    }
}
