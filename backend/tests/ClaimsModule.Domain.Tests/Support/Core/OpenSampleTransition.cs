using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class OpenSampleTransition : SampleTransition<OpenSampleTransition>
{
    public const string ReopenDenied = "REOPEN_DENIED";

    public OpenSampleTransition() : base(new OpenSampleState())
    {
        AllowedFrom(new DraftSampleState());
        AllowedFrom(new ClosedSampleState())
            .Require(SampleRules.Fail(ReopenDenied));
    }
}
