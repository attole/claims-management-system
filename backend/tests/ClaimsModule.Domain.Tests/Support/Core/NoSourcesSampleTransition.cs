using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class NoSourcesSampleTransition : SampleTransition<NoSourcesSampleTransition>
{
    public NoSourcesSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom();
    }
}
