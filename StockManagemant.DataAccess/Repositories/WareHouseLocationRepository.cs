using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories.Interfaces;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories
{
    public class WareHouseLocationRepository : Repository<WarehouseLocation>, IWareHouseLocationRepository
    {
        public WareHouseLocationRepository(AppDbContext context) : base(context) { }



       public async Task<IEnumerable<object>> GetCorridorsByWarehouseIdAsync(int warehouseId)
        {
            return await _context.WarehouseLocations
                .Where(wl => wl.WarehouseId == warehouseId && wl.Shelf == null && wl.Bin == null && !wl.IsDeleted)
                .Select(wl => new {
                    id = wl.Id,
                    name = wl.Corridor
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetShelvesByWarehouseAsync(int warehouseId, string corridor)
        {
            return await _context.WarehouseLocations
                .Where(wl => wl.WarehouseId == warehouseId && wl.Corridor == corridor && wl.Shelf != null && wl.Bin == null && !wl.IsDeleted)
                .Select(wl => new {
                    id = wl.Id,
                    name = wl.Shelf
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetBinsByWarehouseAsync(int warehouseId, string corridor, string shelf)
        {
            return await _context.WarehouseLocations
                .Where(wl => wl.WarehouseId == warehouseId && wl.Corridor == corridor && wl.Shelf == shelf && wl.Bin != null && !wl.IsDeleted)
                .Select(wl => new {
                    id = wl.Id,
                    name = wl.Bin
                })
                .ToListAsync();
        }

        public async Task<int?> GetLocationIdAsync(int warehouseId, string corridor, string? shelf, string? bin)
        {
            var location = await _context.WarehouseLocations
                .Where(x =>
                    x.WarehouseId == warehouseId &&
                    x.Corridor == corridor &&
                    x.Shelf == shelf &&
                    x.Bin == bin &&
                    !x.IsDeleted)
                .FirstOrDefaultAsync();

            return location?.Id;
        }
    }
}