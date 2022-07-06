using Beans.Common;
using Beans.Common.Interfaces;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

namespace Beans.Services;
public class BeanService : IBeanService
{
    private readonly IBeanRepository _beanRepository;
    private readonly IHoldingRepository _holdingRepository;
    private readonly IMovementRepository _movementRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IColorService _colorService;

    public BeanService(IBeanRepository beanRepository, IHoldingRepository holdingRepository, IMovementRepository movementReposotory,
      IOfferRepository offerRepository, ISaleRepository saleRepository, IColorService colorService)
    {
        _beanRepository = beanRepository;
        _holdingRepository = holdingRepository;
        _movementRepository = movementReposotory;
        _offerRepository = offerRepository;
        _saleRepository = saleRepository;
        _colorService = colorService;
    }

    public async Task<int> CountAsync() => await _beanRepository.CountAsync();

    private async Task<ApiError> ValidateModelAsync(BeanModel model, bool checkid = false, bool update = false)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name) || model.Price <= 0M || string.IsNullOrWhiteSpace(model.Filename))
        {
            return new(Strings.InvalidModel);
        }
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = IdEncoder.EncodeId(0);
        }
        if (checkid && IdEncoder.DecodeId(model.Id) == 0)
        {
            return new(string.Format(Strings.Invalid, "id"));
        }
        var existing = await _beanRepository.ReadAsync(model.Name);
        if (update)
        {
            if (existing is not null && existing.Id != IdEncoder.DecodeId(model.Id))
            {
                return new(string.Format(Strings.Duplicate, "a", "bean", "name", model.Name));
            }
        }
        else if (existing is not null)
        {
            return new(string.Format(Strings.Duplicate, "a", "bean", "name", model.Name));
        }
        if (model.ARGB == 0)
        {
            model.ARGB = (long)_colorService.GetLongARGB(model.Name);
        }
        return ApiError.Success;
    }

    public async Task<ApiError> InsertAsync(BeanModel model)
    {
        var checkresult = await ValidateModelAsync(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        BeanEntity entity = model!;
        try
        {
            var result = await _beanRepository.InsertAsync(entity);
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

    public async Task<ApiError> UpdateAsync(BeanModel model)
    {
        var checkresult = await ValidateModelAsync(model, true, true);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        BeanEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _beanRepository.UpdateAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAsync(BeanModel model)
    {
        if (model is null)
        {
            return new(Strings.InvalidModel);
        }
        try
        {
            return ApiError.FromDalResult(await _beanRepository.DeleteAsync(IdEncoder.DecodeId(model.Id)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    private async Task<bool> BeanCanBeDeleted(string beanid)
    {
        var hasHoldings = await _holdingRepository.BeanHasHoldingsAsync(IdEncoder.DecodeId(beanid));
        var hasMovements = await _movementRepository.BeanHasMovementsAsync(IdEncoder.DecodeId(beanid));
        var hasOffers = await _offerRepository.BeanHasOffersAsync(IdEncoder.DecodeId(beanid));
        var hasSales = await _saleRepository.BeanHasSalesAsync(IdEncoder.DecodeId(beanid));
        return Tools.Any(hasHoldings, hasMovements, hasOffers, hasSales) == false;
    }

    public async Task<IEnumerable<BeanModel>> GetAsync()
    {
        var entities = await _beanRepository.GetAsync();
        var models = entities.ToModels<BeanModel, BeanEntity>();
        foreach (var entity in models)
        {
            entity.CanDelete = await BeanCanBeDeleted(entity.Id);
        }
        return models;
    }

    public async Task<BeanModel?> ReadAsync(string id) => (await _beanRepository.ReadAsync(IdEncoder.DecodeId(id)))!;

    public async Task<BeanModel?> ReadForNameAsync(string name) => (await _beanRepository.ReadAsync(name))!;

    public async Task<IEnumerable<string>> BeanIdsAsync() => (await _beanRepository.BeanIdsAsync()).Select(x => IdEncoder.EncodeId(x));

    public async Task<IEnumerable<string>> BeanIdsAsync(string userid) =>
      (await _beanRepository.BeanIdsAsync(IdEncoder.DecodeId(userid))).Select(x => IdEncoder.EncodeId(x));

    public async Task<long> PlayerHeldAsync(string beanid) => await _beanRepository.PlayerHeldAsync(IdEncoder.DecodeId(beanid));

    public async Task<long> ExchangeHeldAsync(string beanid) => await _beanRepository.ExchangeHeldAsync(IdEncoder.DecodeId(beanid));

    public async Task<decimal> CapitalizationAsync() => await _beanRepository.CapitalizationAsync();

    public async Task<decimal> CapitalizationAsync(string beanid) => await _beanRepository.CapitalizationAsync(IdEncoder.DecodeId(beanid));

    public async Task<BeanHistoryModel?> HistoryAsync(string beanid, int days = int.MaxValue)
    {
        var bean = await _beanRepository.ReadAsync(IdEncoder.DecodeId(beanid));
        if (days <= 0 || bean is null)
        {
            return null;
        }
        var cutoff = days == int.MaxValue ? default : (DateTime.UtcNow - TimeSpan.FromDays(days - 1)).Date;
        var ret = new BeanHistoryModel
        {
            BeanId = IdEncoder.EncodeId(bean.Id),
            BeanName = bean.Name,
            ImageName = bean.Filename,
            Price = bean.Price,
            Basis = CostBasis.Default,
            Quantity = 0,
            Days = days,
            Movements = Array.Empty<MovementModel>()
        };
        var sql = $"select * from Movements where BeanId={bean.Id} and CAST(MovementDate as Date) >= '{cutoff:yyyy-MM-dd}' order by MovementDate";
        var movements = (await _movementRepository.GetAsync(sql));
        ret.Movements = movements.ToModels<MovementModel, MovementEntity>().ToArray();
        return ret;
    }

    public async Task<IEnumerable<BeanHistoryModel>> AllBeanHistoryAsync(int days = int.MaxValue)
    {
        var ret = new List<BeanHistoryModel>();
        var ids = await BeanIdsAsync();
        if (ids is not null && ids.Any())
        {
            foreach (var id in ids)
            {
                var model = await HistoryAsync(id, days);
                if (model is not null)
                {
                    ret.Add(model);
                }
            }
        }
        return ret;
    }

    public async Task<ApiError> SellToExchangeAsync(string holdingid, long quantity) =>
      ApiError.FromDalResult(await _beanRepository.SellToExchangeAsync(IdEncoder.DecodeId(holdingid), quantity));

    public async Task<ApiError> BuyFromExchangeAsync(string userid, string beanid, long quantity) =>
      ApiError.FromDalResult(await _beanRepository.BuyFromExchangeAsync(IdEncoder.DecodeId(userid), IdEncoder.DecodeId(beanid), quantity));
}
