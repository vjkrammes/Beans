
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;

using Microsoft.Extensions.Configuration;

namespace Beans.Repositories;
public class LogSeeder : SeederBase<LogEntity, ILogRepository>, ILogSeeder
{
    public LogSeeder(ILogRepository repository) : base(repository) { }

    public override Task SeedAsync(IConfiguration configuration, string sectionName) => Task.CompletedTask;
}
