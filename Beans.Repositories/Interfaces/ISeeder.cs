
using Microsoft.Extensions.Configuration;

namespace Beans.Repositories.Interfaces;
public interface ISeeder<TEntity> where TEntity : class
{
    Task SeedAsync(IConfiguration configuration, string sectionName);
}
