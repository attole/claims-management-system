using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SharedRuleSampleTransition : SampleTransition
{
    public const string SharedDenied = "SHARED_DENIED";

    public SharedRuleSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom(new DraftSampleState(), new OpenSampleState())
            .Require(SampleRules.Fail(SharedDenied));
    }
}
