using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories.Interfaces;
using StockManagemant.Entities.Models;

namespace StockManagemant.DataAccess.Repositories
{
    public class CustomerRepository : Repository<Customers>, ICustomerRepository
    {
        public CustomerRepository(AppDbContext context) : base(context)
        {
        }

        // Eğer Customer'a özel metodlar eklenecekse burada override/ekstra yazacağız
    }
}