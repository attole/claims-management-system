using System.Text.Json;
using ClaimsModule.API.Errors;
using ClaimsModule.Application.Idempotency;
using ClaimsModule.Application.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace ClaimsModule.API.Pipeline.Idempotency;

public sealed class IdempotencyFilter(IIdempotencyStore store, IOptions<JsonOptions> json) : IAsyncResourceFilter
{
    public const string HeaderName = "Idempotency-Key";
    public const int MaxKeyLength = 200;

    private static readonly string _keyTooLongMessage =
        $"The {HeaderName} header must be at most {MaxKeyLength} characters.";

    private static readonly string _inProgressMessage =
        $"A request with this {HeaderName} is still running or was interrupted.";

    private static readonly string _mismatchMessage =
        $"This {HeaderName} was already used with a different request body.";

    private static readonly HashSet<string> _writeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete,
    };

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var http = context.HttpContext;
        string key = http.Request.Headers[HeaderName].ToString();

        if (key.Length == 0 || !_writeMethods.Contains(http.Request.Method))
        {
            await next();

            return;
        }

        if (key.Length > MaxKeyLength)
        {
            context.Result = ErrorBody.Create(ErrorKind.Validation, _keyTooLongMessage).ToResult();

            return;
        }

        var request = await DescribeAsync(http.Request, key);
        var decision = await store.BeginAsync(request, http.RequestAborted);

        if (decision.Status != IdempotencyStatus.Started)
        {
            context.Result = Answer(decision);

            return;
        }

        var executed = await next();

        await RememberSuccessAsync(request, executed, http.Response.StatusCode);
    }

    private static async Task<IdempotencyRequest> DescribeAsync(HttpRequest http, string key)
    {
        var bodyHash = await RequestFingerprint.ComputeAsync(http, http.HttpContext.RequestAborted);

        return new IdempotencyRequest(key, http.Method, http.Path.Value ?? string.Empty, bodyHash);
    }

    private static IActionResult Answer(IdempotencyDecision decision)
    {
        return decision.Status switch
        {
            IdempotencyStatus.Completed => new ContentResult
            {
                StatusCode = decision.Stored.StatusCode,
                Content = decision.Stored.Body,
                ContentType = "application/json",
            },
            IdempotencyStatus.InProgress => ErrorBody.Create(ErrorKind.Conflict, _inProgressMessage).ToResult(),
            _ => ErrorBody.Create(ErrorKind.Validation, _mismatchMessage).ToResult(),
        };
    }

    private async Task RememberSuccessAsync(IdempotencyRequest request, ResourceExecutedContext executed, int status)
    {
        bool isSuccess = status is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices;
        bool isUnhandledFailure = executed.Exception is not null && !executed.ExceptionHandled;

        if (isSuccess && !isUnhandledFailure)
        {
            var response = new IdempotencyResponse(status, ReadBody(executed.Result));

            await store.CompleteAsync(request, response, CancellationToken.None);
        }
    }

    private string ReadBody(IActionResult? result)
    {
        return result switch
        {
            ObjectResult { Value: not null } objectResult =>
                JsonSerializer.Serialize(objectResult.Value, json.Value.JsonSerializerOptions),
            ContentResult content => content.Content ?? string.Empty,
            _ => string.Empty,
        };
    }
}
