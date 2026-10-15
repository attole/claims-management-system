using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class DuplicateSourceSampleTransition : SampleTransition
{
    public DuplicateSourceSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom(new OpenSampleState());
        AllowedFrom(new OpenSampleState());
    }
}
