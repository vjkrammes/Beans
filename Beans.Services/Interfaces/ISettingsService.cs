using Beans.Common;
using Beans.Models;

namespace Beans.Services.Interfaces;

public interface ISettingsService
{
    Task<int> CountAsync();
    Task<ApiError> InsertAsync(SettingsModel model);
    Task<ApiError> UpdateAsync(SettingsModel model);
    Task<ApiError> DeleteAsync(SettingsModel model);
    Task<IEnumerable<SettingsModel>> GetAsync();
    Task<SettingsModel?> ReadAsync(string name);
    Task<(bool valid, int value)> ReadIntSetting(string name);
    Task<(bool valid, long value)> ReadLongSetting(string name);
    Task<(bool valid, decimal value)> ReadDecimalSetting(string name);
    Task<(bool valid, double value)> ReadDoubleSetting(string name);
    Task<(bool valid, bool value)> ReadBoolSetting(string name);
    Task<(bool valid, string value)> ReadStringSetting(string name);
    Task<(bool valid, DateTime value)> ReadDateSetting(string name);
    Task<(bool valid, Guid value)> ReadGuidSetting(string name);
    Task<(bool valid, string value)> ReadJsonSettingAsString(string name);
    Task<(bool valid, object? value)> ReadJsonSettingAsObject(string name);
    Task<(bool valid, dynamic? value)> ReadJsonSettingAsDynamic(string name);
}
