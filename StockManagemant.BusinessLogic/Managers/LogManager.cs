using AutoMapper;
using StockManagemant.Entities.DTO;
using StockManagemant.Logging;
using StockManagemant.DataAccess.LoggingModels;


namespace StockManagemant.Business.Managers
{
   public class LogManager : ILogManager
{
    private readonly ILogRepository _logRepository;
    private readonly IMapper _mapper;

    public LogManager(ILogRepository logRepository, IMapper mapper)
    {
        _logRepository = logRepository;
        _mapper = mapper;
    }

    public async Task LogAsync(AppLogDto logDto)
    {
        var entity = _mapper.Map<AppLogEntry>(logDto);
        await _logRepository.LogAsync(entity);
    }

 

    public async Task<(List<AppLogDto> Logs, int TotalCount)> GetLogsPagedAsync(int page, int pageSize)
    {
        var logs = await _logRepository.GetLogsPagedAsync(page, pageSize);
        var totalCount = await _logRepository.GetLogsCountAsync();
        return (_mapper.Map<List<AppLogDto>>(logs), totalCount);
    }

    
}
}
