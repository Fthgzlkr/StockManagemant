using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {

        Task<int> GetTotalCategoryCountAsync();
        Task<Category> GetByNameAsync(string name);
    }
}
