using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Results;
using Microsoft.AspNetCore.Diagnostics;

namespace ClaimsModule.API.Errors;

internal sealed class ConcurrencyConflictExceptionHandler : IExceptionHandler
{
    private const string Message = "The data was changed by another request. Reload it and try again.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ConcurrencyConflictException)
        {
            return false;
        }

        await ErrorBody.Create(ErrorKind.Conflict, Message).WriteAsync(httpContext, cancellationToken);

        return true;
    }
}
