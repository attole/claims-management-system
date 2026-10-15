using ClaimsModule.Domain.Core.Base.Flows;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class ContinuingSampleFlow : SampleFlow<ContinuingSampleFlow>
{
    public const string Blocked = "BLOCK";

    public ContinuingSampleFlow()
    {
        Require(SampleRules.Fail(Blocked));
    }

    protected override FlowDecision WhenBlocked => FlowDecision.Continue;
}
