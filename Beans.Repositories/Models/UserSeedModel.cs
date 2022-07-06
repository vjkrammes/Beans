namespace Beans.Repositories.Models;
public class UserSeedModel
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public decimal Balance { get; set; }
    public bool IsAdmin { get; set; }

    public UserSeedModel()
    {
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        DisplayName = string.Empty;
        Balance = 0M;
        IsAdmin = false;
    }
}
