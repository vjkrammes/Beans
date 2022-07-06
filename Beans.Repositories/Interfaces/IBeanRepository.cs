
using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Repositories.Interfaces;
public interface IBeanRepository : IRepository<BeanEntity>
{
    Task<IEnumerable<int>> BeanIdsAsync();
    Task<IEnumerable<int>> BeanIdsAsync(int userid);
    Task<BeanEntity?> ReadAsync(string name);
    Task<long> PlayerHeldAsync(int beanid);
    Task<long> ExchangeHeldAsync(int beanid);
    Task<decimal> CapitalizationAsync();
    Task<decimal> CapitalizationAsync(int beanid);
    Task<DalResult> SellToExchangeAsync(int holdingid, long quantity);
    Task<DalResult> BuyFromExchangeAsync(int userid, int beanid, long quantity);
}
