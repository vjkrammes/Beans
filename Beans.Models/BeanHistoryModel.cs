using Beans.Common;
using Beans.Common.Enumerations;

namespace Beans.Models;
public class BeanHistoryModel
{
    public string BeanId { get; set; }
    public string BeanName { get; set; }
    public string ImageName { get; set; }
    public decimal Price { get; set; }
    public CostBasis Basis { get; set; }
    public long Quantity { get; set; }
    public int Days { get; set; }
    public MovementModel[] Movements { get; set; }

    public PriceHistoryModel[] Prices => Movements.OrderBy(x => x.MovementDate).Select(x => new PriceHistoryModel
    {
        Date = x.MovementDate,
        Price = x.Close
    }).ToArray();

    public BeanHistoryModel()
    {
        BeanId = IdEncoder.EncodeId(0);
        BeanName = string.Empty;
        ImageName = string.Empty;
        Price = 0M;
        Basis = new()
        {
            Basis = 0M,
            Type = CostBasisType.Unspecified
        };
        Quantity = 0;
        Days = 0;
        Movements = Array.Empty<MovementModel>();
    }
}
