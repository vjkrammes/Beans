using Beans.Common;
using Beans.Models;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Services.Interfaces;

namespace Beans.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IHoldingRepository _holdingRepository;
    private readonly INoticeRepository _noticeRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ISettingsService _settingsService;
    private readonly IBeanRepository _beanRepository;

    public UserService(IUserRepository userRepository, IHoldingRepository holdingRepository, INoticeRepository noticeRepository,
      IOfferRepository offerRepository, ISaleRepository saleRepository, ISettingsService settingsService, IBeanRepository beanRepository)
    {
        _userRepository = userRepository;
        _holdingRepository = holdingRepository;
        _noticeRepository = noticeRepository;
        _offerRepository = offerRepository;
        _saleRepository = saleRepository;
        _settingsService = settingsService;
        _beanRepository = beanRepository;
    }

    public async Task<int> CountAsync() => await _userRepository.CountAsync();

    private async Task<ApiError> ValidateModelAsync(UserModel model, bool checkid = false, bool update = false)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Identifier) || string.IsNullOrWhiteSpace(model.Email) || 
            string.IsNullOrWhiteSpace(model.DisplayName) || model.Balance < 0M || model.OwedToExchange < 0M)
        {
            return new(Strings.InvalidModel);
        }
        if (model.DateJoined == default)
        {
            model.DateJoined = DateTime.UtcNow;
        }
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            model.Id = IdEncoder.EncodeId(0);
        }
        if (checkid && IdEncoder.DecodeId(model.Id) == 0)
        {
            return new(string.Format(Strings.Invalid, "id"));
        }
        var existing = await _userRepository.ReadAsync(model.Email);
        if (update)
        {
            if (existing is not null && existing.Id != IdEncoder.DecodeId(model.Id))
            {
                return new(string.Format(Strings.Duplicate, "a", "user", "email", model.Email));
            }
        }
        else if (existing is not null)
        {
            return new(string.Format(Strings.Duplicate, "a", "user", "email", model.Email));
        }
        return ApiError.Success;
    }

    public async Task<ApiError> InsertAsync(UserModel model)
    {
        var checkresult = await ValidateModelAsync(model);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        UserEntity entity = model!;
        try
        {
            var result = await _userRepository.InsertAsync(entity);
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

    public async Task<ApiError> UpdateAsync(UserModel model)
    {
        var checkresult = await ValidateModelAsync(model, true, true);
        if (!checkresult.Successful)
        {
            return checkresult;
        }
        UserEntity entity = model!;
        try
        {
            return ApiError.FromDalResult(await _userRepository.UpdateAsync(entity));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    public async Task<ApiError> DeleteAsync(UserModel model)
    {
        if (model is null)
        {
            return new(Strings.InvalidModel);
        }
        var user = await _userRepository.ReadAsync(model.Id);
        if (user is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", model.Id));
        }
        if (user.IsAdmin)
        {
            return new(Strings.CantDeleteAdmins);
        }
        try
        {
            return ApiError.FromDalResult(await _userRepository.DeleteAsync(IdEncoder.DecodeId(model.Id)));
        }
        catch (Exception ex)
        {
            return ApiError.FromException(ex);
        }
    }

    private async Task<bool> UserCanBeDeleted(string userid)
    {
        var hasHoldings = await _holdingRepository.UserHasHoldingsAsync(IdEncoder.DecodeId(userid));
        var hasNotices = await _noticeRepository.UserHasNoticesAsync(IdEncoder.DecodeId(userid));
        var hasOffers = await _offerRepository.UserHasOffersAsync(IdEncoder.DecodeId(userid));
        var hasSales = await _saleRepository.UserHasSalesAsync(IdEncoder.DecodeId(userid));
        return Tools.Any(hasHoldings, hasNotices, hasOffers, hasSales) == false;
    }

    private async Task<IEnumerable<UserModel>> Finish(IEnumerable<UserEntity> entities)
    {
        var models = entities.ToModels<UserModel, UserEntity>();
        foreach (var model in models)
        {
            model.CanDelete = await UserCanBeDeleted(model.Id);
        }
        return models;
    }

    public async Task<IEnumerable<UserModel>> GetAsync()
    {
        var entities = await _userRepository.GetAsync();
        return await Finish(entities);
    }

    private async Task<UserModel?> Finish(UserEntity entity)
    {
        UserModel model = entity!;
        if (model is not null)
        {
            model.CanDelete = await UserCanBeDeleted(model.Id);
            return model;
        }
        return null;
    }

    public async Task<UserModel?> ReadAsync(string id)
    {
        var entity = await _userRepository.ReadAsync(IdEncoder.DecodeId(id));
        return await Finish(entity!);
    }

    public async Task<UserModel?> ReadForEmailAsync(string email)
    {
        var entity = await _userRepository.ReadAsync(email);
        return await Finish(entity!);
    }

    public async Task<UserModel?> ReadForIdentifierAsync(string identifier)
    {
        var entity = await _userRepository.ReadByIdentifierAsync(identifier);
        return await Finish(entity!);
    }

    public async Task<DateTime> JoinedAsync(string userid) => await _userRepository.JoinedAsync(IdEncoder.DecodeId(userid));

    public async Task<ApiError> AddRegisteredUserAsync(UserModel model) => await InsertAsync(model);


    public async Task<bool> UserExistsByIdAsync(string userid)
    {
        var user = await _userRepository.ReadAsync(IdEncoder.DecodeId(userid));
        return user is not null;
    }

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        var user = await _userRepository.ReadAsync(email);
        return user is not null;
    }

    public async Task<bool> UserExistsByIdentifierAsync(string identifier)
    {
        var user = await _userRepository.ReadByIdentifierAsync(identifier);
        return user is not null;
    }

    public async Task<IEnumerable<NameModel>> GetNamesAsync()
    {
        var users = await GetAsync();
        List<NameModel> ret = new();
        foreach (var user in users)
        {
            var name = NameModel.FromUserModel(user)!;
            ret.Add(name);
        }
        return ret;
    }

    public async Task<string> GetNameAsync(string userid)
    {
        var user = await ReadAsync(userid);
        if (user is not null)
        {
            return user.DisplayName;
        }
        return "(unknown)";
    }

    public async Task<ApiError> ChangeNameAsync(string userid, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return new(string.Format(Strings.Invalid, "user id"));
        }
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return new(string.Format(Strings.Invalid, "first name"));
        }
        if (string.IsNullOrWhiteSpace(lastName))
        {
            return new(string.Format(Strings.Invalid, "last name"));
        }
        var user = await ReadAsync(userid);
        if (user is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", userid));
        }
        user.FirstName = firstName;
        user.LastName = lastName;
        return await UpdateAsync(user);
    }

    public async Task<ApiError> LoanAsync(string userid, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return new(string.Format(Strings.Invalid, "user id"));
        }
        if (amount <= 0)
        {
            return new(string.Format(Strings.Invalid, "amount"));
        }
        var pid = IdEncoder.DecodeId(userid);
        var user = await _userRepository.ReadAsync(pid);
        if (user is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", userid));
        }
        var (valid, value) = await _settingsService.ReadDecimalSetting(Constants.MAXIMUM_LOAN_BALANCE);
        if (valid && user.OwedToExchange + amount > value)
        {
            return new(Strings.MLBExceeded);
        }
        user.Balance += amount;
        user.OwedToExchange += amount;
        return ApiError.FromDalResult(await _userRepository.UpdateAsync(user));
    }

    public async Task<ApiError> RepayAsync(string userid, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(userid))
        {
            return new(string.Format(Strings.Invalid, "user id"));
        }
        if (amount <= 0)
        {
            return new(string.Format(Strings.Invalid, "amount"));
        }
        var pid = IdEncoder.DecodeId(userid);
        var user = await _userRepository.ReadAsync(pid);
        if (user is null)
        {
            return new(string.Format(Strings.NotFound, "user", "id", userid));
        }
        if (amount > user.OwedToExchange || amount > user.Balance)
        {
            return new(string.Format(Strings.Invalid, "amount"));
        }
        user.OwedToExchange -= amount;
        user.Balance -= amount;
        return ApiError.FromDalResult(await _userRepository.UpdateAsync(user));
    }

    public async Task<ApiError> ResetUsersAsync() => ApiError.FromDalResult(await _userRepository.ResetUsersAsync());

    public async Task<ApiError> ToggleAdminAsync(string userid)
    {
        var pid = IdEncoder.DecodeId(userid);
        return ApiError.FromDalResult(await _userRepository.ToggleAdminAsync(pid));
    }

    public async Task<IEnumerable<LeaderboardEntry>> GetLeaderboardAsync()
    {
        List<LeaderboardEntry> ret = new();
        var users = await _userRepository.GetAsync();
        if (users is null || !users.Any())
        {
            return ret.ToArray();
        }
        foreach (var user in users)
        {
            LeaderboardEntry entry = new()
            {
                UserId = IdEncoder.EncodeId(user.Id),
                DisplayName = user.DisplayName,
            };
            var holdings = await _holdingRepository.GetForUserAsync(user.Id);
            if (holdings is null || !holdings.Any())
            {
                ret.Add(entry);
                continue;
            }
            long q = 0;
            var cost = 0M;
            var price = 0M;
            foreach (var holding in holdings)
            {
                var bean = await _beanRepository.ReadAsync(holding.BeanId);
                if (bean is null)
                {
                    continue;
                }
                q += holding.Quantity;
                cost += holding.Quantity * holding.Price;
                price += holding.Quantity * bean.Price;
            }
            entry.Quantity = q;
            entry.Basis = cost;
            entry.Value = price;
            entry.GainOrLoss = price - cost;
            entry.Change = cost == 0M ? 0.0 : (double)((price - cost) / cost * 100);
            var sales = await _saleRepository.GetForUserAsync(user.Id);
            var salesbasis = 0M;
            var salestotal = 0M;
            foreach (var sale in sales)
            {
                salesbasis += sale.CostBasis;
                salestotal += sale.SalePrice;
            }
            entry.SalesBasis = salesbasis;
            entry.SalesTotal = salestotal;
            entry.SalesGainOrLoss = salestotal - salesbasis;
            entry.SalesChange = salesbasis == 0M ? 0.0 : (double)((salestotal - salesbasis) / salesbasis) * 100;
            entry.Score = entry.Change + entry.SalesChange;
            ret.Add(entry);
        }
        return ret.ToArray();
    }
}
