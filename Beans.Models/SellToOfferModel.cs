using Beans.Common;

namespace Beans.Models;
public class SellToOfferModel
{
    public string OfferId { get; set; }
    public string SellerId { get; set; }
    public SellToOfferItem[] Items { get; set; }

    public SellToOfferModel()
    {
        SellerId = IdEncoder.EncodeId(0);
        OfferId = IdEncoder.EncodeId(0);
        Items = Array.Empty<SellToOfferItem>();
    }
}
