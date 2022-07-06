using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using Dapper;

using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Beans.Repositories;

public class HoldingRepository : RepositoryBase<HoldingEntity>, IHoldingRepository
{
    private readonly IBeanRepository _beanRepository;

    public HoldingRepository(IDatabase database, IBeanRepository beanRepository) : base(database) => _beanRepository = beanRepository;

    public override async Task<IEnumerable<HoldingEntity>> GetAsync(string sql, params QueryParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentNullException(nameof(sql));
        }
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryAsync<HoldingEntity>(sql, BuildParameters(parameters));
            if (ret is not null && ret.Any())
            {
                ret.ForEach(async x => x.Bean = await _beanRepository.ReadAsync(x.BeanId));
            }
            return ret!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<HoldingEntity>> GetForUserAsync(int userid) => await GetAsync($"select * from Holdings where UserId={userid} order by PurchaseDate desc;");

    public async Task<IEnumerable<HoldingEntity>> GetForBeanAsync(int beanid) => await GetAsync($"select * from Holdings where BeanId={beanid} order by PurchaseDate desc;");

    public async Task<IEnumerable<HoldingEntity>> GetForBeanAsync(int userid, int beanid)
    {
        var sql = $"select * from Holdings where UserId={userid} and BeanId={beanid} order by PurchaseDate desc;";
        return await GetAsync(sql);
    }

    public async Task<IEnumerable<HoldingEntity>> SearchAsync(int userid, int beanid, DateTime startDate, DateTime endDate)
    {
        StringBuilder sb = new($"select * from Holdings where UserId={userid}");
        if (beanid != 0)
        {
            sb.Append($" and BeanId={beanid}");
        }
        List<QueryParameter> parms = new();
        if (startDate != default)
        {
            sb.Append(" and CAST(PurchaseDate as Date) = CAST(@startDate as DATE)");
            parms.Add(new() { Name = "startDate", Value = startDate, Type = DbType.DateTime2 });
        }
        if (endDate != default)
        {
            sb.Append(" and CAST(PurchaseDate as Date) = CAST(@endDate as DATE)");
            parms.Add(new() { Name = "endDate", Value = endDate, Type = DbType.DateTime2 });
        }
        var sql = sb.ToString();
        return await GetAsync(sql, parms.ToArray());
    }

    public override async Task<HoldingEntity?> ReadAsync(string sql, params QueryParameter[] parameters)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<HoldingEntity>(sql, BuildParameters(parameters));
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

    public async Task<long> BeansHeldByUserAsync(int userid)
    {
        using var conn = new SqlConnection(ConnectionString);
        var sql = $"select COALESCE(sum(Quantity),0) from Holdings where UserId={userid};";
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<long>(sql);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<long> BeansHeldByUserAndBeanAsync(int userid, int beanid)
    {
        using var conn = new SqlConnection(ConnectionString);
        var sql = $"select COALESCE(sum(Quantity),0) from Holdings where UserId={userid} and BeanId={beanid};";
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<long>(sql);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<long> BeansHeldByBeanAsync(int beanid)
    {
        using var conn = new SqlConnection(ConnectionString);
        var sql = $"select COALESCE(sum(quantity),0) from Holdings where BeanId={beanid}";
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<long>(sql);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<CostBasis> GetCostBasisAsync(int userid)
    {
        var ret = new CostBasis
        {
            Type = CostBasisType.NoHoldings,
            Basis = 0M
        };
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var sql = $"select * from holdings where UserId={userid};";
            var holdings = (await conn.QueryAsync<HoldingEntity>(sql)).ToList();
            if (holdings is null || !holdings.Any())
            {
                return ret;
            }
            if (holdings.Count == 1)
            {
                ret.Type = CostBasisType.Basis;
                ret.Basis = holdings.First().Price;
            }
            else
            {
                ret.Type = CostBasisType.Average;
                var totalcost = holdings.Sum(x => x.Quantity * x.Price);
                ret.Basis = totalcost / holdings.Sum(x => x.Quantity);
            }
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<CostBasis> GetCostBasisAsync(int userid, int beanid)
    {
        var ret = new CostBasis
        {
            Type = CostBasisType.NoHoldings,
            Basis = 0M
        };
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var sql = $"select * from holdings where UserId={userid} and BeanId={beanid};";
            var holdings = (await conn.QueryAsync<HoldingEntity>(sql)).ToList();
            if (holdings is null || !holdings.Any())
            {
                return ret;
            }
            if (holdings.Count == 1)
            {
                ret.Basis = holdings[0].Price;
                ret.Type = CostBasisType.Basis;
            }
            else
            {
                var totalcost = holdings.Sum(x => x.Quantity * x.Price);
                ret.Basis = totalcost / holdings.Sum(x => x.Quantity);
                ret.Type = CostBasisType.Average;
            }
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<Dictionary<string, long>> BeansHeldAsync()
    {
        var ret = new Dictionary<string, long>();
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var sql = "Select Distinct BeanId from Holdings;";
            var ids = await conn.QueryAsync<int>(sql);
            if (ids is null || !ids.Any())
            {
                return ret;
            }
            ids.ForEach(async x =>
            {
                var bean = await _beanRepository.ReadAsync(x);
                if (bean is not null)
                {
                    var qsql = $"select COALESCE(sum(Quantity),0) from Holdings where BeanId={bean.Id};";
                    var quantity = await conn.QueryFirstOrDefaultAsync<long>(qsql);
                    ret[bean.Name] = quantity;
                }
            });
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    private async Task<int> GetCountAsync(string sql)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<int>(sql);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<bool> UserHasHoldingsAsync(int userid)
    {
        var sql = $"select count(*) from Holdings where UserId={userid};";
        return (await GetCountAsync(sql)) > 0;
    }

    public async Task<bool> UserHasHoldingsAsync(int userid, int beanid)
    {
        var sql = $"select count(*) from Holdings where UserId={userid} and BeanId={beanid};";
        return (await GetCountAsync(sql)) > 0;
    }

    public async Task<bool> BeanHasHoldingsAsync(int beanid)
    {
        var sql = $"select count(*) from Holdings where BeanId={beanid};";
        return (await GetCountAsync(sql)) > 0;
    }

    public async Task<int[]> GetHoldingsAsync(bool oldestFirst, int userid, int beanid, long quantity)
    {
        List<int> ids = new();
        var direction = oldestFirst ? "asc" : "desc";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var sql = $"select * from Holdings where UserId={userid} and BeanId={beanid} order by PurchaseDate {direction};";
            var holdings = (await conn.QueryAsync<HoldingEntity>(sql)).ToList();
            if (holdings is null || !holdings.Any())
            {
                return Array.Empty<int>();
            }
            long sum = 0;
            var ix = 0;
            while (sum < quantity && ix < holdings.Count)
            {
                sum += holdings[ix].Quantity;
                ids.Add(holdings[ix].Id);
                ix++;
            }
            return ids.ToArray();
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<long> HoldingCountAsync(int userid, int? beanid)
    {
        var sql = beanid.HasValue
          ? $"select count(*) from Holdings where UserId={userid} and BeanId={beanid.Value};"
          : $"select count(*) from Holdings where UserId={userid};";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.ExecuteScalarAsync<long>(sql);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<decimal> TotalValueAsync(int userid)
    {
        var holdings = (await GetForUserAsync(userid)).Select(x => new { x.BeanId, x.Quantity }).ToList();
        var ret = 0M;
        foreach (var holding in holdings)
        {
            var bean = await _beanRepository.ReadAsync(holding.BeanId);
            if (bean is not null)
            {
                ret += bean.Price * holding.Quantity;
            }
        }
        return ret;
    }

    public async Task<decimal> TotalValueAsync(int userid, int beanid)
    {
        var bean = await _beanRepository.ReadAsync(beanid);
        if (bean is not null)
        {
            var holdings = (await GetForBeanAsync(userid, beanid)).Select(x => x.Quantity).ToList();
            var ret = 0M;
            foreach (var holding in holdings)
            {
                ret += bean.Price * holding;
            }
            return ret;
        }
        return 0M;
    }

    public async Task<decimal> TotalCostAsync(int userid)
    {
        var holdings = await GetForUserAsync(userid);
        var ret = 0M;
        foreach (var holding in holdings)
        {
            ret += holding.Price * holding.Quantity;
        }
        return ret;
    }

    public async Task<decimal> TotalCostAsync(int userid, int beanid)
    {
        var holdings = await GetForBeanAsync(userid, beanid);
        var ret = 0M;
        foreach (var holding in holdings)
        {
            ret += holding.Price * holding.Quantity;
        }
        return ret;
    }

    public async Task<DalResult> ResetHoldingsAsync()
    {
        //
        // delete all notices, sales, offers, and holdings
        //
        // return all beans to the exchange
        //
        // does not change bean prices or movements
        //
        using var conn = new SqlConnection(ConnectionString);
        if (conn is null)
        {
            return DalResult.FromException(new ArgumentNullException(nameof(conn)));
        }
        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();
        if (transaction is null)
        {
            return DalResult.FromException(new ArgumentNullException(nameof(transaction)));
        }
        try
        {
            var sql = "delete from Notices;";
            await conn.ExecuteAsync(sql, transaction: transaction);
            sql = "delete from Sales;";
            await conn.ExecuteAsync(sql, transaction: transaction);
            sql = "delete from Offers;";
            await conn.ExecuteAsync(sql, transaction: transaction);
            sql = "delete from Holdings;";
            await conn.ExecuteAsync(sql, transaction: transaction);
            sql = "update Beans set ExchangeHeld=Quantity, Held=0;";
            await conn.ExecuteAsync(sql, transaction: transaction);
            await transaction.CommitAsync();
            return DalResult.Success;
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
}
