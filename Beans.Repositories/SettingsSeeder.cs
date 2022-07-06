using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;

using Microsoft.Extensions.Configuration;

using System.Globalization;

namespace Beans.Repositories;
public class SettingsSeeder : ISettingsSeeder
{
    private readonly ISettingsRepository _repository;

    public SettingsSeeder(ISettingsRepository repository) => _repository = repository;

    public async Task SeedAsync(IConfiguration configuration, string sectionName)
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
        var items = section.Get<SettingsEntity[]>();
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
            var keyword = item.Value.ToLower(CultureInfo.CurrentCulture);
            switch (keyword)
            {
                case "newguid":
                    item.Value = Guid.NewGuid().ToString();
                    break;
                case "datetime":
                    item.Value = DateTime.Now.ToString();
                    break;
                case "utctime":
                    item.Value = DateTime.UtcNow.ToString();
                    break;
            }
            var result = await _repository.InsertAsync(item);
            if (!result.Successful)
            {
                Console.WriteLine($"Insert of settings item with name '{item.Name}' failed: {result.ErrorMessage}");
            }
        }
    }
}
