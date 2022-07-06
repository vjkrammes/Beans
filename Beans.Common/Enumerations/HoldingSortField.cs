using System.ComponentModel;

namespace Beans.Common.Enumerations;

[Flags]
public enum HoldingSortField
{
    [Description("Purchase date ascending")]
    PurchaseDateAscending = 1,
    [Description("Purchase date descending")]
    PurchaseDateDescending = 2,
    [Description("Quantity ascending")]
    QuantityAscending = 4,
    [Description("Quantity descending")]
    QuantityDescending = 8,
    [Description("Price ascending")]
    PriceAscending = 16,
    [Description("Price descending")]
    PriceDescending = 32,
    [Description("Purchase date")]
    PurchaseDate = PurchaseDateAscending | PurchaseDateDescending,
    [Description("Quantity")]
    Quantity = QuantityAscending | QuantityDescending,
    [Description("Price")]
    Price = PriceAscending | PriceDescending
}
