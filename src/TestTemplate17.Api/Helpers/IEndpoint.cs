using Microsoft.AspNetCore.Routing;

namespace TestTemplate17.Api.Helpers;

public interface IEndpoint
{
    public void Register(IEndpointRouteBuilder endpointRouteBuilder);
}
