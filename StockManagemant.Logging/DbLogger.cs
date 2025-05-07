using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.LoggingModels;

namespace StockManagemant.Logging
{
    public class DbLogger : IDbLogger
    {
        private readonly AppDbContext _context;

        public DbLogger(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(AppLogEntry logEntry)
        {
            _context.AppLogs.Add(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}