namespace Beans.API.Models;

public class BuyBeanModel
{
    public string Userid { get; set; }
    public BuyBeanItem[] Items { get; set; }

    public BuyBeanModel()
    {
        Userid = string.Empty;
        Items = Array.Empty<BuyBeanItem>();
    }
}
