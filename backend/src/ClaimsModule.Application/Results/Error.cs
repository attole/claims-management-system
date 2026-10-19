namespace ClaimsModule.Application.Results;

public sealed record Error(string Code, string Message, ErrorKind Kind, string Field = "")
{
    public static Error Validation(string code, string message, string field = "")
    {
        return new Error(code, message, ErrorKind.Validation, field);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorKind.NotFound);
    }

    public static Error Forbidden(string code, string message)
    {
        return new Error(code, message, ErrorKind.Forbidden);
    }

    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorKind.Conflict);
    }
}
