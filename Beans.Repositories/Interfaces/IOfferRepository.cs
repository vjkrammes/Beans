
using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Repositories.Interfaces;
public interface IOfferRepository : IRepository<OfferEntity>
{
    Task<IEnumerable<OfferEntity>> GetForUserAsync(int userid);
    Task<IEnumerable<OfferEntity>> GetOtherOffersAsync(int userid);
    Task<IEnumerable<OfferEntity>> GetForBeanAsync(int beanid);
    Task<IEnumerable<OfferEntity>> GetForHoldingAsync(int holdingid);
    Task<IEnumerable<OfferEntity>> GetOffersAsync(int sellerid, int beanid, bool includeExpired);
    Task<bool> UserHasOffersAsync(int userid);
    Task<bool> BeanHasOffersAsync(int beanid);
    Task<bool> HoldingHasOffersAsync(int holdingid);
    Task<DalResult> BuyFromOfferAsync(int buyerid, long quantity, int offerid, bool oldestFirst);
    Task<DalResult> SellToOfferAsync(int offerid, int sellerid, (int holdingid, long quantity)[] holdings);
}
