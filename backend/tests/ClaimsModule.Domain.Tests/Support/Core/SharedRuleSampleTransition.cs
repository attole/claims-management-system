using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class SharedRuleSampleTransition : SampleTransition<SharedRuleSampleTransition>
{
    public const string SharedDenied = "SHARED_DENIED";

    public SharedRuleSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom(new DraftSampleState(), new OpenSampleState())
            .Require(SampleRules.Critical(SharedDenied));
    }
}
