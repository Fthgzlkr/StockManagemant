using StockManagemant.DataAccess.Context;
using StockManagemant.Entities.Models;
using Microsoft.EntityFrameworkCore;

using StockManagemant.DataAccess.Repositories.Interfaces;

namespace StockManagemant.DataAccess.Repositories
{
    public class AppUserRepository : Repository<AppUser>, IAppUserRepository
    {
        public AppUserRepository(AppDbContext context) : base(context) { }

        public async Task<AppUser?> GetByUsernameAndPasswordAsync(string username, string password)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password && !u.IsDeleted);
        }
    }
}