namespace Beans.API.Models;

public class BuyBeanItem
{
    public string Id { get; set; }
    public long Quantity { get; set; }

    public BuyBeanItem()
    {
        Id = string.Empty;
        Quantity = 0;
    }
}
