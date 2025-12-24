using Microsoft.Extensions.DependencyInjection;
using TestTemplate17.Common.Interfaces;
using TestTemplate17.Core.Interfaces;
using TestTemplate17.Data.Repositories;

namespace TestTemplate17.Data;

public static class ServiceCollectionExtensions
{
    public static void AddSpecificRepositories(this IServiceCollection services) =>
        services.AddScoped<IFooRepository, FooRepository>();

    public static void AddGenericRepository(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    }
}
