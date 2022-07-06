using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

namespace Beans.Services;
public class MovementService : IMovementService
{
    private readonly IMovementRepository _movementRepository;
    private readonly IBeanRepository _beanRepository;

    public MovementService(IMovementRepository movementRepository, IBeanRepository beanRepository)
    {
        _movementRepository = movementRepository;
        _beanRepository = beanRepository;
    }

    public async Task<int> CountAsync() => await _movementRepository.CountAsync();

    private async Task<ApiError> ValidateModelAsync(MovementModel model, bool checkid = false)
    {
        if (model is null || IdEncoder.DecodeId(model.BeanId) <= 0 || model.Open <= 0M || model.Close <= 0M || model.Movement <= 0M ||
          !Enum.IsDefined(typeof(MovementType), model.MovementType))
        {
            return new(Strings.InvalidModel);
        }
        if (model.MovementDate == default)
        {
            model.MovementDate = DateTime.UtcNow;
        }
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = IdEncoder.EncodeId(0);
        }
        if (checkid && IdEncoder.DecodeId(model.Id) == 0)
        {
            return new(string.Format(Strings.Invalid, "id"));
        }
        if (model.Movement != model.Close - model.Open)
        {
            return new(string.Format(Strings.Invalid, "movement value"));
        }
        var bean = await _beanRepository.ReadAsync(model.BeanId);
        if (bean is null)
        {
            return new(string.Format(Strings.NotFound, "bean", "id", model.BeanId));
        }
        return ApiError.Success;
    }

    public async Task<ApiError> InsertAsync(MovementModel model)
    {
        var checkresult = await ValidateModelAsync(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        MovementEntity entity = model!;
        try
        {
            var result = await _movementRepository.InsertAsync(entity);
            if (result.Successful)
            {
                model.Id = IdEncoder.EncodeId(entity.Id);
            }
            return ApiError.FromDalResult(result);
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> InsertAsync(MovementModel movement, BeanModel bean)
    {
        var checkresult = await ValidateModelAsync(movement);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        if (bean is null)
        {
            return new(string.Format(Strings.Invalid, "bean model"));
        }
        MovementEntity movementEntity = movement!;
        BeanEntity beanEntity = bean!;
        try
        {
            return ApiError.FromDalResult(await _movementRepository.InsertAsync(movementEntity, beanEntity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> UpdateAsync(MovementModel model)
    {
        var checkresult = await ValidateModelAsync(model, true);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        MovementEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _movementRepository.UpdateAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAsync(MovementModel model)
    {
        if (model is null)
        {
            return new(Strings.InvalidModel);
        }
        try
        {
            return ApiError.FromDalResult(await _movementRepository.DeleteAsync(IdEncoder.DecodeId(model.Id)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    private static IEnumerable<MovementModel> Finish(IEnumerable<MovementEntity> entities)
    {
        var models = entities.ToModels<MovementModel, MovementEntity>();
        models.ForEach(x => x.CanDelete = true);
        return models;
    }

    public async Task<IEnumerable<MovementModel>> GetAsync()
    {
        var entities = await _movementRepository.GetAsync();
        return Finish(entities);
    }

    public async Task<IEnumerable<MovementModel>> GetForBeanAsync(string beanid)
    {
        var entities = await _movementRepository.GetForBeanAsync(IdEncoder.DecodeId(beanid));
        return Finish(entities);
    }

    public async Task<IEnumerable<MovementModel>> GetForBeanAsync(string beanid, int days)
    {
        var entities = await _movementRepository.GetForBeanAsync(IdEncoder.DecodeId(beanid), days);
        return Finish(entities);
    }

    public async Task<IEnumerable<MovementModel>> TopForBeanAsync(string beanid, int count)
    {
        var entities = await _movementRepository.TopForBeanAsync(IdEncoder.DecodeId(beanid), count);
        return Finish(entities);
    }

    public async Task<IEnumerable<MovementModel>> HistoryAsync(string beanid, DateTime date)
    {
        var entites = await _movementRepository.HistoryAsync(IdEncoder.DecodeId(beanid), date);
        return Finish(entites);
    }

    public async Task<IEnumerable<string>> BeanIdsAsync() => (await _movementRepository.BeanIdsAsync()).Select(x => IdEncoder.EncodeId(x));

    public async Task<IEnumerable<MovementModel>> MostRecentAsync()
    {
        var entities = await _movementRepository.MostRecentAsync();
        return Finish(entities);
    }

    public async Task<IEnumerable<MovementModel>> MostRecentAsync(string[] beanids)
    {
        var entities = await _movementRepository.MostRecentAsync(beanids.Select(x => IdEncoder.DecodeId(x)).ToArray());
        return Finish(entities);
    }

    private static MovementModel? Finish(MovementEntity? entity)
    {
        if (entity is not null)
        {
            MovementModel model = entity!;
            model.CanDelete = true;
            return model;
        }
        return null;
    }

    public async Task<MovementModel?> ReadAsync(string id)
    {
        var entity = await _movementRepository.ReadAsync(IdEncoder.DecodeId(id));
        return Finish(entity);
    }

    public async Task<MovementModel?> MostRecentAsync(string beanid)
    {
        var entity = await _movementRepository.MostRecentAsync(IdEncoder.DecodeId(beanid));
        return Finish(entity);
    }

    public async Task<MovementModel?> ReadForDateAsync(string beanid, DateTime date)
    {
        var entity = await _movementRepository.ReadForDateAsync(IdEncoder.DecodeId(beanid), date);
        return Finish(entity);
    }

    public async Task<bool> BeanHasMovementsAsync(string beanid) => await _movementRepository.BeanHasMovementsAsync(IdEncoder.DecodeId(beanid));

    public async Task<DateTime> LowestDateAsync() => await _movementRepository.LowestDateAsync();

    public async Task<ApiError> CatchupAsync(string beanid, decimal minValue, DateTime startDate) =>
      ApiError.FromDalResult(await _movementRepository.CatchupAsync(IdEncoder.DecodeId(beanid), minValue, startDate));

    public async Task<ApiError> MoveAsync(string beanid, decimal minValue, DateTime date) =>
      ApiError.FromDalResult(await _movementRepository.MoveAsync(IdEncoder.DecodeId(beanid), minValue, date));

    public async Task<decimal> GetMinRangeAsync(string beanid, int days) => await _movementRepository.GetMinRangeAsync(IdEncoder.DecodeId(beanid), days);

    public async Task<decimal> GetAverageRangeAsync(string beanid, int days) => await _movementRepository.GetAverageRangeAsync(IdEncoder.DecodeId(beanid), days);

    public async Task<decimal> GetMaxRangeAsync(string beanid, int days) => await _movementRepository.GetMaxRangeAsync(IdEncoder.DecodeId(beanid), days);

    public async Task<decimal> GetLargestMovementAsync(string beanid) => await _movementRepository.GetLargestMovementAsync(IdEncoder.DecodeId(beanid));

    public async Task<decimal> GetStandardDeviationAsync(string beanid, int days) =>
      await _movementRepository.GetStandardDeviationAsync(IdEncoder.DecodeId(beanid), days);
}
