using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Core.Transitions;

public sealed record TransitionFinding(string From, string To)
    : DomainFinding(DefaultCode, $"Transition from {From} to {To} is not permitted.", string.Empty)
{
    public const string DefaultCode = "TRANSITION_NOT_PERMITTED";

    public IReadOnlyList<string> ValidTargets { get; init; } = [];
}
