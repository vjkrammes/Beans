
using Beans.Repositories.Entities;

namespace Beans.Repositories.Interfaces;
public interface ILogRepository : IRepository<LogEntity>
{
    Task<IEnumerable<LogEntity>> GetForDateAsync(DateTime date);
    Task<IEnumerable<LogEntity>> GetForDateRangeAsync(DateTime start, DateTime end);
}
