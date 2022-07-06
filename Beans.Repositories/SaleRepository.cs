using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using Dapper;

using System.Data.SqlClient;

namespace Beans.Repositories;
public class SaleRepository : RepositoryBase<SaleEntity>, ISaleRepository
{
    private readonly IBeanRepository _beanRepository;

    public SaleRepository(IDatabase database, IBeanRepository beanRepository) : base(database) => _beanRepository = beanRepository;

    public override async Task<IEnumerable<SaleEntity>> GetAsync(string sql, params QueryParameter[] parameters)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var sales = await conn.QueryAsync<SaleEntity>(sql, BuildParameters(parameters));
            if (sales is not null && sales.Any())
            {
                foreach (var sale in sales)
                {
                    sale.Bean = await _beanRepository.ReadAsync(sale.BeanId);
                }
            }
            return sales!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public override async Task<SaleEntity?> ReadAsync(string sql, params QueryParameter[] parameters)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<SaleEntity>(sql, BuildParameters(parameters));
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

    public async Task<IEnumerable<SaleEntity>> GetForUserAsync(int userid) => await GetAsync($"select * from Sales where UserId={userid};");

    public async Task<IEnumerable<SaleEntity>> GetForUserAsync(int userid, int days)
    {
        var date = DateTime.UtcNow.AddDays(-(days - 1));
        var sql = $"select * from Sales where UserId={userid} and SaleDate >= '{date:yyyy-MM-dd}' order by SaleDate desc;";
        return await GetAsync(sql);
    }

    public async Task<IEnumerable<SaleEntity>> GetForUserAndBeanAsync(int userid, int beanid) =>
      await GetAsync($"select * from Sales where UserId={userid} and BeanId={beanid}");

    public async Task<IEnumerable<SaleEntity>> GetForUserAndBeanAsync(int userid, int beanid, int days)
    {
        var date = DateTime.UtcNow.AddDays(-(days - 1));
        var sql = $"select * from Sales where UserId={userid} and BeanId={beanid} and SaleDate >= '{date:yyyy-MM-dd}' order by SaleDate desc;";
        return await GetAsync(sql);
    }

    private async Task<int> GetCountAsync(string sql)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            return await conn.QueryFirstOrDefaultAsync<int>(sql);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<bool> UserHasSalesAsync(int userid) => await GetCountAsync($"Select count(*) from Sales where UserId={userid};") > 0;

    public async Task<bool> UserHasSoldAsync(int userid, int beanid) => await GetCountAsync($"select count(*) from Sales where UserId={userid} and BeanId={beanid};") > 0;

    public async Task<bool> BeanHasSalesAsync(int beanid) => await GetCountAsync($"select count(*) from Sales where BeanId={beanid};") > 0;

    private async Task<decimal> ProfitOrLossAsync(string sql)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            var sales = await GetAsync(sql);
            if (sales is not null && sales.Any())
            {
                return sales.Sum(x => x.GainOrLoss);
            }
            return 0M;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<decimal> ProfitOrLossAsync(int userid) => await ProfitOrLossAsync($"select * from Sales where UserId={userid};");

    public async Task<decimal> ProfitOrLossAsync(int userid, int beanid) => await ProfitOrLossAsync($"select * from Sales where UserId={userid} and BeanId={beanid};");

    public async Task<decimal> ProfitOrLossAsync(int userid, DateTime startdate, DateTime enddate)
    {
        var start = startdate.ToString("yyyy-MM-dd");
        var end = enddate.ToString("yyyy-MM-dd");
        return await ProfitOrLossAsync($"select * from Sales where UserId={userid} and SaleDate >= '{start}' and SaleDate <= '{end}';");
    }

    public async Task<decimal> ProfitOrLossAsync(int userid, int beanid, DateTime startdate, DateTime enddate)
    {
        var start = startdate.ToString("yyyy-MM-dd");
        var end = enddate.ToString("yyyy-MM-dd");
        return await ProfitOrLossAsync($"select * from Sales where UserId={userid} and BeanId={beanid} and SaleDate >= '{start}' and SaleDate <= '{end}';");
    }
}
