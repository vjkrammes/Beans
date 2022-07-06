using Beans.Common;
using Beans.Models;

namespace Beans.Services.Interfaces;
public interface IOfferService : IDataService<OfferModel>
{
    Task<ApiError> CreateAsync(string userid, string beanid, string holdingid, long quantity, decimal price, bool buy);
    Task<IEnumerable<OfferModel>> GetForUserAsync(string userid);
    Task<IEnumerable<OfferModel>> GetOtherOffersAsync(string userid);
    Task<IEnumerable<OfferModel>> GetForBeanAsync(string beanid);
    Task<IEnumerable<OfferModel>> GetForHoldingAsync(string holdingid);
    Task<IEnumerable<OfferModel>> GetOffersAsync(string sellerid, string? beanid, bool includeExpired);
    Task<bool> UserHasOffersAsync(string userid);
    Task<bool> BeanHasOffersAsync(string beanid);
    Task<bool> HoldingHasOffersAsync(string holdingid);
    Task<ApiError> BuyFromOfferAsync(string buyerid, long quantity, string offerid, bool oldestFirst);
    Task<ApiError> SellToOfferAsync(string offerid, string sellerid, SellToOfferItem[] items);
}
