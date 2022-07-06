using Beans.Common;
using Beans.Common.Interfaces;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using Dapper;
using Dapper.Contrib.Extensions;

using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Beans.Repositories;
public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class, IIdEntity, ISqlEntity, new()
{
    private readonly string _tableName;
    protected string ConnectionString { get; }

    public RepositoryBase(IDatabase database)
    {
        ConnectionString = database.ConnectionString;
        var type = typeof(TEntity);
        _tableName = (type.GetCustomAttribute(typeof(TableAttribute), false) as TableAttribute)?.Name!;
        if (string.IsNullOrWhiteSpace(_tableName))
        {
            throw new InvalidOperationException($"Table name is not defined on class '{type.Name}'");
        }
        SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
    }

    public virtual async Task<int> CountAsync()
    {
        var sql = $"select count (*) from {_tableName};";
        using var conn = new SqlConnection(ConnectionString);
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

    public virtual async Task<DalResult> InsertAsync(TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.InsertAsync(entity);
            entity.Id = ret;
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

    public virtual async Task<DalResult> UpdateAsync(TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        using var conn = new SqlConnection(ConnectionString);
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

    public virtual async Task<DalResult> DeleteAsync(TEntity entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
        return await DeleteAsync(entity.Id);
    }

    public virtual async Task<DalResult> DeleteAsync(int id)
    {
        var sql = $"delete from {_tableName} where Id={id};";
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            await conn.ExecuteAsync(sql);
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

    public virtual async Task<IEnumerable<TEntity>> GetAsync(string sql, params QueryParameter[] parameters)
    {
        var parm = BuildParameters(parameters);
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryAsync<TEntity>(sql, parm);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAsync()
    {
        var sql = $"select * from {_tableName};";
        return await GetAsync(sql);
    }

    public virtual async Task<TEntity?> ReadAsync(string sql, params QueryParameter[] parameters)
    {
        var parm = BuildParameters(parameters);
        using var conn = new SqlConnection(ConnectionString);
        try
        {
            await conn.OpenAsync();
            var ret = await conn.QueryFirstOrDefaultAsync<TEntity>(sql, parm);
            return ret;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public virtual async Task<TEntity?> ReadAsync(int id)
    {
        var sql = $"select * from {_tableName} where Id={id};";
        return await ReadAsync(sql);
    }
}
