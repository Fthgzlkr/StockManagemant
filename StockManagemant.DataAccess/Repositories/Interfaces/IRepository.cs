using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id); // Soft delete işlemi burada olacak
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task RestoreAsync(int id); // Silinmiş veriyi geri getirme
    }
}