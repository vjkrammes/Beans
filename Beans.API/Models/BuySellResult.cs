namespace Beans.API.Models;

public class BuySellResult
{
    public string Color { get; set; }
    public string Result { get; set; }

    public BuySellResult()
    {
        Color = string.Empty;
        Result = string.Empty;
    }
}
