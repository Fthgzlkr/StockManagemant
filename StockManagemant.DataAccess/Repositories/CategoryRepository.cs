using StockManagemant.DataAccess.Context;
using StockManagemant.Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockManagemant.DataAccess.Repositories.Interfaces;

namespace StockManagemant.DataAccess.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context) { }

        public async Task<int> GetTotalCategoryCountAsync()
        {
            return await _context.Categories.CountAsync(c => !c.IsDeleted);
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _context.Categories
                                 .Where(c => !c.IsDeleted && c.Name == name)
                                 .FirstOrDefaultAsync();
        }
    }
}