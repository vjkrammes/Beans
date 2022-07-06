using Beans.Common.Enumerations;

namespace Beans.Models;
public class CostBasisModel
{
    public string BeanId { get; set; }
    public string BeanName { get; set; }
    public string Filename { get; set; }
    public CostBasisType BasisType { get; set; }
    public decimal Basis { get; set; }
    public decimal GainOrLoss { get; set; }
    public decimal Percent { get; set; }

    public CostBasisModel()
    {
        BeanId = string.Empty;
        BeanName = string.Empty;
        Filename = string.Empty;
        BasisType = CostBasisType.Unspecified;
        Basis = 0M;
        GainOrLoss = 0M;
        Percent = 0M;
    }
}
