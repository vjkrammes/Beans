using Beans.Common;
using Beans.Models;

namespace Beans.Services.Interfaces;
public interface IMovementService : IDataService<MovementModel>
{
    Task<ApiError> InsertAsync(MovementModel movement, BeanModel bean);
    Task<IEnumerable<string>> BeanIdsAsync();
    Task<IEnumerable<MovementModel>> GetForBeanAsync(string beanid);
    Task<IEnumerable<MovementModel>> GetForBeanAsync(string beanid, int days);
    Task<IEnumerable<MovementModel>> TopForBeanAsync(string beanid, int count);
    Task<IEnumerable<MovementModel>> HistoryAsync(string beanid, DateTime date);
    Task<IEnumerable<MovementModel>> MostRecentAsync();
    Task<IEnumerable<MovementModel>> MostRecentAsync(string[] beanids);
    Task<MovementModel?> MostRecentAsync(string beanid);
    Task<MovementModel?> ReadForDateAsync(string beanid, DateTime date);
    Task<bool> BeanHasMovementsAsync(string beanid);
    Task<DateTime> LowestDateAsync();
    Task<ApiError> CatchupAsync(string beanid, decimal minValue, DateTime startDate);
    Task<ApiError> MoveAsync(string beanid, decimal minValue, DateTime date);
    Task<decimal> GetMinRangeAsync(string beanid, int days);
    Task<decimal> GetAverageRangeAsync(string beanid, int days);
    Task<decimal> GetMaxRangeAsync(string beanid, int days);
    Task<decimal> GetLargestMovementAsync(string beanid);
    Task<decimal> GetStandardDeviationAsync(string beanid, int days);
}
