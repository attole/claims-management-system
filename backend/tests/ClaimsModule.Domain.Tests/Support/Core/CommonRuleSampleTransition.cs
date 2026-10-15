using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class CommonRuleSampleTransition : SampleTransition<CommonRuleSampleTransition>
{
    public const string CommonDenied = "COMMON_DENIED";

    public CommonRuleSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom(new DraftSampleState());
        AllowedFrom(new OpenSampleState());
        Require(SampleRules.Fail(CommonDenied));
    }
}
