using AutoMapper;
using StockManagemant.BusinessLogic.Managers.Interfaces;
using StockManagemant.DataAccess.Repositories.Interfaces;
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
                 var locations = await _repository.FindAsync(x => x.WarehouseId == warehouseId);
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

        public async Task<IEnumerable<object>> GetCorridorsByWarehouseIdAsync(int warehouseId)
        {
            return await _repository.GetCorridorsByWarehouseIdAsync(warehouseId);
        }

        public async Task<IEnumerable<object>> GetShelvesByWarehouseAsync(int warehouseId, string corridor)
        {
            return await _repository.GetShelvesByWarehouseAsync(warehouseId, corridor);
        }

        public async Task<IEnumerable<object>> GetBinsByWarehouseAsync(int warehouseId, string corridor, string shelf)
        {
            return await _repository.GetBinsByWarehouseAsync(warehouseId, corridor, shelf);
        }
    }
}