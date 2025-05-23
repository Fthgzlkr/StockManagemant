using AutoMapper;
using StockManagemant.BusinessLogic.Managers.Interfaces;
using StockManagemant.DataAccess.Repositories.Interfaces;
using StockManagemant.Entities.Enums;
using StockManagemant.Entities.DTO;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.BusinessLogic.Managers
{
    public class WareHouseLocationManager : IWareHouseLocationManager
    {
        private readonly IWareHouseLocationRepository _repository;
        private readonly IMapper _mapper;

        public WareHouseLocationManager(IWareHouseLocationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<WarehouseLocationDto>> GetAllAsync()
        {
            var locations = await _repository.GetAllAsync();
            return _mapper.Map<List<WarehouseLocationDto>>(locations);
        }

        public async Task<WarehouseLocationDto> GetByIdAsync(int id)
        {
            var location = await _repository.GetByIdAsync(id);
            return _mapper.Map<WarehouseLocationDto>(location);
        }

        public async Task AddAsync(WarehouseLocationDto locationDto)
        {
            var entity = _mapper.Map<WarehouseLocation>(locationDto);
            await _repository.AddAsync(entity);
        }
        public async Task<IEnumerable<WarehouseLocationDto>> GetLocationsByWarehouseIdAsync(int warehouseId)
        {
            var locations = await _repository.GetLocationsByWarehouseIdAsync(warehouseId);
            return _mapper.Map<IEnumerable<WarehouseLocationDto>>(locations);
        }



        public async Task UpdateAsync(WarehouseLocationDto locationDto)
        {
            var entity = _mapper.Map<WarehouseLocation>(locationDto);
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task RestoreAsync(int id)
        {
            await _repository.RestoreAsync(id);
        }
        
        //  Parent-child alt lokasyonları getir
        public async Task<List<WarehouseLocationDto>> GetChildrenAsync(int parentId)
        {
            var children = await _repository.GetChildrenAsync(parentId);
            return _mapper.Map<List<WarehouseLocationDto>>(children);
        }

        //  StorageType’a göre filtreleme
        public async Task<List<WarehouseLocationDto>> GetLocationsByStorageTypeAsync(int warehouseId, StorageType storageType)
        {
            var locations = await _repository.GetLocationsByStorageTypeAsync(warehouseId, storageType);
            return _mapper.Map<List<WarehouseLocationDto>>(locations);
        }

        //  Level + StorageType’a göre filtreleme
        public async Task<List<WarehouseLocationDto>> GetLocationsByLevelAndStorageTypeAsync(int warehouseId, LocationLevel level, StorageType storageType)
        {
            var locations = await _repository.GetLocationsByLevelAndStorageTypeAsync(warehouseId, level, storageType);
            return _mapper.Map<List<WarehouseLocationDto>>(locations);
        }
    }
}