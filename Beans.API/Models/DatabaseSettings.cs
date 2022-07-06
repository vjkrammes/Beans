namespace Beans.API.Models;

public class DatabaseSettings
{
    public string Server { get; set; }
    public string Name { get; set; }
    public string Auth { get; set; }

    public DatabaseSettings()
    {
        Server = "localhost";
        Name = "Beans";
        Auth = "Trusted_Connection = true";
    }
}
