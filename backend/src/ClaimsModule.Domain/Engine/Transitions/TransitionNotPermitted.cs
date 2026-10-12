using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Engine.Transitions;

public sealed record TransitionNotPermitted(
    string Code,
    string Message,
    string From,
    string To,
    IReadOnlyList<string> ValidTargets) : DomainError(Code, Message)
{
    public const string DefaultCode = "TRANSITION_NOT_PERMITTED";

    public static TransitionNotPermitted Create(string code, string from, string to)
    {
        return new TransitionNotPermitted(code, $"Cannot move from {from} to {to}.", from, to, []);
    }
}
