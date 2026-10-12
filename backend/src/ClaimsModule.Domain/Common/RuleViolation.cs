namespace ClaimsModule.Domain.Common;

public sealed record RuleViolation(string Code, string Message, string Field) : DomainError(Code, Message)
{
    public RuleViolation(string code, string message)
        : this(code, message, string.Empty)
    {
    }
}
