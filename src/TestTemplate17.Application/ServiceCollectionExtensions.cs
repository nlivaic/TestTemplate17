using Microsoft.Extensions.DependencyInjection;
using TestTemplate17.Application.Pipelines;

namespace TestTemplate17.Application;

public static class ServiceCollectionExtensions
{
    public static void AddTestTemplate17ApplicationHandlers(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining(typeof(ServiceCollectionExtensions)));
        services.AddPipelines();
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
    }
}
