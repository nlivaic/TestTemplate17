using System.Threading.Tasks;
using TestTemplate17.Common.Interfaces;

namespace TestTemplate17.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly TestTemplate17DbContext _dbContext;

    public UnitOfWork(TestTemplate17DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveAsync()
    {
        if (_dbContext.ChangeTracker.HasChanges())
        {
            return await _dbContext.SaveChangesAsync();
        }
        return 0;
    }
}