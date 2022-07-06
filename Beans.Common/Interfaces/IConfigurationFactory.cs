
using Microsoft.Extensions.Configuration;

namespace Beans.Common.Interfaces;
public interface IConfigurationFactory
{
    IConfiguration Create(string filename, bool isOptional = true, string? directory = null);
}
