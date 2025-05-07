using StockManagemant.DataAccess.LoggingModels;

namespace StockManagemant.Logging
{
    public interface IDbLogger
    {
        Task LogAsync(AppLogEntry logEntry);
    }
}