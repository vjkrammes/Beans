using Beans.Common.Enumerations;

using System.Text;

namespace Beans.Common;

public class CostBasis
{
    public CostBasisType Type { get; set; }
    public decimal Basis { get; set; }

    public static CostBasis Default => new()
    {
        Type = CostBasisType.NoHoldings,
        Basis = 0M
    };

    public override string ToString()
    {
        var sb = new StringBuilder(Basis.ToCurrency(2));
        if (Basis != 0M)
        {
            sb.Append(' ');
            sb.Append(Type.GetDescriptionFromEnumValue());
        }
        return sb.ToString();
    }
}
