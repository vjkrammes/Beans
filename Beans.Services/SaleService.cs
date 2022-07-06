using Beans.Common;
using Beans.Common.Interfaces;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

using Microsoft.Extensions.Options;

namespace Beans.Services;
public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBeanRepository _beanRepository;
    private readonly TimeSpan _longtermTimespan;

    public SaleService(ISaleRepository saleRepository, IUserRepository userRepository, IBeanRepository beanRepository, IOptions<AppSettings> settings,
      ITimeSpanConverter timeSpanConverter)
    {
        _saleRepository = saleRepository;
        _userRepository = userRepository;
        _beanRepository = beanRepository;
        _longtermTimespan = timeSpanConverter.Convert(settings.Value.LongTermSpan);
    }

    public async Task<int> CountAsync() => await _saleRepository.CountAsync();

    private async Task<ApiError> ValidateModelAsync(SaleModel model, bool checkid = false)
    {
        if (model is null || IdEncoder.DecodeId(model.Id) <= 0 || IdEncoder.DecodeId(model.BeanId) <= 0 || model.CostBasis <= 0M || model.SalePrice <= 0M ||
          model.SaleDate == default || model.Quantity == 0)
        {
            return new(Strings.InvalidModel);
        }
        if (model.SaleDate == default)
        {
            model.SaleDate = DateTime.UtcNow;
        }
        if (model.SaleDate < model.PurchaseDate)
        {
            return new(string.Format(Strings.Invalid, "sale date"));
        }
        var user = await _userRepository.ReadAsync(model.UserId);
        if (user is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", model.UserId));
        }
        var bean = await _beanRepository.ReadAsync(model.BeanId);
        if (bean is null)
        {
            return new(string.Format(Strings.NotFound, "bean", "id", model.BeanId));
        }
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = IdEncoder.EncodeId(0);
        }
        if (checkid && IdEncoder.DecodeId(model.Id) == 0)
        {
            return new(string.Format(Strings.Invalid, "id"));
        }
        return ApiError.Success;
    }

    public async Task<ApiError> InsertAsync(SaleModel model)
    {
        var checkresult = await ValidateModelAsync(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        SaleEntity entity = model!;
        try
        {
            var result = await _saleRepository.InsertAsync(entity);
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

    public async Task<ApiError> UpdateAsync(SaleModel model)
    {
        var checkresult = await ValidateModelAsync(model, true);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        SaleEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _saleRepository.UpdateAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAsync(SaleModel model)
    {
        if (model is null)
        {
            return new(Strings.InvalidModel);
        }
        try
        {
            return ApiError.FromDalResult(await _saleRepository.DeleteAsync(IdEncoder.DecodeId(model.Id)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    private IEnumerable<SaleModel> Finish(IEnumerable<SaleEntity> entities)
    {
        var models = entities.ToModels<SaleModel, SaleEntity>();
        models.ForEach(x =>
        {
            x.CanDelete = true;
            x.LongTerm = x.SaleDate - x.PurchaseDate >= _longtermTimespan;
        });
        return models;
    }

    public async Task<IEnumerable<SaleModel>> GetAsync()
    {
        var entities = await _saleRepository.GetAsync();
        return Finish(entities);
    }

    public async Task<IEnumerable<SaleModel>> GetForUserAsync(string userid)
    {
        var entities = await _saleRepository.GetForUserAsync(IdEncoder.DecodeId(userid));
        return Finish(entities);
    }

    public async Task<IEnumerable<SaleModel>> GetForUserAsync(string userid, int days)
    {
        var entities = await _saleRepository.GetForUserAsync(IdEncoder.DecodeId(userid), days);
        return Finish(entities);
    }

    public async Task<IEnumerable<SaleModel>> GetForUserAndBeanAsync(string userid, string beanid)
    {
        var entities = await _saleRepository.GetForUserAndBeanAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));
        return Finish(entities);
    }

    public async Task<IEnumerable<SaleModel>> GetForUserAndBeanAsync(string userid, string beanid, int days)
    {
        var entities = await _saleRepository.GetForUserAndBeanAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid), days);
        return Finish(entities);
    }

    public async Task<SaleModel?> ReadAsync(string id)
    {
        var entity = await _saleRepository.ReadAsync(IdEncoder.DecodeId(id));
        if (entity is not null)
        {
            SaleModel model = entity!;
            model.CanDelete = true;
            return model;
        }
        return null;
    }

    public async Task<bool> UserHasSalesAsync(string userid) => await _saleRepository.UserHasSalesAsync(IdEncoder.DecodeId(userid));

    public async Task<bool> UserHasSoldAsync(string userid, string beanid) =>
      await _saleRepository.UserHasSoldAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));

    public async Task<bool> BeanHasSalesAsync(string beanid) => await _saleRepository.BeanHasSalesAsync(IdEncoder.DecodeId(beanid));

    public async Task<decimal> ProfitOrLossAsync(string userid) => await _saleRepository.ProfitOrLossAsync(IdEncoder.DecodeId(userid));

    public async Task<decimal> ProfitOrLossAsync(string userid, string beanid) =>
      await _saleRepository.ProfitOrLossAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));

    public async Task<decimal> ProfitOrLossAsync(string userid, DateTime startDate, DateTime endDate) =>
      await _saleRepository.ProfitOrLossAsync(IdEncoder.DecodeId(userid), startDate, endDate);

    public async Task<decimal> ProfitOrLossAsync(string userid, string beanid, DateTime startDate, DateTime endDate) =>
      await _saleRepository.ProfitOrLossAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid), startDate, endDate);
}
