
using Beans.Common;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;

using Microsoft.Extensions.Configuration;

namespace Beans.Repositories;

public class BeanSeeder : SeederBase<BeanEntity, IBeanRepository>, IBeanSeeder
{
    public BeanSeeder(IBeanRepository repository) : base(repository) { }

    public override async Task SeedAsync(IConfiguration configuration, string sectionName)
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
        var items = section.Get<BeanEntity[]>();
        if (items is null || !items.Any())
        {
            return;
        }
        foreach (var item in items)
        {
            var existing = await _repository.ReadAsync(item.Name);
            if (existing is not null)
            {
                continue;
            }
            var result = await _repository.InsertAsync(item);
            if (!result.Successful)
            {
                Console.WriteLine($"Insert of bean '{item.Name}' failed: {result.ErrorMessage}");
                Console.WriteLine(Tools.DumpObject(item));
            }
        }
    }
}
