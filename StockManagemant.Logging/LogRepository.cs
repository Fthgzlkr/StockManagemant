using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Context;
using StockManagemant.DataAccess.LoggingModels;

namespace StockManagemant.Logging
{
    public class LogRepository : ILogRepository
    {
        private readonly AppDbContext _context;

        public LogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(AppLogEntry logEntry)
        {
            _context.AppLogs.Add(logEntry);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AppLogEntry>> GetAllLogsAsync()
        {
            return await _context.AppLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AppLogEntry>> GetLogsPagedAsync(int page, int pageSize)
        {
            return await _context.AppLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetLogsCountAsync()
        {
            return await _context.AppLogs.CountAsync();
        }
    }
}