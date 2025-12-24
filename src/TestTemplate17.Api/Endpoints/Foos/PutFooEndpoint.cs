using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TestTemplate17.Api.Helpers;
using TestTemplate17.Application.Foos.Commands;

namespace TestTemplate17.Api.Endpoints.Foos;

public class PutFooEndpoint : IEndpoint
{
    public void Register(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapGroup(Group.Foos)
            .MapPut("{id}", ExecuteAsync)
            .WithName("PutFoo")
            .WithTags(Tags.Foos)
            //.RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Edit foo.
    /// </summary>
    /// <param name="sender">MediatR sender.</param>
    /// <param name="mapper">Automapper instance</param>
    /// <param name="updateFooCommand">Foo edit data.</param>
    public async Task<IResult> ExecuteAsync(
        ISender sender,
        IMapper mapper,
        [FromBody] UpdateFooCommand updateFooCommand)
    {
        // call handler.
        // map return value
        var command = mapper.Map<UpdateFooCommand>(updateFooCommand);
        await sender.Send(command);
        return TypedResults.NoContent();
    }
}
