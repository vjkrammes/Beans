namespace Beans.API.Models;

public class SellBeanItem
{
    public string HoldingId { get; set; }
    public long Quantity { get; set; }

    public SellBeanItem()
    {
        HoldingId = string.Empty;
        Quantity = 0;
    }
}
