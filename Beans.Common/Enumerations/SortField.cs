using System.ComponentModel;

namespace Beans.Common.Enumerations;

[Flags]
public enum SortField
{
    [Description("Date ascending")]
    DateAscending = 1,
    [Description("Date descending")]
    DateDescending = 2,
    [Description("Seller ascending")]
    SellerAscending = 4,
    [Description("Seller descending")]
    SellerDescending = 8,
    [Description("Bean ascending")]
    BeanAscending = 16,
    [Description("Bean descending")]
    BeanDescending = 32,
    [Description("Quantity ascending")]
    QuantityAscending = 64,
    [Description("Quantity descending")]
    QuantityDescending = 128,
    [Description("Expiration ascending")]
    ExpirationAscending = 256,
    [Description("Expiration descending")]
    ExpirationDescending = 512,
    [Description("Buy / Sell ascending")]
    BuySellAscending = 1024,
    [Description("Buy / Sell descending")]
    BuySellDescending = 2048,
    [Description("Price ascending")]
    PriceAscending = 4096,
    [Description("Price descending")]
    PriceDescending = 8192,
    [Description("Date")]
    Date = DateAscending | DateDescending,
    [Description("Seller")]
    Seller = SellerAscending | SellerDescending,
    [Description("Bean")]
    Bean = BeanAscending | BeanDescending,
    [Description("Quantity")]
    Quantity = QuantityAscending | QuantityDescending,
    [Description("Expiration")]
    Expiration = ExpirationAscending | ExpirationDescending,
    [Description("Buy / Sell")]
    BuySell = BuySellAscending | BuySellDescending,
    [Description("Price")]
    Price = PriceAscending | PriceDescending
}
