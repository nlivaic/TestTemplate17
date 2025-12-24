using TestTemplate17.Core.Entities;
using TestTemplate17.Core.Interfaces;

namespace TestTemplate17.Data.Repositories;

public class FooRepository : Repository<Foo>, IFooRepository
{
    public FooRepository(TestTemplate17DbContext context)
        : base(context)
    {
    }
}
