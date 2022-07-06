using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

namespace Beans.Services;
public class LogService : ILogService
{
    private readonly ILogRepository _logRepository;

    public LogService(ILogRepository logRepository) => _logRepository = logRepository;

    public async Task<int> CountAsync() => await _logRepository.CountAsync();

    public static ApiError ValidateModel(LogModel model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Source) || string.IsNullOrWhiteSpace(model.Description) || string.IsNullOrWhiteSpace(model.Data))
        {
            return new(Strings.InvalidModel);
        }
        if (!Enum.IsDefined(typeof(Level), model.LogLevel))
        {
            return new(string.Format(Strings.Invalid, "log level"));
        }
        if (model.Timestamp == default)
        {
            model.Timestamp = DateTime.UtcNow;
        }
        return ApiError.Success;
    }

    public async Task<ApiError> InsertAsync(LogModel model)
    {
        var checkresult = ValidateModel(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        LogEntity entity = model!;
        try
        {
            var response = await _logRepository.InsertAsync(entity);
            if (response.Successful)
            {
                model.Id = IdEncoder.EncodeId(entity.Id);
            }
            return ApiError.FromDalResult(response);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public Task<ApiError> UpdateAsync(LogModel model) => throw new NotImplementedException("Log records cannot be updated");

    public Task<ApiError> DeleteAsync(LogModel model) => throw new NotImplementedException("Log records cannot be deleted");

    private static IEnumerable<LogModel> Finish(IEnumerable<LogEntity> entities)
    {
        var models = entities.ToModels<LogModel, LogEntity>();
        models.ForEach(x => x.CanDelete = false);
        return models;
    }

    public async Task<IEnumerable<LogModel>> GetAsync()
    {
        var entities = await _logRepository.GetAsync();
        return Finish(entities);
    }

    public async Task<IEnumerable<LogModel>> GetForDateAsync(DateTime date)
    {
        var entities = await _logRepository.GetForDateAsync(date);
        return Finish(entities);
    }

    public async Task<IEnumerable<LogModel>> GetForDateRangeAsync(DateTime start, DateTime end)
    {
        var entities = await _logRepository.GetForDateRangeAsync(start, end);
        return Finish(entities);
    }

    public async Task<LogModel?> ReadAsync(string id)
    {
        var pid = IdEncoder.DecodeId(id);
        var entity = await _logRepository.ReadAsync(pid);
        return entity!;
    }
}
