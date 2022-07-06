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
public class SettingsRepository : ISettingsRepository
{
    private readonly IDatabase _database;

    public SettingsRepository(IDatabase database)
    {
        _database = database;
    }

    public async Task<int> CountAsync()
    {
        var sql = "select count(*) from Settings;";
        using var conn = new SqlConnection(_database.ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.ExecuteScalarAsync<int>(sql);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<DalResult> InsertAsync(SettingsEntity entity)
    {
        if (entity is null || string.IsNullOrWhiteSpace(entity.Name))
        {
            return new(DalErrorCode.Invalid, new Exception("Entity is null or key is missing"));
        }
        var existing = await ReadAsync(entity.Name);
        using var conn = new SqlConnection(_database.ConnectionString);
        if (existing is not null)
        {
            return new(DalErrorCode.Duplicate);
        }
        try
        {
            await conn.OpenAsync();
            await conn.InsertAsync(entity);
            return DalResult.Success;
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

    public async Task<DalResult> UpdateAsync(SettingsEntity entity)
    {
        if (entity is null || string.IsNullOrWhiteSpace(entity.Name))
        {
            return new(DalErrorCode.Invalid, new Exception("Entity is null or key is missing"));
        }
        var existing = await ReadAsync(entity.Name);
        if (existing is null)
        {
            return new(DalErrorCode.NotFound);
        }
        using var conn = new SqlConnection(_database.ConnectionString);
        try
        {
            await conn.OpenAsync();
            await conn.UpdateAsync(entity);
            return DalResult.Success;
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

    public async Task<DalResult> DeleteAsync(SettingsEntity entity)
    {
        if (entity is null || string.IsNullOrWhiteSpace(entity.Name))
        {
            return new(DalErrorCode.Invalid, new Exception("Entity is null or key is missing"));
        }
        var sql = "Delete from Settings where [Name]=@name;";
        using var conn = new SqlConnection(_database.ConnectionString);
        try
        {
            await conn.OpenAsync();
            await conn.ExecuteAsync(sql, new { name = entity.Name });
            return DalResult.Success;
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

    protected static DynamicParameters BuildParameters(params QueryParameter[] parameters)
    {
        DynamicParameters ret = new();
        parameters.ForEach(x => ret.Add(x.Name, x.Value, x.Type, ParameterDirection.Input));
        return ret;
    }

    public async Task<IEnumerable<SettingsEntity>> GetAsync()
    {
        var sql = "select * from Settings order by [Name];";
        return await GetAsync(sql);
    }

    public async Task<IEnumerable<SettingsEntity>> GetAsync(string sql, params QueryParameter[] parameters)
    {
        var parm = BuildParameters(parameters);
        using var conn = new SqlConnection(_database.ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryAsync<SettingsEntity>(sql, parm);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<SettingsEntity?> ReadAsync(string name) =>
        await ReadAsync("select * from Settings where [Name]=@name;", new QueryParameter("name", name, DbType.String));

    public async Task<SettingsEntity?> ReadAsync(string sql, params QueryParameter[] parameters)
    {
        var parm = BuildParameters(parameters);
        using var conn = new SqlConnection(_database.ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<SettingsEntity>(sql, parm);
            return ret!;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }
}
