using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using Dapper;
using Dapper.Contrib.Extensions;

using System.Data;
using System.Data.SqlClient;

namespace Beans.Repositories;
public class BeanRepository : RepositoryBase<BeanEntity>, IBeanRepository
{
    public BeanRepository(IDatabase database) : base(database) { }

    private async Task<IEnumerable<int>> GetIdsAsync(string sql)
    {
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryAsync<int>(sql);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<IEnumerable<int>> BeanIdsAsync()
    {
        var sql = "select Id from Beans order by Id;";
        return await GetIdsAsync(sql);
    }

    public async Task<IEnumerable<int>> BeanIdsAsync(int userid)
    {
        var sql = $"select distinct BeanId from Holdings where UserId={userid} order by BeanId;";
        return await GetIdsAsync(sql);
    }

    public async Task<BeanEntity?> ReadAsync(string name)
    {
        var sql = "select * from Beans where Name=@name;";
        return await ReadAsync(sql, new QueryParameter("name", name, DbType.String));
    }

    public async Task<long> PlayerHeldAsync(int beanid)
    {
        var sql = $"select Held from Beans where Id={beanid};";
        using var conn = new SqlConnection(ConnectionString);
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

    public async Task<long> ExchangeHeldAsync(int beanid)
    {
        var sql = $"select ExchangeHeld from Beans where Id={beanid};";
        using var conn = new SqlConnection(ConnectionString);
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

    public async Task<decimal> CapitalizationAsync()
    {
        var ret = 0M;
        var ids = await BeanIdsAsync();
        ids.ForEach(async x => ret += await CapitalizationAsync(x));
        return ret;
    }

    public async Task<decimal> CapitalizationAsync(int beanid)
    {
        var bean = await ReadAsync(beanid);
        if (bean is not null)
        {
            return bean.Price * bean.Quantity;
        }
        return 0M;
    }

    public async Task<DalResult> SellToExchangeAsync(int holdingid, long quantity)
    {
        if (holdingid <= 0)
        {
            return new(DalErrorCode.Invalid, new("Holding id is invalid"));
        }
        if (quantity <= 0)
        {
            return new(DalErrorCode.Invalid, new("Quantity is invalid"));
        }
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            var sql = $"select * from holdings where id={holdingid};";
            var holding = await conn.QueryFirstOrDefaultAsync<HoldingEntity>(sql, transaction: transaction);
            if (holding is null)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NotFound, new($"No holding with the id '{holdingid}' was found"));
            }
            if (holding.Quantity < quantity)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NSF, new("Insufficient beans in that holding"));
            }
            sql = $"select * from Users where Id={holding.UserId};";
            var user = await conn.QueryFirstOrDefaultAsync<UserEntity>(sql, transaction: transaction);
            if (user is null)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NotFound, new($"No user with the email id '{holding.UserId}' was found"));
            }
            sql = $"select * from Offers where UserId={user.Id} and HoldingId={holdingid};";
            var offers = await conn.QueryAsync<OfferEntity>(sql, transaction: transaction);
            if (offers is not null && offers.Any())
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.Invalid, new Exception("Holding has associated offers"));
            }
            sql = $"select * from Beans where Id={holding.BeanId};";
            var bean = await conn.QueryFirstOrDefaultAsync<BeanEntity>(sql, transaction: transaction);
            if (bean is null)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NotFound, new($"No bean with the id '{holding.BeanId}' was found"));
            }
            var sale = new SaleEntity
            {
                Id = 0,
                UserId = user.Id,
                BeanId = bean.Id,
                Quantity = quantity,
                PurchaseDate = holding.PurchaseDate,
                CostBasis = holding.Price,
                SaleDate = DateTime.UtcNow,
                SalePrice = bean.Price,
                Bean = null,
            };
            bean.ExchangeHeld += quantity;
            bean.Held -= quantity;
            if (quantity == holding.Quantity)
            {
                sql = $"delete from holdings where Id={holdingid};";
                await conn.ExecuteAsync(sql, transaction: transaction);
            }
            else
            {
                holding.Quantity -= quantity;
                await conn.UpdateAsync(holding, transaction: transaction);
            }
            user.Balance += quantity * bean.Price;
            await conn.InsertAsync(sale, transaction: transaction);
            await conn.UpdateAsync(user, transaction: transaction);
            await conn.UpdateAsync(bean, transaction: transaction);
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

    public async Task<DalResult> BuyFromExchangeAsync(int userid, int beanid, long quantity)
    {
        if (userid <= 0)
        {
            return new(DalErrorCode.Invalid, new("User id is invalid"));
        }
        if (beanid <= 0)
        {
            return new(DalErrorCode.Invalid, new("Bean id is invalid"));
        }
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            var sql = $"select * from Users where Id={userid};";
            var user = await conn.QueryFirstOrDefaultAsync<UserEntity>(sql, transaction: transaction);
            if (user is null)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NotFound, new($"No user with the id '{userid}' was found"));
            }
            sql = $"select * from Beans where Id={beanid};";
            var bean = await conn.QueryFirstOrDefaultAsync<BeanEntity>(sql, transaction: transaction);
            if (bean is null)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NotFound, new($"No bean with the id '{beanid}' was found"));
            }
            if (quantity > bean.ExchangeHeld)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NSF, new("Insufficient outstanding beans for that quantity"));
            }
            if (quantity * bean.Price > user.Balance)
            {
                await transaction.RollbackAsync();
                return new(DalErrorCode.NSF, new("User has insufficient funds to buy that many beans"));
            }
            user.Balance -= quantity * bean.Price;
            bean.Held += quantity;
            bean.ExchangeHeld -= quantity;
            var holding = new HoldingEntity
            {
                Id = 0,
                UserId = userid,
                BeanId = beanid,
                PurchaseDate = DateTime.UtcNow,
                Price = bean.Price,
                Quantity = quantity,
                Bean = null
            };
            await conn.InsertAsync(holding, transaction: transaction);
            await conn.UpdateAsync(user, transaction: transaction);
            await conn.UpdateAsync(bean, transaction: transaction);
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
