
using Beans.Repositories.Enumerations;
using Beans.Repositories.Models;

namespace Beans.Repositories.Interfaces;
public interface IDatabase
{
    string ConnectionString { get; }
    bool DatabaseExists();
    bool TableExists(string tableName);
    bool ColumnExists(string tableName, string columnName);
    bool IndexExists(string tableName, string indexName);
    IEnumerable<string> Tables();
    IEnumerable<string> Columns(string tableName);
    void CreateDatabase(string? location = null);
    void DropDatabase();
    void BackupDatabase(string location);
    void RestoreDatabase(string location, bool overwrite = false);
    void TruncateTableAndResetIdentity(string tableName);
    void CreateIndices(string tableName, List<IndexDefinition> indices);
    CreateTableResult CreateTable(string tableName, string sql, List<IndexDefinition>? indices = null);
    Dictionary<string, CreateTableResult> CreateTables(Dictionary<string, string> tableDefinitions, Dictionary<string, List<IndexDefinition>>? indices = null);
    void DropTable(string tableName);
    Dictionary<string, CreateTableResult> Instantiate(Dictionary<string, string> tableDefinitions, bool dropFirst = false,
      Dictionary<string, List<IndexDefinition>>? indices = null);
}
