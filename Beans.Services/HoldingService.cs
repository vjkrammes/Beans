using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

namespace Beans.Services;
public class HoldingService : IHoldingService
{
    private readonly IHoldingRepository _holdingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBeanRepository _beanRepository;
    private readonly IOfferRepository _offerRepository;

    public HoldingService(IHoldingRepository holdingRepository, IUserRepository userRepository, IBeanRepository beanRepository, IOfferRepository offerRepository)
    {
        _holdingRepository = holdingRepository;
        _userRepository = userRepository;
        _beanRepository = beanRepository;
        _offerRepository = offerRepository;
    }

    public async Task<int> CountAsync() => await _holdingRepository.CountAsync();

    private async Task<ApiError> ValidateModelAsync(HoldingModel model, bool checkid = false)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.BeanId) || model.Quantity == 0 || model.Price <= 0M)
        {
            return new(Strings.InvalidModel);
        }
        if (model.PurchaseDate == default)
        {
            model.PurchaseDate = DateTime.UtcNow;
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

    public async Task<ApiError> InsertAsync(HoldingModel model)
    {
        var checkresult = await ValidateModelAsync(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        HoldingEntity entity = model!;
        try
        {
            var result = await _holdingRepository.InsertAsync(entity);
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

    public async Task<ApiError> UpdateAsync(HoldingModel model)
    {
        var checkresult = await ValidateModelAsync(model, true);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        HoldingEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _holdingRepository.UpdateAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAsync(HoldingModel model)
    {
        if (model is null)
        {
            return new(Strings.InvalidModel);
        }
        try
        {
            return ApiError.FromDalResult(await _holdingRepository.DeleteAsync(IdEncoder.DecodeId(model.Id)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    private async Task<IEnumerable<HoldingModel>> Finish(IEnumerable<HoldingEntity> entities)
    {
        var models = entities.ToModels<HoldingModel, HoldingEntity>();
        if (models is not null && models.Any())
        {
            foreach (var model in models)
            {
                model.CanDelete = !await _offerRepository.HoldingHasOffersAsync(IdEncoder.DecodeId(model.Id));
                if (model.Bean is null)
                {
                    model.Bean = (await _beanRepository.ReadAsync(IdEncoder.DecodeId(model.BeanId)))!;
                }
            }
        }
        return models!;
    }

    public async Task<IEnumerable<HoldingModel>> GetAsync()
    {
        var entities = await _holdingRepository.GetAsync();
        return await Finish(entities);
    }

    public async Task<IEnumerable<HoldingModel>> GetForUserAsync(string userid)
    {
        var entities = await _holdingRepository.GetForUserAsync(IdEncoder.DecodeId(userid));
        return await Finish(entities);
    }

    public async Task<IEnumerable<HoldingModel>> GetForBeanAsync(string beanid)
    {
        var entities = await _holdingRepository.GetForBeanAsync(IdEncoder.DecodeId(beanid));
        return await Finish(entities);
    }

    public async Task<IEnumerable<HoldingModel>> GetForBeanAsync(string userid, string beanid)
    {
        var entities = await _holdingRepository.GetForBeanAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));
        return await Finish(entities);
    }

    public async Task<IEnumerable<HoldingModel>> SearchAsync(SearchHoldingsModel model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Id))
        {
            return new List<HoldingModel>();
        }
        var uid = IdEncoder.DecodeId(model.Id);
        var bid = string.IsNullOrWhiteSpace(model.BeanId) ? 0 : IdEncoder.DecodeId(model.BeanId);
        var entities = await _holdingRepository.SearchAsync(uid, bid, model.StartDate, model.EndDate);
        return await Finish(entities);
    }

    public async Task<HoldingModel?> ReadAsync(string id)
    {
        var entity = await _holdingRepository.ReadAsync(IdEncoder.DecodeId(id));
        if (entity is not null)
        {
            HoldingModel model = entity!;
            model.CanDelete = true;
            return model;
        }
        return null;
    }

    public async Task<long> BeansHeldByUserAsync(string userid) => await _holdingRepository.BeansHeldByUserAsync(IdEncoder.DecodeId(userid));

    public async Task<long> BeansHeldByUserAndBeanAsync(string userid, string beanid) =>
      await _holdingRepository.BeansHeldByUserAndBeanAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));

    public async Task<long> BeansHeldByBeanAsync(string beanid) => await _holdingRepository.BeansHeldByBeanAsync(IdEncoder.DecodeId(beanid));
    public async Task<CostBasis> GetCostBasisAsync(string userid) => await _holdingRepository.GetCostBasisAsync(IdEncoder.DecodeId(userid));

    public async Task<CostBasisModel[]> GetCostBasesAsync(string userid)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return Array.Empty<CostBasisModel>();
        }
        var uid = IdEncoder.DecodeId(userid);
        var ret = new List<CostBasisModel>();
        var beanids = await _beanRepository.BeanIdsAsync();
        if (beanids is not null && beanids.Any())
        {
            foreach (var beanid in beanids)
            {
                var bean = await _beanRepository.ReadAsync(beanid);
                if (bean is null)
                {
                    continue;
                }
                var basis = await _holdingRepository.GetCostBasisAsync(uid, beanid);
                if (basis is not  null && basis.Type is CostBasisType.Basis or CostBasisType.Average)
                {
                    var model = new CostBasisModel()
                    {
                        BeanId = IdEncoder.EncodeId(beanid),
                        BeanName = bean.Name,
                        Filename = bean.Filename,
                        BasisType = basis.Type,
                        Basis = basis.Basis,
                        GainOrLoss = bean.Price - basis.Basis,
                        Percent = (bean.Price - basis.Basis) / basis.Basis * 100,
                    };
                    ret.Add(model);
                }
            }
        }
        return ret.ToArray();
    }

    public async Task<CostBasis> GetCostBasisAsync(string userid, string beanid) =>
      await _holdingRepository.GetCostBasisAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));

    public async Task<HoldingSummaryModel> SummaryAsync(string userid, string beanid)
    {
        var ret = new HoldingSummaryModel();
        var holdings = await _holdingRepository.GetForBeanAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));
        if (holdings is null || !holdings.Any())
        {
            return ret;
        }
        holdings.ForEach(x => ret.Quantity += x.Quantity);
        ret.CostBasis = await GetCostBasisAsync(userid, beanid);
        return ret;
    }

    public async Task<Dictionary<string, long>> BeansHeldAsync() => await _holdingRepository.BeansHeldAsync();

    public async Task<bool> UserHasHoldingsAsync(string userid) => await _holdingRepository.UserHasHoldingsAsync(IdEncoder.DecodeId(userid));

    public async Task<bool> UserHasHoldingsAsync(string userid, string beanid) =>
      await _holdingRepository.UserHasHoldingsAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));

    public async Task<bool> BeanHasHoldingsAsync(string beanid) => await _holdingRepository.BeanHasHoldingsAsync(IdEncoder.DecodeId(beanid));

    public async Task<string[]> GetHoldingsAsync(bool oldestFirst, string userid, string beanid, long quantity) =>
      (await _holdingRepository.GetHoldingsAsync(oldestFirst, IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid), quantity))
        .Select(x => IdEncoder.EncodeId(x)).ToArray();

    public async Task<long> HoldingCountAsync(string userid, string? beanid) =>
      await _holdingRepository.HoldingCountAsync(IdEncoder.DecodeId(userid), string.IsNullOrWhiteSpace(beanid) ? 0 : IdEncoder.DecodeId(beanid));

    public async Task<decimal> TotalValueAsync(string userid) => await _holdingRepository.TotalValueAsync(IdEncoder.DecodeId(userid));

    public async Task<decimal> TotalValueAsync(string userid, string beanid) =>
      await _holdingRepository.TotalValueAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));

    public async Task<decimal> TotalCostAsync(string userid) => await _holdingRepository.TotalCostAsync(IdEncoder.DecodeId(userid));

    public async Task<decimal> TotalCostAsync(string userid, string beanid) =>
      await _holdingRepository.TotalCostAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid));

    public async Task<ApiError> ResetHoldingsAsync() => ApiError.FromDalResult(await _holdingRepository.ResetHoldingsAsync());
}
