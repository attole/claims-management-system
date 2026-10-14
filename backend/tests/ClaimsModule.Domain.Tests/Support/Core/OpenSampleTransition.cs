using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class OpenSampleTransition : SampleTransition
{
    public const string ReopenDenied = "REOPEN_DENIED";

    public OpenSampleTransition()
        : base(new OpenSampleState())
    {
        AllowedFrom(new DraftSampleState());
        AllowedFrom(new ClosedSampleState())
            .Require(SampleRules.Fail(ReopenDenied));
    }
}
