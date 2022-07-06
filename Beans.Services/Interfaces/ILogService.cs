using Beans.Models;

namespace Beans.Services.Interfaces;
public interface ILogService : IDataService<LogModel>
{
    Task<IEnumerable<LogModel>> GetForDateAsync(DateTime date);
    Task<IEnumerable<LogModel>> GetForDateRangeAsync(DateTime start, DateTime end);
}
