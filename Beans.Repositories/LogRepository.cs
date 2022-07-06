using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using System.Data;

namespace Beans.Repositories;
public class LogRepository : RepositoryBase<LogEntity>, ILogRepository
{
    public LogRepository(IDatabase database) : base(database) { }

    public async Task<IEnumerable<LogEntity>> GetForDateAsync(DateTime date)
    {
        var sql = "select * from Logs where CAST(Timestamp as DATE) = CAST(@date as DATE);";
        return await GetAsync(sql, new QueryParameter("date", date, DbType.DateTime2));
    }

    public async Task<IEnumerable<LogEntity>> GetForDateRangeAsync(DateTime start, DateTime stop)
    {
        var sql = "select * from Logs where CAST(Timestamp as DATE) >= CAST(@start as DATE) and CAST(Timestamp as DATE) <= CAST(@stop as date);";
        return await GetAsync(sql,
          new QueryParameter("start", start, DbType.DateTime2),
          new QueryParameter("stop", stop, DbType.DateTime2));
    }
}
