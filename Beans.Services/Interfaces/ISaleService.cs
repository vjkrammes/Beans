using Beans.Models;

namespace Beans.Services.Interfaces;
public interface ISaleService : IDataService<SaleModel>
{
    Task<IEnumerable<SaleModel>> GetForUserAsync(string userid);
    Task<IEnumerable<SaleModel>> GetForUserAsync(string userid, int days);
    Task<IEnumerable<SaleModel>> GetForUserAndBeanAsync(string userid, string beanid);
    Task<IEnumerable<SaleModel>> GetForUserAndBeanAsync(string userid, string beanid, int days);
    Task<bool> UserHasSalesAsync(string userid);
    Task<bool> UserHasSoldAsync(string userid, string beanid);
    Task<bool> BeanHasSalesAsync(string beanid);
    Task<decimal> ProfitOrLossAsync(string userid);
    Task<decimal> ProfitOrLossAsync(string userid, string beanid);
    Task<decimal> ProfitOrLossAsync(string userid, DateTime startDate, DateTime endDate);
    Task<decimal> ProfitOrLossAsync(string userid, string beanid, DateTime startDate, DateTime endDate);
}
