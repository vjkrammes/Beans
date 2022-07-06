namespace Beans.Repositories.Models;
public class HoldingSeedModel
{
    public string UserEmail { get; set; }
    public string BeanName { get; set; }
    public string PurchaseDate { get; set; }
    public long Quantity { get; set; }
    public decimal Price { get; set; }

    public HoldingSeedModel()
    {
        UserEmail = string.Empty;
        BeanName = string.Empty;
        PurchaseDate = string.Empty;
        Quantity = 0;
        Price = 0M;
    }
}
