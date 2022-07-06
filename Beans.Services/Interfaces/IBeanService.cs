using Beans.Common;
using Beans.Models;

namespace Beans.Services.Interfaces;
public interface IBeanService : IDataService<BeanModel>
{
    Task<IEnumerable<string>> BeanIdsAsync();
    Task<IEnumerable<string>> BeanIdsAsync(string userid);
    Task<BeanModel?> ReadForNameAsync(string name);
    Task<long> PlayerHeldAsync(string beanid);
    Task<long> ExchangeHeldAsync(string beanid);
    Task<decimal> CapitalizationAsync();
    Task<decimal> CapitalizationAsync(string beanid);
    Task<BeanHistoryModel?> HistoryAsync(string beanid, int days = int.MaxValue);
    Task<IEnumerable<BeanHistoryModel>> AllBeanHistoryAsync(int days = int.MaxValue);
    Task<ApiError> SellToExchangeAsync(string holdingid, long quantity);
    Task<ApiError> BuyFromExchangeAsync(string userid, string beanid, long quantity);
}
