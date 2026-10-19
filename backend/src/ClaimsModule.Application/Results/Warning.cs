namespace ClaimsModule.Application.Results;

public sealed record Warning(string Code, string Message, string Field = "");
