using Beans.Common;
using Beans.Models;

namespace Beans.Services.Interfaces;
public interface IHoldingService : IDataService<HoldingModel>
{
    Task<IEnumerable<HoldingModel>> GetForUserAsync(string userid);
    Task<IEnumerable<HoldingModel>> GetForBeanAsync(string beanid);
    Task<IEnumerable<HoldingModel>> GetForBeanAsync(string userid, string beanid);
    Task<IEnumerable<HoldingModel>> SearchAsync(SearchHoldingsModel model);
    Task<long> BeansHeldByUserAsync(string userid);
    Task<long> BeansHeldByUserAndBeanAsync(string userid, string beanid);
    Task<long> BeansHeldByBeanAsync(string beanid);
    Task<CostBasis> GetCostBasisAsync(string userid);
    Task<CostBasisModel[]> GetCostBasesAsync(string userid);
    Task<CostBasis> GetCostBasisAsync(string userid, string beanid);
    Task<HoldingSummaryModel> SummaryAsync(string userid, string beanid);
    Task<Dictionary<string, long>> BeansHeldAsync();
    Task<bool> UserHasHoldingsAsync(string userid);
    Task<bool> UserHasHoldingsAsync(string userid, string beanid);
    Task<bool> BeanHasHoldingsAsync(string beanid);
    Task<string[]> GetHoldingsAsync(bool oldestFirst, string userid, string beanid, long quantity);
    Task<long> HoldingCountAsync(string userid, string? beanid);
    Task<decimal> TotalValueAsync(string userid);
    Task<decimal> TotalValueAsync(string userid, string beanid);
    Task<decimal> TotalCostAsync(string userid);
    Task<decimal> TotalCostAsync(string userid, string beanid);
    Task<ApiError> ResetHoldingsAsync();
}
