
using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Common.Interfaces;
using Beans.Repositories.Interfaces;

using Microsoft.Extensions.Configuration;

namespace Beans.Repositories;
public abstract class SeederBase<TEntity, TRepository> : ISeeder<TEntity>
  where TEntity : class, IIdEntity, ISqlEntity, new()
  where TRepository : class, IRepository<TEntity>
{
    protected readonly TRepository _repository;

    public SeederBase(TRepository repository) => _repository = repository;

    public virtual async Task SeedAsync(IConfiguration configuration, string sectionName)
    {
        if (configuration is null || string.IsNullOrWhiteSpace(sectionName))
        {
            return;
        }
        var section = configuration.GetSection(sectionName);
        if (section is null)
        {
            return;
        }
        var items = section.Get<TEntity[]>();
        if (items is null || !items.Any())
        {
            return;
        }
        foreach (var item in items)
        {
            var result = await _repository.InsertAsync(item);
            if (!result.Successful)
            {
                if (result.ErrorCode != DalErrorCode.Duplicate)
                {
                    Console.WriteLine($"Insert of item from section '{sectionName}' failed: {result.ErrorMessage}");
                    Console.WriteLine(Tools.DumpObject(item));
                }
            }
        }
    }
}
