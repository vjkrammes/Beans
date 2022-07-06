using System.Data;

namespace Beans.Repositories.Models;
public class QueryParameter
{
    public string Name { get; set; }
    public DbType Type { get; set; }
    public object? Value { get; set; }

    public QueryParameter()
    {
        Name = string.Empty;
        Type = DbType.String;
        Value = null;
    }

    public QueryParameter(string name, object? value, DbType type)
    {
        Name = name;
        Type = type;
        Value = value;
    }
}
