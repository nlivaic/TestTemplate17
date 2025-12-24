using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace TestTemplate17.Api.Helpers;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpointRouteBuilder, Assembly assembly = null)
    {
        assembly = assembly ?? Assembly.GetExecutingAssembly();
        var endpointTypes = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                type.IsAssignableTo(typeof(IEndpoint)))
            .ToList();
        foreach (var endpointType in endpointTypes)
        {
            var endpointInstance = Activator.CreateInstance(endpointType) as IEndpoint;
            endpointInstance?.Register(endpointRouteBuilder);
        }
        return endpointRouteBuilder;
    }
}
