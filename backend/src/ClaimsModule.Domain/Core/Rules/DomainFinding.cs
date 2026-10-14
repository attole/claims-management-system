using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Core.Rules;

public abstract record DomainFinding(string Code, string Message) : IFinding
{
    public bool IsBlocking { get; init; } = true;

    public DomainFinding AsWarning()
    {
        return this with { IsBlocking = false };
    }
}
