
using Beans.Common.Interfaces;

using Microsoft.Extensions.Configuration;

namespace Beans.Common;
public class ConfigurationFactory : IConfigurationFactory
{
    public IConfiguration Create(string filename, bool isOptional, string? directory = null)
    {
        var dir = string.IsNullOrWhiteSpace(directory) ? Directory.GetCurrentDirectory() : directory;
        var ret = new ConfigurationBuilder()
          .SetBasePath(dir)
          .AddJsonFile(filename, optional: isOptional, reloadOnChange: true)
          .AddEnvironmentVariables()
          .Build();
        return ret;
    }
}
