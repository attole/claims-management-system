using ClaimsModule.API.Errors;
using ClaimsModule.Application.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace ClaimsModule.API.Controllers;

public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    public sealed record CommandResponse<TData>(TData Data, IReadOnlyList<Warning> Warnings);

    protected async Task<IActionResult> QueryAsync<TData>(
        IRequest<Result<TData>> request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ErrorBody.From(result.Errors).ToResult();
    }

    protected async Task<IActionResult> CommandAsync<TData>(
        IRequest<Result<TData>> request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return ErrorBody.From(result.Errors).ToResult();
        }

        var response = new CommandResponse<TData>(result.Value, result.Warnings);

        return new ObjectResult(response) { StatusCode = ReadSuccessStatus() };
    }

    private int ReadSuccessStatus()
    {
        var codes = ControllerContext.ActionDescriptor.EndpointMetadata
            .OfType<IApiResponseMetadataProvider>()
            .Select(metadata => metadata.StatusCode)
            .Where(code => code is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices)
            .Distinct()
            .ToList();

        return codes.Count switch
        {
            0 => StatusCodes.Status200OK,
            1 => codes[0],
            _ => throw new InvalidOperationException("An action must declare at most one success status."),
        };
    }
}
