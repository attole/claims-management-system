using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Transitions;

public sealed record TransitionFinding(
    string Code,
    string Message,
    string From,
    string To,
    IReadOnlyList<string> ValidTargets) : DomainFinding(Code, Message)
{
    public const string DefaultCode = "TRANSITION_NOT_PERMITTED";

    public static TransitionFinding Create(string from, string to)
    {
        return new TransitionFinding(DefaultCode, $"Cannot move from {from} to {to}.", from, to, []);
    }
}
