using Beans.Common;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

using Newtonsoft.Json;

namespace Beans.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;

    public SettingsService(ISettingsRepository settingsRepository) => _settingsRepository = settingsRepository;

    public async Task<int> CountAsync() => await _settingsRepository.CountAsync();

    public async Task<ApiError> ValidateModel(SettingsModel model, bool update = false)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
        {
            return new(Strings.InvalidModel);
        }
        var existing = await _settingsRepository.ReadAsync(model.Name);
        if (existing is not null && !update)
        {
            return new(string.Format(Strings.Duplicate, "a", "setting", "key", model.Name));
        }
        if (model.Value is null)
        {
            model.Value = string.Empty;
        }
        return ApiError.Success;
    }

    public async Task<ApiError> InsertAsync(SettingsModel model)
    {
        var checkresult = await ValidateModel(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        SettingsEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _settingsRepository.InsertAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> UpdateAsync(SettingsModel model)
    {
        var checkresult = await ValidateModel(model, true);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        SettingsEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _settingsRepository.UpdateAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAsync(SettingsModel model)
    {
        if (model is null)
        {
            return new(Strings.InvalidModel);
        }
        SettingsEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _settingsRepository.DeleteAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<IEnumerable<SettingsModel>> GetAsync()
    {
        var entities = await _settingsRepository.GetAsync();
        var models = entities.ToModels<SettingsModel, SettingsEntity>();
        models.ForEach(x => x.CanDelete = true);
        return models;
    }

    public async Task<SettingsModel?> ReadAsync(string name)
    {
        SettingsModel model = (await _settingsRepository.ReadAsync(name))!;
        if (model is not null)
        {
            model.CanDelete = true;
        }
        return model;
    }

    delegate bool ParseDelegate<T>(string source, out T value);

    private async Task<(bool valid, T value)> ParseSetting<T>(string name, ParseDelegate<T> parser)
    {
        if (string.IsNullOrWhiteSpace(name) || parser is null)
        {
            return (false, default)!;
        }
        var model = await ReadAsync(name);
        if (model is null)
        {
            return (false, default)!;
        }
        var ret = parser(model.Value, out var val);
        return (ret, val);
    }

    public async Task<(bool valid, int value)> ReadIntSetting(string name) => await ParseSetting<int>(name, int.TryParse);

    public async Task<(bool valid, long value)> ReadLongSetting(string name) => await ParseSetting<long>(name, long.TryParse);

    public async Task<(bool valid, decimal value)> ReadDecimalSetting(string name) => await ParseSetting<decimal>(name, decimal.TryParse);

    public async Task<(bool valid, double value)> ReadDoubleSetting(string name) => await ParseSetting<double>(name, double.TryParse);

    public async Task<(bool valid, bool value)> ReadBoolSetting(string name) => await ParseSetting<bool>(name, bool.TryParse);

    public async Task<(bool valid, string value)> ReadStringSetting(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, string.Empty);
        }
        var model = await ReadAsync(name);
        if (model is null)
        {
            return (false, string.Empty);
        }
        return (true, model.Value);
    }

    public async Task<(bool valid, DateTime value)> ReadDateSetting(string name) => await ParseSetting<DateTime>(name, DateTime.TryParse);

    public async Task<(bool valid, Guid value)> ReadGuidSetting(string name) => await ParseSetting<Guid>(name, Guid.TryParse);

    public async Task<(bool valid, string value)> ReadJsonSettingAsString(string name) => await ReadStringSetting(name);

    public async Task<(bool valid, object? value)> ReadJsonSettingAsObject(string name)
    {
        var (valid, value) = await ReadStringSetting(name);
        if (!valid)
        {
            return (false, null)!;
        }
        var obj = JsonConvert.DeserializeObject<object>(value);
        return obj is null ? (false, null)! : (true, obj)!;
    }

    public async Task<(bool valid, dynamic? value)> ReadJsonSettingAsDynamic(string name)
    {
        var (valid, value) = await ReadStringSetting(name);
        if (!valid)
        {
            return (false, null)!;
        }
        var dobj = JsonConvert.DeserializeObject<dynamic>(value);
        return dobj is null ? (false, null)! : (true, dobj)!;
    }
}
