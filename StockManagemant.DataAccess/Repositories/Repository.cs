using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet
                .Where(e => EF.Property<bool>(e, "IsDeleted") == false)
                .ToListAsync();
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(e => EF.Property<int>(e, "Id") == id && EF.Property<bool>(e, "IsDeleted") == false)
                .FirstOrDefaultAsync();
        }

        public virtual  async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
                if (isDeletedProperty != null)
                {
                    isDeletedProperty.SetValue(entity, true);
                    _dbSet.Update(entity);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task RestoreAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
                if (isDeletedProperty != null)
                {
                    isDeletedProperty.SetValue(entity, false);
                    _dbSet.Update(entity);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet
                .Where(predicate)
                .Where(e => EF.Property<bool>(e, "IsDeleted") == false)
                .ToListAsync();
        }
    }
}
