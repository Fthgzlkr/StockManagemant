using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public interface ILogManager
{
    Task LogAsync(AppLogDto logDto);
    Task<(List<AppLogDto> Logs, int TotalCount)> GetLogsPagedAsync(int page, int pageSize);
}
}