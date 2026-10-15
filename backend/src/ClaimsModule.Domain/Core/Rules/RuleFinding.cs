namespace ClaimsModule.Domain.Core.Rules;

public sealed record RuleFinding(string Code, string Message, string Field) : DomainFinding(Code, Message)
{
    public RuleFinding(string code, string message) : this(code, message, string.Empty)
    {
    }
}
