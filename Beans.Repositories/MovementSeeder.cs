using Beans.Common;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;

using Microsoft.Extensions.Configuration;

namespace Beans.Repositories;
public class MovementSeeder : SeederBase<MovementEntity, IMovementRepository>, IMovementSeeder
{
    private readonly IBeanRepository _beanRepository;

    public MovementSeeder(IMovementRepository repository, IBeanRepository beanRepository) : base(repository) => _beanRepository = beanRepository;

    public async override Task SeedAsync(IConfiguration configuration, string sectionName)
    {
        if (await _repository.CountAsync() != 0)
        {
            return;
        }
        var beanids = (await _beanRepository.BeanIdsAsync()).ToList();
        if (beanids is null || !beanids.Any())
        {
            return;
        }
        for (var day = -7; day <= 0; day++)
        {
            foreach (var beanid in beanids)
            {
                var bean = await _beanRepository.ReadAsync(beanid);
                if (bean is null)
                {
                    throw new InvalidOperationException($"Bean id '{beanid}' returned but no bean found with that id");
                }
                var result = await _repository.MakeMovementAsync(beanid, Constants.MinimumBeanPrice, DateTime.UtcNow.AddDays(day));
                if (!result.Successful)
                {
                    Console.WriteLine($"Error seeding movements for bean '{bean.Name}':");
                    Console.WriteLine(result.ErrorMessage);
                    Console.WriteLine(Tools.DumpObject(bean));
                }
            }
        }
    }
}
