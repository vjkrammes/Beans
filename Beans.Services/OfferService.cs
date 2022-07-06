using Beans.Common;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

namespace Beans.Services;
public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBeanRepository _beanRepository;
    private readonly IHoldingRepository _holdingRepository;

    public OfferService(IOfferRepository offerRepository, IUserRepository userRepository, IBeanRepository beanRepository, IHoldingRepository holdingRepository)
    {
        _offerRepository = offerRepository;
        _userRepository = userRepository;
        _beanRepository = beanRepository;
        _holdingRepository = holdingRepository;
    }

    public async Task<int> CountAsync() => await _offerRepository.CountAsync();

    private async Task<ApiError> ValidateModelAsync(OfferModel model, bool checkid = false)
    {
        if (model is null || IdEncoder.DecodeId(model.UserId) <= 0 || IdEncoder.DecodeId(model.BeanId) <= 0 || model.Quantity == 0 || model.Price <= 0M)
        {
            return new(Strings.InvalidModel);
        }
        if (model.OfferDate == default)
        {
            model.OfferDate = DateTime.UtcNow;
        }
        var user = await _userRepository.ReadAsync(IdEncoder.DecodeId(model.UserId));
        if (user is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", model.UserId));
        }
        var bean = await _beanRepository.ReadAsync(IdEncoder.DecodeId(model.BeanId));
        if (bean is null)
        {
            return new(string.Format(Strings.NotFound, "bean", "id", model.BeanId));
        }
        if (model.Buy)
        {
            model.HoldingId = IdEncoder.EncodeId(0);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.HoldingId))
            {
                return new(string.Format(Strings.Required, "holding id"));
            }
            var holding = await _holdingRepository.ReadAsync(IdEncoder.DecodeId(model.HoldingId));
            if (holding is null)
            {
                return new(string.Format(Strings.NotFound, "holding", "id", model.HoldingId));
            }
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

    public async Task<ApiError> InsertAsync(OfferModel model)
    {
        var checkresult = await ValidateModelAsync(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        OfferEntity entity = model!;
        try
        {
            var result = await _offerRepository.InsertAsync(entity);
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

    public async Task<ApiError> UpdateAsync(OfferModel model)
    {
        var checkresult = await ValidateModelAsync(model, true);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        OfferEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _offerRepository.UpdateAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAsync(OfferModel model)
    {
        if (model is null)
        {
            return new(Strings.InvalidModel);
        }
        try
        {
            return ApiError.FromDalResult(await _offerRepository.DeleteAsync(IdEncoder.DecodeId(model.Id)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> CreateAsync(string userid, string beanid, string holdingid, long quantity, decimal price, bool buy)
    {
        var user = await _userRepository.ReadAsync(IdEncoder.DecodeId(userid));
        if (user is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", userid));
        }
        var bean = await _beanRepository.ReadAsync(IdEncoder.DecodeId(beanid));
        if (bean is null)
        {
            return new(string.Format(Strings.NotFound, "bean", "id", beanid));
        }
        if (quantity <= 0)
        {
            return new(string.Format(Strings.Invalid, "quantity"));
        }
        if (price <= 0M)
        {
            return new(string.Format(Strings.Invalid, "price"));
        }
        if (!buy)
        {
            var holding = await _holdingRepository.ReadAsync(IdEncoder.DecodeId(holdingid));
            if (holding is null)
            {
                return new(string.Format(Strings.NotFound, "holding", "id", holdingid));
            }
            if (quantity > holding.Quantity)
            {
                return new(string.Format(Strings.Invalid, "quantity"));
            }
        }
        var offer = new OfferModel
        {
            Id = IdEncoder.EncodeId(0),
            UserId = userid,
            BeanId = beanid,
            HoldingId = holdingid,
            Price = price,
            OfferDate = DateTime.UtcNow,
            Buy = buy,
            Quantity = quantity,
            Bean = null,
            User = null,
            Holding = null,
            CanDelete = true
        };
        return await InsertAsync(offer);
    }

    private async Task<IEnumerable<OfferModel>> Finish(IEnumerable<OfferEntity> entities)
    {
        var models = entities.ToModels<OfferModel, OfferEntity>();
        foreach (var model in models)
        {
            model.CanDelete = true;
            model.Bean = (await _beanRepository.ReadAsync(IdEncoder.DecodeId(model.BeanId)))!;
            model.Holding = (await _holdingRepository.ReadAsync(IdEncoder.DecodeId(model.HoldingId)))!;
        }
        return models;
    }

    public async Task<IEnumerable<OfferModel>> GetAsync()
    {
        var entities = await _offerRepository.GetAsync();
        return await Finish(entities);
    }

    public async Task<IEnumerable<OfferModel>> GetForUserAsync(string userid)
    {
        var entities = await _offerRepository.GetForUserAsync(IdEncoder.DecodeId(userid));
        return await Finish(entities);
    }

    public async Task<IEnumerable<OfferModel>> GetOtherOffersAsync(string userid)
    {
        var entities = await _offerRepository.GetOtherOffersAsync(IdEncoder.DecodeId(userid));
        return await Finish(entities);
    }

    public async Task<IEnumerable<OfferModel>> GetForBeanAsync(string beanid)
    {
        var entities = await _offerRepository.GetForBeanAsync(IdEncoder.DecodeId(beanid));
        return await Finish(entities);
    }

    public async Task<IEnumerable<OfferModel>> GetForHoldingAsync(string holdingid)
    {
        var entities = await _offerRepository.GetForHoldingAsync(IdEncoder.DecodeId(holdingid));
        return await Finish(entities);
    }

    public async Task<IEnumerable<OfferModel>> GetOffersAsync(string sellerid, string? beanid, bool includeExpired)
    {
        var entities = await _offerRepository.GetOffersAsync(IdEncoder.DecodeId(sellerid),
          string.IsNullOrWhiteSpace(beanid) ? 0 : IdEncoder.DecodeId(beanid), includeExpired);
        return await Finish(entities);
    }

    public async Task<OfferModel?> ReadAsync(string id)
    {
        var entity = await _offerRepository.ReadAsync(IdEncoder.DecodeId(id));
        if (entity is not null)
        {
            OfferModel model = entity!;
            model.CanDelete = true;
            model.Bean = (await _beanRepository.ReadAsync(entity.BeanId))!;
            model.Holding = (await _holdingRepository.ReadAsync(entity.HoldingId))!;
            return model;
        }
        return null;
    }

    public async Task<bool> UserHasOffersAsync(string userid) => await _offerRepository.UserHasOffersAsync(IdEncoder.DecodeId(userid));

    public async Task<bool> BeanHasOffersAsync(string beanid) => await _offerRepository.BeanHasOffersAsync(IdEncoder.DecodeId(beanid));

    public async Task<bool> HoldingHasOffersAsync(string holdingid) => await _offerRepository.HoldingHasOffersAsync(IdEncoder.DecodeId(holdingid));

    public async Task<ApiError> BuyFromOfferAsync(string buyerid, long quantity, string offerid, bool oldestfirst)
    {
        var bid = IdEncoder.DecodeId(buyerid);
        if (bid <= 0)
        {
            return new(string.Format(Strings.Invalid, "buyer id"));
        }
        if (quantity == 0)
        {
            return new(string.Format(Strings.Invalid, "quantity"));
        }
        var oid = IdEncoder.DecodeId(offerid);
        if (oid <= 0)
        {
            return new(string.Format(Strings.Invalid, "offer id"));
        }
        try
        {
            return ApiError.FromDalResult(await _offerRepository.BuyFromOfferAsync(bid, quantity, oid, oldestfirst));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> SellToOfferAsync(string offerid, string sellerid, SellToOfferItem[] items)
    {
        if (IdEncoder.DecodeId(offerid) <= 0)
        {
            return new(string.Format(Strings.Invalid, "offer id"));
        }
        if (IdEncoder.DecodeId(sellerid) <= 0)
        {
            return new(string.Format(Strings.Invalid, "seller id"));
        }
        if (items is null || !items.Any())
        {
            return new(string.Format(Strings.Required, "holdings"));
        }
        var seller = await _userRepository.ReadAsync(IdEncoder.DecodeId(sellerid));
        if (seller is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", sellerid));
        }
        var offer = await _offerRepository.ReadAsync(IdEncoder.DecodeId(offerid));
        if (offer is null)
        {
            return new(string.Format(Strings.NotFound, "offer", "id", offer));
        }
        var holdings = new List<(int, long)>();
        foreach (var item in items)
        {
            holdings.Add((IdEncoder.DecodeId(item.HoldingId), item.Quantity));
        }
        try
        {
            return ApiError.FromDalResult(await _offerRepository.SellToOfferAsync(offer.Id, seller.Id, 
                holdings.ToArray()));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }
}
