﻿using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public interface IProductManager
    {
        Task<int> GetTotalProductCountAsync(ProductFilter filter);
        Task<List<ProductDto>> GetPagedProductAsync(int page, int pageSize, ProductFilter filter);
        Task<int> AddProductAsync(ProductDto productDto);
        Task UpdateProductAsync(ProductDto productDto);
        Task DeleteProductAsync(int productId);
        Task RestoreProductAsync(int productId);
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<ProductDto> GetProductByIdWithDeletedAsync(int productId);
        Task<bool> IsProductInWarehouseAsync(int productId, int warehouseId);
        Task<(int insertedCount, List<string> errors)> AddProductsFromExcelAsync(List<RawProductModel> rawProducts, int userId, string fileName);
        Task<ProductDto> GetProductByBarcode(string barcode);
    }
}
