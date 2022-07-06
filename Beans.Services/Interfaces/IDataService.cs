using Beans.Common;
using Beans.Models;

namespace Beans.Services.Interfaces;

public interface IDataService<TModel> where TModel : ModelBase
{
    Task<int> CountAsync();
    Task<ApiError> InsertAsync(TModel model);
    Task<ApiError> UpdateAsync(TModel model);
    Task<ApiError> DeleteAsync(TModel model);
    Task<IEnumerable<TModel>> GetAsync();
    Task<TModel?> ReadAsync(string id);
}
