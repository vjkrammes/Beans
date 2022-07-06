namespace Beans.API.Models;

public class PasswordPlugin
{
    public string Assembly { get; set; }
    public string[] Plugins { get; set; }

    public PasswordPlugin()
    {
        Assembly = string.Empty;
        Plugins = Array.Empty<string>();
    }
}
