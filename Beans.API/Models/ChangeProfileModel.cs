namespace Beans.API.Models;

public class ChangeProfileModel
{
    public string Identifier { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }

    public ChangeProfileModel()
    {
        Identifier = string.Empty;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        DisplayName = string.Empty;
    }
}
