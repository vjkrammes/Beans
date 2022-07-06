
using Beans.Common;
using Beans.Repositories.Entities;
using Beans.Repositories.Models;

namespace Beans.Repositories.Interfaces;
public interface ISettingsRepository
{
    Task<int> CountAsync();
    Task<DalResult> InsertAsync(SettingsEntity entity);
    Task<DalResult> UpdateAsync(SettingsEntity entity);
    Task<DalResult> DeleteAsync(SettingsEntity entity);
    Task<IEnumerable<SettingsEntity>> GetAsync();
    Task<IEnumerable<SettingsEntity>> GetAsync(string sql, params QueryParameter[] parameters);
    Task<SettingsEntity?> ReadAsync(string name);
    Task<SettingsEntity?> ReadAsync(string sql, params QueryParameter[] parameters);
}
