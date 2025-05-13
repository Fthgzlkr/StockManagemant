using StockManagemant.DataAccess.LoggingModels;

namespace StockManagemant.Logging
{
  public interface ILogRepository
    {
        Task LogAsync(AppLogEntry logEntry);
        Task<List<AppLogEntry>> GetLogsPagedAsync(int page, int pageSize);
        Task<int> GetLogsCountAsync();
    }
}