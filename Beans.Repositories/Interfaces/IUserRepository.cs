
using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Repositories.Interfaces;
public interface IUserRepository : IRepository<UserEntity>
{
    Task<UserEntity?> ReadAsync(string email);
    Task<UserEntity?> ReadByIdentifierAsync(string identifier);
    Task<DateTime> JoinedAsync(int userid);
    Task<DalResult> LoanAsync(int userid, decimal amount);
    Task<DalResult> RepayAsync(int userid, decimal amount);
    Task<DalResult> ResetUsersAsync();
    Task<DalResult> ToggleAdminAsync(int userid);
}
