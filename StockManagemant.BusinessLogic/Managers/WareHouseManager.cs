using StockManagemant.Entities.Models;
using StockManagemant.Entities.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Repositories.Interfaces;

namespace StockManagemant.Business.Managers
{
    public class WarehouseManager : IWarehouseManager
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IMapper _mapper;

        public WarehouseManager(IWarehouseRepository warehouseRepository, IMapper mapper)
        {
            _warehouseRepository = warehouseRepository;
            _mapper = mapper;
        }

        //Tüm Depoları getir
        public async Task<List<WareHouseDto>> GetAllWarehousesAsync()
        {
            var warehouses = await _warehouseRepository.GetAllActiveWarehousesAsync();
            return _mapper.Map<List<WareHouseDto>>(warehouses);
        }

        //İd ye göre depo getir
        public async Task<WareHouseDto> GetWarehouseByIdAsync(int warehouseId)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            return warehouse == null ? null : _mapper.Map<WareHouseDto>(warehouse);
        }


        //İsime Göre depo getir
        public async Task<WareHouseDto> GetWarehouseByNameAsync(string name)
        {
            var warehouse = await _warehouseRepository.GetByNameAsync(name);
            return warehouse == null ? null : _mapper.Map<WareHouseDto>(warehouse);
        }


        //Yeni Depo oluşturma 
        public async Task<int> AddWarehouseAsync(WareHouseDto warehouseDto)
        {
            var warehouse = _mapper.Map<Warehouse>(warehouseDto);
            await _warehouseRepository.AddAsync(warehouse);
            return warehouse.Id;
        }


        //Depoyu güncelle
        public async Task UpdateWarehouseAsync(WareHouseDto warehouseDto)
        {
            var existingWarehouse = await _warehouseRepository.GetByIdAsync(warehouseDto.Id ?? 0);
            if (existingWarehouse == null) throw new Exception("Depo bulunamadı.");

            _mapper.Map(warehouseDto, existingWarehouse);
            await _warehouseRepository.UpdateAsync(existingWarehouse);
        }



        //Depoyu sil (Kullanırken Dikkatli ol)
        public async Task DeleteWarehouseAsync(int warehouseId)
        {
            await _warehouseRepository.DeleteAsync(warehouseId);
        }

        public async Task RestoreWarehouseAsync(int warehouseId)
        {
            await _warehouseRepository.RestoreAsync(warehouseId);
        }

        public async Task<WareHouseDto> GetWarehouseWithProductsAsync(int warehouseId)
        {
            var warehouse = await _warehouseRepository.GetWarehouseWithProductsAsync(warehouseId);
            if (warehouse == null) return null;

            var dto = _mapper.Map<WareHouseDto>(warehouse);
            dto.TotalProducts = warehouse.WarehouseProducts.Count;
            return dto;
        }
    }
}
