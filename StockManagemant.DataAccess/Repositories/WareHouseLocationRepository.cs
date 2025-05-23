using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories.Interfaces;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories
{
    public class WareHouseLocationRepository : Repository<WarehouseLocation>, IWareHouseLocationRepository
    {
        public WareHouseLocationRepository(AppDbContext context) : base(context) { }

        // 🔹 Belirli bir depoya ait tüm lokasyonları getir (silinmemiş)
        public async Task<List<WarehouseLocation>> GetLocationsByWarehouseIdAsync(int warehouseId)
        {
            return await _context.WarehouseLocations
                .Where(wl => wl.WarehouseId == warehouseId && !wl.IsDeleted)
                .ToListAsync();
        }



        // 🔹 Root seviyedeki (parent olmayan) lokasyonları getir
        public async Task<List<WarehouseLocation>> GetRootLocationsAsync(int warehouseId)
        {
            return await _context.WarehouseLocations
                .Where(wl => wl.WarehouseId == warehouseId && wl.ParentId == null && !wl.IsDeleted)
                .ToListAsync();
        }

        // 🔹 Belirli bir lokasyonun çocuklarını getir
        public async Task<List<WarehouseLocation>> GetChildrenAsync(int parentId)
        {
            return await _context.WarehouseLocations
                .Where(wl => wl.ParentId == parentId && !wl.IsDeleted)
                .ToListAsync();
        }

        // 🔹 Belirli bir lokasyonun altı var mı kontrol et
        public async Task<bool> HasChildrenAsync(int id)
        {
            return await _context.WarehouseLocations
                .AnyAsync(wl => wl.ParentId == id && !wl.IsDeleted);
        }

        // 🔹 Belirli bir lokasyonu ID'ye göre getir
        public async Task<WarehouseLocation?> GetLocationByIdAsync(int locationId)
        {
            return await _context.WarehouseLocations
                .FirstOrDefaultAsync(wl => wl.Id == locationId && !wl.IsDeleted);
        }

        //  StorageType'a göre filtreleme
        public async Task<List<WarehouseLocation>> GetLocationsByStorageTypeAsync(int warehouseId, StorageType storageType)
        {
            return await _context.WarehouseLocations
                .Where(wl => wl.WarehouseId == warehouseId && wl.StorageType == storageType && !wl.IsDeleted)
                .ToListAsync();
        }

        //  Level + StorageType'a göre filtreleme
        public async Task<List<WarehouseLocation>> GetLocationsByLevelAndStorageTypeAsync(int warehouseId, LocationLevel level, StorageType storageType)
        {
            return await _context.WarehouseLocations
                .Where(wl => wl.WarehouseId == warehouseId &&
                             wl.Level == level &&
                             wl.StorageType == storageType &&
                             !wl.IsDeleted)
                .ToListAsync();
        }
        public async Task<int?> GetLocationIdByLevelPathAsync(int warehouseId, List<string> levelPath)
        {
            if (levelPath == null || levelPath.Count == 0)
                return null;

            var locations = await _context.WarehouseLocations
                .Where(wl => wl.WarehouseId == warehouseId && !wl.IsDeleted)
                .ToListAsync();

            WarehouseLocation? current = null;

            foreach (var levelName in levelPath)
            {
                current = locations.FirstOrDefault(l =>
                    l.Name.Equals(levelName, StringComparison.OrdinalIgnoreCase) &&
                    l.ParentId == (current?.Id ?? null));

                if (current == null)
                    return null;
            }

            return current?.Id;
        }

    }
}