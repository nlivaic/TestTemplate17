using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TestTemplate17.Api.Helpers;
using TestTemplate17.Application.Foos.Queries;

namespace TestTemplate17.Api.Endpoints.Foos;

public class GetFooByIdEndpoint : IEndpoint
{
    public void Register(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapGroup(Group.Foos)
            .MapGet("{id}", ExecuteAsync)
            .WithName("GetFoo")
            .WithTags(Tags.Foos)
            //.RequireAuthorization()
            .Produces<IEnumerable<FooGetModel>>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status406NotAcceptable)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Get a single foo.
    /// </summary>
    /// <param name="sender">MediatR sender.</param>
    /// <param name="mapper">Automapper instance</param>
    /// <param name="getFooQuery">Specifies which foo to fetch.</param>
    /// <returns>Foo data.</returns>
    public async Task<IResult> ExecuteAsync(
        ISender sender,
        IMapper mapper,
        [AsParameters] GetFooQuery getFooQuery)
    {
        // call handler.
        // map return value
        var foo = await sender.Send(getFooQuery);
        var response = mapper.Map<FooGetModel>(foo);
        return TypedResults.Ok(response);
    }
}
