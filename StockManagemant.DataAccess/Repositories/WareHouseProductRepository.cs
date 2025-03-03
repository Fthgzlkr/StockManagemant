using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories.Filters;
using StockManagemant.Entities.Models;

namespace StockManagemant.DataAccess.Repositories
{
    public class WarehouseProductRepository : Repository<WarehouseProduct>
    {
        public WarehouseProductRepository(AppDbContext context) : base(context) { }

        //  Belirli bir ürünün toplam stok miktarını getir (Sadece aktif kayıtlar)
        public async Task<int> GetTotalStockByProductIdAsync(int productId)
        {
            return await _context.WarehouseProducts
                .Where(wp => wp.ProductId == productId && !wp.IsDeleted)
                .SumAsync(wp => wp.StockQuantity);
        }

        //  Belirli bir depodaki ürünleri getir (Sadece aktif kayıtlar)
        public async Task<IEnumerable<WarehouseProduct>> GetProductsByWarehouseIdAsync(int warehouseId)
        {
            return await _context.WarehouseProducts
                .Where(wp => wp.WarehouseId == warehouseId && !wp.IsDeleted)
                .Include(wp => wp.Product)
                .ThenInclude(p => p.Category) // Ürün bilgilerini yükle ve kategorisini de dahil et
                .ToListAsync();
        }



        //Normal Productda yapılanlar 

        //  Toplam depo ürünü sayısını getir 
        public async Task<int> GetTotalCountAsync(WarehouseProductFilter filter ,int WareHouseId)
        {
            return await _context.WarehouseProducts
                .Where(wp =>wp.WarehouseId==WareHouseId)
                .Where(filter.GetFilterExpression())
                .CountAsync();
        }

        //  Sayfalama ile depodaki ürünleri getir 
        public async Task<IEnumerable<WarehouseProduct>> GetPagedWarehouseProductsWithStockAsync(WarehouseProductFilter filter, int warehouseId, int page, int pageSize)
        {
            return await _context.WarehouseProducts
                .Where(wp => wp.WarehouseId == warehouseId) // ✅ Depo ID'ye göre filtreleme eklendi
                .Where(filter.GetFilterExpression())
                .OrderBy(wp => wp.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(wp => new WarehouseProduct
                {
                    ProductId = wp.ProductId,
                    Product = new Product
                    {
                        Name = wp.Product.Name,
                        Price = wp.Product.Price,   
                        Category = new Category
                        {
                            Name = wp.Product.Category.Name
                        }
                    },
                    WarehouseId = wp.WarehouseId,
                    Warehouse = new Warehouse
                    {
                        Name = wp.Warehouse.Name
                    },
                    StockQuantity = wp.StockQuantity
                })
                .ToListAsync();
        }


        
        public override async Task<WarehouseProduct> GetByIdAsync(int id)
        {
            return await _context.WarehouseProducts
                .Include(wp => wp.Product)
                .ThenInclude(p => p.Category) // Ürün bilgilerini yükle ve kategorisini de dahil et
                .Include(wp => wp.Warehouse)
                .FirstOrDefaultAsync(wp => wp.Id == id && !wp.IsDeleted);
        }

        //Fiş oluştururken kullanacağımız depoya göre ürünü id ile getiren metod
        public async Task<WarehouseProduct> GetProductInWarehouseByIdAsync(int warehouseId, int productId)
        {
            return await _context.WarehouseProducts
                .Include(wp => wp.Product)
                .ThenInclude(p => p.Category)
                .Include(wp => wp.Warehouse)
                .FirstOrDefaultAsync(wp => wp.WarehouseId == warehouseId && wp.ProductId == productId && !wp.IsDeleted);
        }



        // ID'ye göre depo ürününü getir (Silinmişler dahil)
        public async Task<WarehouseProduct> GetByIdWithDeletedAsync(int id)
        {
            return await _context.WarehouseProducts
                .IgnoreQueryFilters()
                .Include(wp => wp.Product)
                .ThenInclude(p => p.Category) // Ürün bilgilerini yükle ve kategorisini de dahil et
                .Include(wp => wp.Warehouse)
                .FirstOrDefaultAsync(wp => wp.Id == id);
        }
    }
}
