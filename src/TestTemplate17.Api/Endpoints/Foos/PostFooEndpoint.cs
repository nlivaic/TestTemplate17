using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TestTemplate17.Api.Helpers;
using TestTemplate17.Application.Foos.Commands;
using TestTemplate17.Application.Foos.Queries;

namespace TestTemplate17.Api.Endpoints.Foos;

public class PostFooEndpoint : IEndpoint
{
    public void Register(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapGroup(Group.Foos)
            .MapPost(string.Empty, ExecuteAsync)
            .WithName("PostFoo")
            .WithTags(Tags.Foos)
            //.RequireAuthorization()
            .Produces<FooGetModel>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status406NotAcceptable)
            .Produces<ValidationProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Create a new foo.
    /// </summary>
    /// <param name="sender">MediatR sender.</param>
    /// <param name="mapper">Automapper instance</param>
    /// <param name="createFooCommand">Foo create body.</param>
    /// <returns>Newly created foo.</returns>
    public async Task<IResult> ExecuteAsync(
        ISender sender,
        IMapper mapper,
        [FromBody] CreateFooCommand createFooCommand)
    {
        var command = mapper.Map<CreateFooCommand>(createFooCommand);
        var foo = await sender.Send(command);
        var response = mapper.Map<FooGetModel>(foo);
        return TypedResults.CreatedAtRoute(response, "GetFoo", new { id = foo.Id });
    }
}
