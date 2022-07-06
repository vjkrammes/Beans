
using Beans.Common;
using Beans.Repositories.Entities;

namespace Beans.Repositories.Interfaces;
public interface IMovementRepository : IRepository<MovementEntity>
{
    Task<DalResult> InsertAsync(MovementEntity movement, BeanEntity bean);
    Task<IEnumerable<MovementEntity>> GetForBeanAsync(int beandid);
    Task<IEnumerable<MovementEntity>> GetForBeanAsync(int beanid, int days);
    Task<IEnumerable<int>> BeanIdsAsync();
    Task<IEnumerable<MovementEntity>> MostRecentAsync();
    Task<IEnumerable<MovementEntity>> MostRecentAsync(int[] beanids);
    Task<IEnumerable<MovementEntity>> TopForBeanAsync(int beanid, int count);
    Task<IEnumerable<MovementEntity>> HistoryAsync(int beanid, DateTime date);
    Task<MovementEntity> MostRecentAsync(int beanid);
    Task<MovementEntity> ReadForDateAsync(int beanid, DateTime date);
    Task<bool> BeanHasMovementsAsync(int beanid);
    Task<DateTime> LowestDateAsync();
    Task<DalResult> MakeMovementAsync(int beanid, decimal minPrice, DateTime date);
    Task<DalResult> CatchupAsync(int beanid, decimal lowestValue, DateTime startDate);
    Task<DalResult> MoveAsync(int beanid, decimal lowestValue, DateTime date);
    Task<decimal> GetMinRangeAsync(int beanid, int days);
    Task<decimal> GetAverageRangeAsync(int beanid, int days);
    Task<decimal> GetMaxRangeAsync(int beanid, int days);
    Task<decimal> GetLargestMovementAsync(int beanid);
    Task<decimal> GetStandardDeviationAsync(int beanid, int days);
}
