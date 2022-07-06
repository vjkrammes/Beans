namespace Beans.API.Models;

public class PluginDescription
{
    public char Tag { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public PluginDescription()
    {
        Tag = ' ';
        Name = string.Empty;
        Description = string.Empty;
    }
}
