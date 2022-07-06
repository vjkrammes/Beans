namespace Beans.API.Models;

public class UserHoldingModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Filename { get; set; }
    public long Held { get; set; }
    public long Quantity { get; set; }

    public UserHoldingModel()
    {
        Id = string.Empty;
        Name = string.Empty;
        Filename = string.Empty;
        Held = 0;
        Quantity = 0;
    }
}
