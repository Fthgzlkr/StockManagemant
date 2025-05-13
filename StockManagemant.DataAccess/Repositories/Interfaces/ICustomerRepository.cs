using StockManagemant.DataAccess.Repositories.Interfaces;
using StockManagemant.Entities.Models;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface ICustomerRepository : IRepository<Customers>
    {
        // Eğer özel Customer metodları eklemek istersen burada tanımlayacağız
    }
}