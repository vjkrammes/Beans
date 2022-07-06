using Beans.Common;

namespace Beans.Models;
public class HoldingSummaryModel
{
    public long Quantity { get; set; }
    public CostBasis CostBasis { get; set; }

    public HoldingSummaryModel()
    {
        Quantity = 0;
        CostBasis = CostBasis.Default;
    }
}
