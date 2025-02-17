using StockManagemant.DataAccess.Context;
using StockManagemant.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories
{
    public class CategoryRepository : Repository<Category>
    {
        public CategoryRepository(AppDbContext context) : base(context) { }

        // ✅ Toplam aktif kategori sayısını getir (Silinmiş olanlar dahil edilmez)
        public async Task<int> GetTotalCategoryCountAsync()
        {
            return await _context.Categories.CountAsync(c => !c.IsDeleted);
        }

        // ✅ İsme göre kategori getir (Generic repository’de yok, özel metod)
        public async Task<Category> GetByNameAsync(string name)
        {
            return await _context.Categories
                                 .Where(c => !c.IsDeleted && c.Name == name)
                                 .FirstOrDefaultAsync();
        }
    }

}
