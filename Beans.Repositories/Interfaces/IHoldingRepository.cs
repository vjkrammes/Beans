
using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Repositories.Interfaces;
public interface IHoldingRepository : IRepository<HoldingEntity>
{
    Task<IEnumerable<HoldingEntity>> GetForUserAsync(int userid);
    Task<IEnumerable<HoldingEntity>> GetForBeanAsync(int beanid);
    Task<IEnumerable<HoldingEntity>> GetForBeanAsync(int userid, int beanid);
    Task<IEnumerable<HoldingEntity>> SearchAsync(int userid, int beanid, DateTime startDate, DateTime endDate);
    Task<long> BeansHeldByUserAsync(int userid);
    Task<long> BeansHeldByUserAndBeanAsync(int userid, int beanid);
    Task<long> BeansHeldByBeanAsync(int beanid);
    Task<CostBasis> GetCostBasisAsync(int userid);
    Task<CostBasis> GetCostBasisAsync(int userid, int beanid);
    Task<Dictionary<string, long>> BeansHeldAsync();
    Task<bool> UserHasHoldingsAsync(int userid);
    Task<bool> UserHasHoldingsAsync(int userid, int beanid);
    Task<bool> BeanHasHoldingsAsync(int beanid);
    Task<int[]> GetHoldingsAsync(bool oldestFirst, int userid, int beanid, long quantity);
    Task<long> HoldingCountAsync(int userid, int? beanid);
    Task<decimal> TotalValueAsync(int userid);
    Task<decimal> TotalValueAsync(int userid, int beanid);
    Task<decimal> TotalCostAsync(int userid);
    Task<decimal> TotalCostAsync(int userid, int beanid);
    Task<DalResult> ResetHoldingsAsync();
}
