using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public sealed class DuplicateSourceSampleTransition : SampleTransition
{
    public DuplicateSourceSampleTransition()
        : base(new ClosedSampleState())
    {
        From(new OpenSampleState());
        From(new OpenSampleState());
    }
}
