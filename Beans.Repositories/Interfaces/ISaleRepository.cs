
using Beans.Common;
using Beans.Repositories.Entities;

using System.Data.Common;

namespace Beans.Repositories.Interfaces;
public interface ISaleRepository : IRepository<SaleEntity>
{
    Task<IEnumerable<SaleEntity>> GetForUserAsync(int userid);
    Task<IEnumerable<SaleEntity>> GetForUserAsync(int userid, int days);
    Task<IEnumerable<SaleEntity>> GetForUserAndBeanAsync(int userid, int beanid);
    Task<IEnumerable<SaleEntity>> GetForUserAndBeanAsync(int userid, int beanid, int days);
    Task<bool> UserHasSalesAsync(int userid);
    Task<bool> UserHasSoldAsync(int userid, int beanid);
    Task<bool> BeanHasSalesAsync(int beanid);
    Task<decimal> ProfitOrLossAsync(int userid);
    Task<decimal> ProfitOrLossAsync(int userid, int beanid);
    Task<decimal> ProfitOrLossAsync(int userid, DateTime startDate, DateTime endDate);
    Task<decimal> ProfitOrLossAsync(int userid, int beanid, DateTime startDate, DateTime endDate);
}
