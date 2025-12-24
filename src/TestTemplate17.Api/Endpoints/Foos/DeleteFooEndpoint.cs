using System;
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

public class DeleteFooEndpoint : IEndpoint
{
    public void Register(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapGroup(Group.Foos)
            .MapDelete("{id}", ExecuteAsync)
            .WithName("DeleteFoo")
            .WithTags(Tags.Foos)
            .WithOpenApi()
            //.RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Delete foo.
    /// </summary>
    /// <param name="sender">MediatR sender.</param>
    /// <param name="mapper">Automapper instance</param>
    /// <param name="id">Foo identifier.</param>
    public async Task<IResult> ExecuteAsync(
        ISender sender,
        IMapper mapper,
        [FromRoute] Guid id)
    {
        var deleteQuestionCommand = new DeleteFooCommand
        {
            Id = id
        };
        await sender.Send(deleteQuestionCommand);
        return TypedResults.NoContent();
    }
}
