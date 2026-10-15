using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class NoSourcesSampleTransition : SampleTransition
{
    public NoSourcesSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom();
    }
}
