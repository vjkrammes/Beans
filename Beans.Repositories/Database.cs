using Beans.Common;
using Beans.Repositories.Enumerations;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using System.Data.SqlClient;

namespace Beans.Repositories;
public class Database : IDatabase
{
    private readonly string _baseConnectionString;
    private readonly string _databaseName;

    private string _connectionString => $"{_baseConnectionString};Database={_databaseName}";
    public string ConnectionString => _connectionString;

    public Database(string server, string database, string auth = "Trusted_Connection=true")
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            throw new ArgumentException("Server name is required", nameof(server));
        }
        if (string.IsNullOrWhiteSpace(database))
        {
            throw new ArgumentException("Database name is required", nameof(database));
        }
        _baseConnectionString = $"Server={server};Timeout=10;Pooling=true;{auth};MultipleActiveResultSets=true;";
        _databaseName = database;
    }

    public bool DatabaseExists()
    {
        using var conn = new SqlConnection(_baseConnectionString);
        conn.Open();
        var sql = "select database_id from sys.databases where name=@name;";
        using var command = new SqlCommand(sql, conn);
        command.Parameters.Add(new("name", _databaseName));
        try
        {
            var result = command.ExecuteScalar();
            if (result is null)
            {
                return false;
            }
            if (result is int dbid)
            {
                return dbid > -1;
            }
            return false;
        }
        catch
        {
            return false;
        }
        finally
        {
            conn.Close();
        }
    }

    public bool TableExists(string tableName)
    {
        var ret = false;
        if (!DatabaseExists() || string.IsNullOrWhiteSpace(tableName))
        {
            return false;
        }
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        var sql = "select * from INFORMATION_SCHEMA.TABLES where TABLE_TYPE='BASE TABLE' and TABLE_NAME=@tbn;";
        using var command = new SqlCommand(sql, conn);
        command.Parameters.Add(new("tbn", tableName));
        var reader = command.ExecuteReader();
        if (reader.Read())
        {
            ret = true;
        }
        conn.Close();
        return ret;
    }

    public bool ColumnExists(string tableName, string columnName)
    {
        var ret = false;
        if (!DatabaseExists() || !TableExists(tableName) || string.IsNullOrWhiteSpace(columnName))
        {
            return false;
        }
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        var sql = "select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_CATALOG=@dbn and TABLE_NAME=@tbn and COLUMN_NAME=@col;";
        using var command = new SqlCommand(sql, conn);
        command.Parameters.Add(new("dbn", _databaseName));
        command.Parameters.Add(new("tbn", tableName));
        command.Parameters.Add(new("col", columnName));
        var reader = command.ExecuteReader();
        if (reader.Read())
        {
            ret = true;
        }
        conn.Close();
        return ret;
    }

    public bool IndexExists(string tableName, string indexName)
    {
        var ret = false;
        if (!DatabaseExists() || !TableExists(tableName) || string.IsNullOrWhiteSpace(indexName))
        {
            return false;
        }
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        var sql = "select * from sys.indexes where name=@ixn and object_id=OBJECT_ID(@tbn);";
        using var command = new SqlCommand(sql, conn);
        command.Parameters.Add(new("tbn", tableName));
        command.Parameters.Add(new("ixn", indexName));
        var reader = command.ExecuteReader();
        if (reader.Read())
        {
            ret = true;
        }
        conn.Close();
        return ret;
    }

    public IEnumerable<string> Tables()
    {
        List<string> ret = new();
        if (!DatabaseExists())
        {
            return ret;
        }
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        var sql = "select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_TYPE='BASE TABLE' and TABLE_CATALOG=@dbn order by TABLE_NAME;";
        using var command = new SqlCommand(sql, conn);
        command.Parameters.Add(new("dbn", _databaseName));
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            ret.Add((string)reader["TABLE_NAME"]);
        }
        conn.Close();
        return ret;
    }

    public IEnumerable<string> Columns(string tableName)
    {
        List<string> ret = new();
        if (!DatabaseExists() || TableExists(tableName))
        {
            return ret;
        }
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        var sql = "select name from sys.columns where [object_id] = object_id(@tbn) order by name;";
        using var command = new SqlCommand(sql, conn);
        command.Parameters.Add(new("tbn", tableName));
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            ret.Add((string)reader["name"]);
        }
        conn.Close();
        return ret;
    }

    public void CreateDatabase(string? location = null)
    {
        if (DatabaseExists())
        {
            return;
        }
        var sql = $"create database [{_databaseName}]";
        var connectionString = $"{_baseConnectionString};Database=master";
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        if (!string.IsNullOrWhiteSpace(location))
        {
            sql += $" on (name='{_databaseName}.mdb', filename='{location}')";
        }
        sql += ";";
        using var command = new SqlCommand(sql, conn);
        try
        {
            command.ExecuteNonQuery();
        }
        catch
        {
            throw;
        }
        finally
        {
            conn.Close();
        }
    }

    public void DropDatabase()
    {
        if (!DatabaseExists())
        {
            return;
        }
        var sql = $"drop database [{_databaseName}];";
        var connectionString = $"{_baseConnectionString};Database=master;";
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        using var command = new SqlCommand(sql, conn);
        try
        {
            command.ExecuteNonQuery();
        }
        catch
        {
            throw;
        }
        finally
        {
            conn.Close();
        }
    }

    public void BackupDatabase(string location)
    {
        if (!DatabaseExists() || string.IsNullOrWhiteSpace(location))
        {
            return;
        }
        var sql = $"backup database [{_databaseName}] to disk = @loc with init;";
        using var conn = new SqlConnection(_baseConnectionString);
        conn.Open();
        using var command = new SqlCommand(sql, conn);
        command.Parameters.Add(new("loc", location));
        try
        {
            command.ExecuteNonQuery();
        }
        catch
        {
            throw;
        }
        finally
        {
            conn.Close();
        }
    }

    public void RestoreDatabase(string location, bool overwrite = false)
    {
        if (!overwrite && DatabaseExists())
        {
            return;
        }
        var sql = $"restore database [{_databaseName}] from disk = @loc;";
        using var conn = new SqlConnection(_baseConnectionString);
        conn.Open();
        using var command = new SqlCommand(sql, conn);
        try
        {
            if (DatabaseExists())
            {
                DropDatabase();
            }
            command.ExecuteNonQuery();
        }
        catch
        {
            throw;
        }
        finally
        {
            conn.Close();
        }
    }

    public void TruncateTableAndResetIdentity(string tableName)
    {
        var sql = $"truncate table [{tableName}];";
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var command = new SqlCommand(sql, conn);
        try
        {
            command.ExecuteNonQuery();
        }
        catch
        {
            throw;
        }
        finally
        {
            conn.Close();
        }
    }

    public void CreateIndices(string tableName, List<IndexDefinition> indices)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return;
        }
        if (indices is null || !indices.Any())
        {
            return;
        }
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        try
        {
            foreach (var index in indices)
            {
                if (string.IsNullOrWhiteSpace(index.IndexName) || IndexExists(tableName, index.IndexName))
                {
                    continue;
                }
                var sql = $"create nonclustered index {index.IndexName} on {tableName}({index.ColumnName});";
                using var command = new SqlCommand(sql, conn);
                command.ExecuteNonQuery();
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            conn.Close();
        }
    }

    public CreateTableResult CreateTable(string tableName, string sql, List<IndexDefinition>? indices)
    {
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(sql))
        {
            return CreateTableResult.Missing;
        }
        if (!DatabaseExists())
        {
            return CreateTableResult.NoDatabase;
        }
        if (TableExists(tableName))
        {
            return CreateTableResult.TableExists;
        }
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        using var command = new SqlCommand(sql, conn);
        command.ExecuteNonQuery();
        if (indices is not null && indices.Any())
        {
            foreach (var index in indices)
            {
                var indexsql = $"create index {index.IndexName} on {tableName}({index.ColumnName});";
                using var indexcommand = new SqlCommand(indexsql, conn);
                try
                {
                    indexcommand.ExecuteNonQuery();
                }
                catch
                {
                    return CreateTableResult.IndexFailed;
                }
                finally
                {
                    conn.Close();
                }
            }
            conn.Close();
        }
        else
        {
            conn.Close();
        }
        return CreateTableResult.Success;
    }

    public Dictionary<string, CreateTableResult> CreateTables(Dictionary<string, string> tableDefinitions, Dictionary<string, List<IndexDefinition>>? indices = null)
    {
        if (tableDefinitions is null || !tableDefinitions.Any())
        {
            return new() { [""] = CreateTableResult.Missing };
        }
        Dictionary<string, CreateTableResult> ret = new();
        tableDefinitions.ForEach(x => ret.Add(x.Key, CreateTableResult.NotStarted));
        tableDefinitions.ForEach(x =>
        {
            List<IndexDefinition> ix = new();
            ix = indices is not null && indices.ContainsKey(x.Key) ? indices[x.Key] : null!;
            try
            {
                ret[x.Key] = CreateTable(x.Key, x.Value, ix);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table {x.Key}: {ex.Innermost()}");
                Console.WriteLine($"SQL = {x.Value}");
                ret[x.Key] = CreateTableResult.Exception;
            }
        });
        return ret;
    }

    public void DropTable(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName) || !DatabaseExists() || !TableExists(tableName))
        {
            return;
        }
        using var conn = new SqlConnection(ConnectionString);
        conn.Open();
        var sql = $"drop table [{tableName}];";
        using var command = new SqlCommand(sql, conn);
        try
        {
            command.ExecuteNonQuery();
        }
        catch
        {
            throw;
        }
        finally
        {
            conn.Close();
        }
    }

    public Dictionary<string, CreateTableResult> Instantiate(Dictionary<string, string> tableDefinitions, bool dropFirst = false,
      Dictionary<string, List<IndexDefinition>>? indices = null)
    {
        if (!dropFirst && DatabaseExists())
        {
            return new() { [""] = CreateTableResult.DatabaseExists };
        }
        if (dropFirst)
        {
            DropDatabase();
        }
        CreateDatabase();
        return CreateTables(tableDefinitions, indices);
    }
}
