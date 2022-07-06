namespace Beans.Models;
public class SellToOfferItem
{
    public string HoldingId { get; set; }
    public long Quantity { get; set; }

    public SellToOfferItem()
    {
        HoldingId = string.Empty;
        Quantity = 0;
    }
}
