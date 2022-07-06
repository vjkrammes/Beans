using System.ComponentModel;

namespace Beans.Common.Enumerations;

public enum CostBasisType
{
    [Description("Unspecified")]
    Unspecified = 0,
    [Description("No Holdings")]
    NoHoldings = 1,
    [Description("Average")]
    Average = 2,
    [Description("Basis")]
    Basis = 3
}
