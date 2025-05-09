﻿using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public interface IWarehouseManager
    {
        Task<List<WareHouseDto>> GetAllWarehousesAsync();
        Task<WareHouseDto> GetWarehouseByIdAsync(int warehouseId);
        Task<WareHouseDto> GetWarehouseByNameAsync(string name);
        Task<int> AddWarehouseAsync(WareHouseDto warehouseDto);
        Task UpdateWarehouseAsync(WareHouseDto warehouseDto);
        Task DeleteWarehouseAsync(int warehouseId);
        Task RestoreWarehouseAsync(int warehouseId);
        Task<WareHouseDto> GetWarehouseWithProductsAsync(int warehouseId);
    }
}
