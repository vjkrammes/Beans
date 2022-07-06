using Beans.Common;
using Beans.Common.Interfaces;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using Dapper;
using Dapper.Contrib.Extensions;

using System.Data.SqlClient;

namespace Beans.Repositories;
public class MovementRepository : RepositoryBase<MovementEntity>, IMovementRepository
{
    private readonly IBeanRepository _beanRepository;
    private readonly INormalRandom _normalRandom;
    private readonly IBreakpointManager _breakpointManager;

    public MovementRepository(IDatabase database, IBeanRepository beanRepository, INormalRandom normalRandom, IBreakpointManager breakpointManager) : base(database)
    {
        _beanRepository = beanRepository;
        _normalRandom = normalRandom;
        _breakpointManager = breakpointManager;
    }

    public override async Task<IEnumerable<MovementEntity>> GetAsync(string sql, params QueryParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentNullException(nameof(sql));
        }
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryAsync<MovementEntity>(sql, BuildParameters(parameters));
            if (ret is not null && ret.Any())
            {
                foreach (var entity in ret)
                {
                    entity.Bean = await _beanRepository.ReadAsync(entity.BeanId);
                }
            }
            return ret!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<MovementEntity>> GetForBeanAsync(int beanid) =>
      await GetAsync($"select * from Movements where BeanId={beanid} order by MovementDate desc;");

    public async Task<IEnumerable<MovementEntity>> GetForBeanAsync(int beanid, int days)
    {
        if (days <= 0)
        {
            return new List<MovementEntity>();
        }
        return await GetAsync($"select top({days}) * from Movements where BeanId={beanid} order by MovementDate desc;");
    }

    public override async Task<MovementEntity?> ReadAsync(string sql, params QueryParameter[] parameters)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<MovementEntity>(sql, BuildParameters(parameters));
            if (ret is not null)
            {
                ret.Bean = await _beanRepository.ReadAsync(ret.BeanId);
            }
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<DalResult> InsertAsync(MovementEntity movement, BeanEntity bean)
    {
        if (movement is null)
        {
            throw new ArgumentNullException(nameof(movement));
        }
        if (bean is null)
        {
            throw new ArgumentNullException(nameof(bean));
        }
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            var result = await conn.InsertAsync(movement, transaction: transaction);
            movement.Id = result;
            var beanresult = await conn.UpdateAsync(bean, transaction: transaction);
            await transaction.CommitAsync();
            return beanresult ? DalResult.Success : DalResult.NotFound();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return DalResult.FromException(ex);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<MovementEntity>> TopForBeanAsync(int beanid, int count)
    {
        if (beanid <= 0 || count <= 0)
        {
            return new List<MovementEntity>();
        }
        var sql = $"select top {count} * from Movements where BeanId={beanid} order by MovementDate desc;";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var entities = await conn.QueryAsync<MovementEntity>(sql);
            if (entities is not null && entities.Any())
            {
                entities.ForEach(async x => x.Bean = await _beanRepository.ReadAsync(x.BeanId));
            }
            return entities!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<MovementEntity>> HistoryAsync(int beanid, DateTime date)
    {
        if (beanid <= 0)
        {
            return new List<MovementEntity>();
        }
        var sql = $"select * from Movements where BeanId={beanid} and MovementDate >= '{date:yyyy-MM-dd}' order by MovementDate desc;";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var entities = await conn.QueryAsync<MovementEntity>(sql);
            if (entities is not null && entities.Any())
            {
                entities.ForEach(async x => x.Bean = await _beanRepository.ReadAsync(x.BeanId));
            }
            return entities!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<int>> BeanIdsAsync()
    {
        var sql = "select distinct BeanId from Movements order by BeanId;";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            return await conn.QueryAsync<int>(sql);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<MovementEntity>> MostRecentAsync()
    {
        var ids = (await BeanIdsAsync()).ToArray();
        return await MostRecentAsync(ids);
    }

    public async Task<IEnumerable<MovementEntity>> MostRecentAsync(int[] ids)
    {
        var ret = new List<MovementEntity>();
        if (ids is null || !ids.Any())
        {
            return ret;
        }
        foreach (var id in ids)
        {
            ret.Add(await MostRecentAsync(id));
        }
        return ret;
    }

    public async Task<MovementEntity> MostRecentAsync(int id)
    {
        var sql = $"select top 1 * from Movements where BeanId={id} order by MovementDate desc;";
        var beansql = $"select * from Beans where Id={id};";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var movement = await conn.QueryFirstOrDefaultAsync<MovementEntity>(sql);
            if (movement is not null)
            {
                movement.Bean = await conn.QueryFirstOrDefaultAsync<BeanEntity>(beansql);
            }
            return movement!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<MovementEntity> ReadForDateAsync(int beanid, DateTime date)
    {
        var querydate = date.ToString("yyyy-mm-DD");
        var sql = $"select * from Movements where BeanId={beanid} and CAST(MovementDate as Date)='{querydate}'";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            return await conn.QueryFirstOrDefaultAsync<MovementEntity>(sql);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<bool> BeanHasMovementsAsync(int beanid)
    {
        var sql = $"select count(*) from Movements where BeanId={beanid};";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            var count = await conn.QueryFirstOrDefaultAsync<int>(sql);
            return count > 0;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<DateTime> LowestDateAsync()
    {
        var sql = $"select top 1 MovementDate from Movements order by MovementDate;";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            return await conn.QueryFirstOrDefaultAsync<DateTime>(sql);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private async Task<bool> MovementExistsAsync(int beanid, DateTime date)
    {
        var sql = $"select count(*) from Movements where BeanId={beanid} and CAST(MovementDate as Date) = '{date:yyyy-MM-dd}';";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var result = await conn.QueryFirstOrDefaultAsync<int>(sql);
            return result > 0;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<DalResult> MakeMovementAsync(int beanid, decimal lowestValue, DateTime date)
    {
        if (beanid <= 0)
        {
            return DalResult.NotFound();
        }
        var bean = await _beanRepository.ReadAsync(beanid);
        if (bean is null)
        {
            return DalResult.NotFound();
        }
        if (await MovementExistsAsync(beanid, date))
        {
            return DalResult.Success;
        }
        if (lowestValue < 0)
        {
            lowestValue = Constants.MinimumBeanPrice;
        }
        var startingPrice = bean.Price;
        var breakpoint = _breakpointManager.GenerateBreakpoint();
        var multiplier = _breakpointManager.GetMultiplier(breakpoint);
        var amount = (double)startingPrice * (_normalRandom.Next() / 100) * multiplier;
        bean.Price += (decimal)amount;
        if (bean.Price < lowestValue)
        {
            bean.Price = lowestValue;
        }
        var movement = new MovementEntity
        {
            Id = 0,
            BeanId = beanid,
            MovementDate = date,
            Open = startingPrice,
            Close = bean.Price,
            Movement = bean.Price - startingPrice,
            MovementType = _breakpointManager.GetMovementType(breakpoint)
        };
        var result = await InsertAsync(movement, bean);
        return result;
    }

    public async Task<DalResult> CatchupAsync(int beanid, decimal lowestValue, DateTime startDate)
    {
        var now = DateTime.UtcNow;
        var date = startDate;
        while (date.Date <= now.Date)
        {
            if (await MovementExistsAsync(beanid, date))
            {
                date = date.AddDays(1);
                continue;
            }
            var result = await MakeMovementAsync(beanid, lowestValue, date);
            if (!result.Successful)
            {
                return result;
            }
            date = date.AddDays(1);
        }
        return DalResult.Success;
    }

    public async Task<DalResult> MoveAsync(int beanid, decimal lowestValue, DateTime date) => await MakeMovementAsync(beanid, lowestValue, date);

    private async Task<IEnumerable<decimal>> GetMovementsByDaysAsync(int beanid, int days)
    {
        var sql = $"select top {days} * from Movements where BeanId={beanid} order by MovementDate desc;";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var entities = await conn.QueryAsync<MovementEntity>(sql);
            return entities.Where(x => x.Movement != 0M).Select(x => x.Movement);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<decimal> GetMinRangeAsync(int beanid, int days)
    {
        var movements = await GetMovementsByDaysAsync(beanid, days);
        return movements?.OrderBy(x => x).FirstOrDefault() ?? 0M;
    }

    public async Task<decimal> GetAverageRangeAsync(int beanid, int days)
    {
        var movements = await GetMovementsByDaysAsync(beanid, days);
        return movements?.Average() ?? 0M;
    }

    public async Task<decimal> GetMaxRangeAsync(int beanid, int days)
    {
        var movements = await GetMovementsByDaysAsync(beanid, days);
        return movements?.OrderBy(x => x).LastOrDefault() ?? 0M;
    }

    public async Task<decimal> GetLargestMovementAsync(int beanid)
    {
        var sql = $"select top 1 Movement from Movements where BeanId={beanid} order by Movement desc;";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            return await conn.QueryFirstOrDefaultAsync<decimal>(sql);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<decimal> GetStandardDeviationAsync(int beanid, int days)
    {
        var movements = await GetMovementsByDaysAsync(beanid, days);
        return movements?.StandardDeviation() ?? 0M;
    }
}
