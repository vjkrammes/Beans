namespace Beans.API.Models;

public class SellBeanModel
{
    public string Userid { get; set; }
    public SellBeanItem[] Holdings { get; set; }

    public SellBeanModel()
    {
        Userid = string.Empty;
        Holdings = Array.Empty<SellBeanItem>();
    }
}
