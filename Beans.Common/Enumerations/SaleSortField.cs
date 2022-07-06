using System.ComponentModel;

namespace Beans.Common.Enumerations;

[Flags]
public enum SaleSortField
{
    [Description("Bean ascending")]
    BeanAscending = 1,
    [Description("Bean descending")]
    BeanDescending = 2,
    [Description("Purchase date ascending")]
    PurchaseDateAscending = 4,
    [Description("Purchase date descending")]
    PurchaseDateDescending = 8,
    [Description("Sale date ascending")]
    SaleDateAscending = 16,
    [Description("Sale date descending")]
    SaleDateDescending = 32,
    [Description("Quantity ascending")]
    QuantityAscending = 64,
    [Description("Quantity descending")]
    QuantityDescending = 128,
    [Description("Basis ascending")]
    BasisAscending = 256,
    [Description("Basis descending")]
    BasisDescending = 512,
    [Description("Price ascending")]
    PriceAscending = 1024,
    [Description("Price descending")]
    PriceDescending = 2048,
    [Description("Gain ascending")]
    GainAscending = 4096,
    [Description("Gain descending")]
    GainDescending = 8192,
    [Description("Bean")]
    Bean = BeanAscending | BeanDescending,
    [Description("Purchase date")]
    PurchaseDate = PurchaseDateAscending | PurchaseDateDescending,
    [Description("Sale date")]
    SaleDate = SaleDateAscending | SaleDateDescending,
    [Description("Quantity")]
    Quantity = QuantityAscending | QuantityDescending,
    [Description("Basis")]
    Basis = BasisAscending | BasisDescending,
    [Description("Price")]
    Price = PriceAscending | PriceDescending,
    [Description("Gain")]
    Gain = GainAscending | GainDescending
}
