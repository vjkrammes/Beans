using Beans.Common;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using Dapper;

using System.Data;
using System.Data.SqlClient;

namespace Beans.Repositories;
public class UserRepository : RepositoryBase<UserEntity>, IUserRepository
{
    public UserRepository(IDatabase database) : base(database) { }

    public async Task<UserEntity?> ReadAsync(string email) =>
      await ReadAsync($"Select * from Users where Email=@email;", new QueryParameter("email", email, DbType.String));

    public async Task<UserEntity?> ReadByIdentifierAsync(string identifier) =>
      await ReadAsync($"Select * from Users where Identifier=@id", new QueryParameter("id", identifier, DbType.String));

    public async Task<DateTime> JoinedAsync(int userid)
    {
        var user = await ReadAsync(userid);
        return user?.DateJoined ?? default;
    }

    public async Task<DalResult> LoanAsync(int userid, decimal amount)
    {
        if (amount == 0)
        {
            return DalResult.Success;
        }
        if (amount < 0)
        {
            return DalResult.FromException(new ArgumentException("The amount is invalid. It must be a positive amount."));
        }
        var user = await ReadAsync(userid);
        if (user is null)
        {
            return DalResult.NotFound(new ArgumentException($"No user with the id '{userid}' was found"));
        }
        user.OwedToExchange += amount;
        user.Balance += amount;
        return await UpdateAsync(user);
    }

    public async Task<DalResult> RepayAsync(int userid, decimal amount)
    {
        if (amount == 0)
        {
            return DalResult.Success;
        }
        if (amount < 0)
        {
            return DalResult.FromException(new ArgumentException("The amount is invalid. It must be a positive amount."));
        }
        var user = await ReadAsync(userid);
        if (user is null)
        {
            return DalResult.NotFound(new ArgumentException($"No user with the id '{userid}' was found"));
        }
        user.OwedToExchange -= amount;
        if (user.OwedToExchange < 0M)
        {
            user.OwedToExchange = 0M;
        }
        user.Balance -= amount;
        if (user.Balance < 0M)
        {
            user.Balance = 0M;
        }
        return await UpdateAsync(user);
    }

    public async Task<DalResult> ResetUsersAsync()
    {
        //
        // resets all loan balances, and resets Balances to startingBalance
        //
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var sql = $"Select Value from Settings where Name='{Constants.STARTING_BALANCE}';";
            var sb = await conn.ExecuteScalarAsync<int>(sql);
            if (sb == 0)
            {
                sb = Constants.DefaultStartingBalance;
            }
            sql = $"Update Users set Balance={sb}, OwedToExchange=0;";
            await conn.ExecuteAsync(sql);
            return DalResult.Success;
        }
        catch (Exception ex)
        {
            return DalResult.FromException(ex);
        }
    }

    public async Task<DalResult> ToggleAdminAsync(int userid)
    {
        if (userid <= 0)
        {
            return DalResult.NotFound(new Exception($"Invalid user id"));
        }
        var user = await ReadAsync(userid);
        if (user is null)
        {
            return DalResult.NotFound(new Exception($"No user with the id '{userid}' was found"));
        }
        if (user.IsAdmin)
        {
            using var conn = new SqlConnection(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var sql = "select count(*) from Users where IsAdmin=1;";
                var count = await conn.ExecuteScalarAsync<int>(sql);
                if (count <= 1)
                {
                    return DalResult.FromException(new InvalidOperationException("Can't demote the last admin user"));
                }
            }
            catch (Exception ex)
            {
                return DalResult.FromException(ex);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
        user.IsAdmin = !user.IsAdmin;
        return await UpdateAsync(user);
    }
}
