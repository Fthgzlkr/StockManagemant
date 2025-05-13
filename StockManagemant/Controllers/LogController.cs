using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StockManagemant.Business.Managers;

namespace StockManagemant.Controllers
{
    
  public class LogController : Controller
{
    private readonly ILogManager _logManager;

    public LogController(ILogManager logManager)
    {
        _logManager = logManager;
    }

    public IActionResult Index()
    {
        return View(); // Views/Log/Index.cshtml
    }

    [HttpGet]
 
public async Task<IActionResult> GetLogs(int page = 1, int rows = 20)
{
    var (logs, totalCount) = await _logManager.GetLogsPagedAsync(page, rows);
    var totalPages = (int)Math.Ceiling((double)totalCount / rows);

    var jsonData = new
    {
        total = totalPages,
        page = page,
        records = totalCount,
        rows = logs.Select((log, index) => new
        {
            id = index + 1,
            timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            level = log.Level,
            action = log.Action,
            target = log.Target,
            fileName = log.FileName,
            message = log.Message
        })
    };

    return Json(jsonData);
}
}
}
