using ClaimsModule.Application.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ClaimsModule.API.Errors;

public sealed record ErrorBody(string Type, string Title, int Status, IReadOnlyDictionary<string, string[]> Errors)
{
    public const string GeneralKey = "General";

    public static ErrorBody From(IReadOnlyList<Error> errors)
    {
        var messages = errors.Select(error => (Key: KeyFor(error.Field), error.Message));

        return Build(Describe(errors[0].Kind), messages);
    }

    public static ErrorBody Create(ErrorKind kind, string message)
    {
        return Build(Describe(kind), [(GeneralKey, message)]);
    }

    public static ErrorBody Unauthorized()
    {
        var description = new ErrorDescription(
            StatusCodes.Status401Unauthorized,
            "UnauthorizedError",
            "Authentication is required.");

        return Build(description, [(GeneralKey, "A valid bearer token is required.")]);
    }

    public static ErrorBody FromModelState(ModelStateDictionary modelState)
    {
        var messages = modelState.SelectMany(
            entry => entry.Value?.Errors ?? [],
            (entry, error) => (Key: KeyFor(entry.Key), error.ErrorMessage));

        return Build(Describe(ErrorKind.Validation), messages);
    }

    public ObjectResult ToResult()
    {
        return new ObjectResult(this) { StatusCode = Status };
    }

    public Task WriteAsync(HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = Status;

        return context.Response.WriteAsJsonAsync(this, cancellationToken);
    }

    private static string KeyFor(string field)
    {
        string key = field.TrimStart('$', '.');

        return key.Length == 0 ? GeneralKey : key;
    }

    private static ErrorBody Build(ErrorDescription description, IEnumerable<(string Key, string Message)> messages)
    {
        var errors = messages
            .GroupBy(item => item.Key)
            .ToDictionary(group => group.Key, group => group.Select(item => item.Message).ToArray());

        return new ErrorBody(description.Type, description.Title, description.Status, errors);
    }

    private static ErrorDescription Describe(ErrorKind kind)
    {
        return kind switch
        {
            ErrorKind.NotFound => new ErrorDescription(
                StatusCodes.Status404NotFound,
                "NotFoundError",
                "The requested resource was not found."),
            ErrorKind.Forbidden => new ErrorDescription(
                StatusCodes.Status403Forbidden,
                "ForbiddenError",
                "You do not have permission to perform this action."),
            ErrorKind.Conflict => new ErrorDescription(
                StatusCodes.Status409Conflict,
                "ConflictError",
                "The request conflicts with the current state of the resource."),
            _ => new ErrorDescription(
                StatusCodes.Status422UnprocessableEntity,
                "ValidationError",
                "One or more validation errors occurred."),
        };
    }

    private sealed record ErrorDescription(int Status, string Type, string Title);
}
