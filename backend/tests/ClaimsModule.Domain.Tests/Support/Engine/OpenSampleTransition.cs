using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public sealed class OpenSampleTransition : SampleTransition
{
    public const string ReopenDenied = "REOPEN_DENIED";

    public OpenSampleTransition()
        : base(new OpenSampleState())
    {
        From(new DraftSampleState());
        From(new ClosedSampleState())
            .Require(SampleRules.Fail(ReopenDenied));
    }
}
