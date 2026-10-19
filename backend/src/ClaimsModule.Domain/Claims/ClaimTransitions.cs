using System.Reflection;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims;

public static class ClaimTransitions
{
    private const string InstanceName = "Instance";
    private const BindingFlags InstanceFlags =
        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    private static readonly IReadOnlyList<ITransition<ClaimState>> _transitions = FindTransitions();

    public static IReadOnlyList<ClaimState> NextFrom(ClaimState state)
    {
        var targets = _transitions
            .Where(transition => transition.Sources.Contains(state))
            .Select(transition => transition.Target)
            .OrderBy(target => target.Marker)
            .ToList();

        return targets;
    }

    private static List<ITransition<ClaimState>> FindTransitions()
    {
        var transitions = typeof(ClaimTransitions).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract)
            .Where(type => typeof(ITransition<ClaimState>).IsAssignableFrom(type))
            .Select(type => type.GetProperty(InstanceName, InstanceFlags)!.GetValue(null))
            .Cast<ITransition<ClaimState>>()
            .ToList();

        return transitions;
    }
}
