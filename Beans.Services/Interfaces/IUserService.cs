using Beans.Common;
using Beans.Models;

namespace Beans.Services.Interfaces;
public interface IUserService : IDataService<UserModel>
{
    Task<UserModel?> ReadForEmailAsync(string email);
    Task<UserModel?> ReadForIdentifierAsync(string identifier);
    Task<DateTime> JoinedAsync(string userid);
    Task<ApiError> AddRegisteredUserAsync(UserModel model);
    Task<bool> UserExistsByIdAsync(string userid);
    Task<bool> UserExistsByEmailAsync(string email);
    Task<bool> UserExistsByIdentifierAsync(string identifier);
    Task<IEnumerable<NameModel>> GetNamesAsync();
    Task<string> GetNameAsync(string userid);
    Task<ApiError> ChangeNameAsync(string userid, string firstName, string lastName);
    Task<ApiError> LoanAsync(string userid, decimal amount);
    Task<ApiError> RepayAsync(string userid, decimal amount);
    Task<ApiError> ResetUsersAsync();
    Task<ApiError> ToggleAdminAsync(string userid);
    Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync();
}
