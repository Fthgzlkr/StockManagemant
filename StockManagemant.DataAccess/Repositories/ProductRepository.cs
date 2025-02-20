using StockManagemant.DataAccess.Filters;
using StockManagemant.DataAccess.Context;
using StockManagemant.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace StockManagemant.DataAccess.Repositories
{
    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(AppDbContext context) : base(context) { }

        // ✅ Toplam aktif ürün sayısını getir (Silinmiş olanlar dahil edilmez)
        public async Task<int> GetTotalCountAsync(ProductFilter filter)
        {
            return await _context.Products
                .Where(filter.GetFilterExpression())
                .CountAsync();
        }

        // ✅ Sayfalama ile ürünleri getir (Sadece silinmemiş ürünleri getir)
        public async Task<IEnumerable<Product>> GetPagedProductsAsync(ProductFilter filter, int page, int pageSize)
        {
            return await _context.Products
                .Where(filter.GetFilterExpression())
                .Include(p => p.Category)
                .OrderBy(p => p.Id) // Varsayılan sıralama
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // ✅ ID'ye göre sadece aktif (silinmemiş) ürünü getir (Override edildi)
        public override async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        // ✅ ID'ye göre ürünü getir (Silinmişler de dahil) bunu fiş ürünleri getirirken silinmişleride getirmek için kullan
        public async Task<Product> GetByIdWithDeletedAsync(int id)
        {
            return await _context.Products
                .IgnoreQueryFilters() // Soft delete olanları da getir
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


    }
}
