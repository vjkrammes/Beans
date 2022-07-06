using System.ComponentModel;

namespace Beans.Repositories.Enumerations;

public enum CreateTableResult
{
    [Description("Success")]
    Success = 0,
    [Description("Missing table name or missing sql statement")]
    Missing = 1,
    [Description("Table Exists")]
    TableExists = 2,
    [Description("Database does not exist")]
    NoDatabase = 3,
    [Description("Index creation failed")]
    IndexFailed = 4,
    [Description("Creation was not started due to other errors")]
    NotStarted = 5,
    [Description("An exception occurred")]
    Exception = 6,
    [Description("Database already exists")]
    DatabaseExists = 7
}
