using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IAppUserRepository : IRepository<AppUser>
    {
        Task<AppUser?> GetByUsernameAndPasswordAsync(string username, string password);
    }
}
