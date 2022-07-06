
using Beans.Repositories.Models;

namespace Beans.Repositories.Interfaces;
public interface IDatabaseBuilder
{
    Task BuildDatabaseAsync(bool dropIfExists);
    (int order, string sql)[] Tables();
    (int order, string name)[] TableNames();
    (int order, List<IndexDefinition> indices)[] Indices();
}
