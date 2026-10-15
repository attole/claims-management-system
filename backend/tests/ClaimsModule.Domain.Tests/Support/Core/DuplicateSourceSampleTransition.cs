using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

internal sealed class DuplicateSourceSampleTransition : SampleTransition<DuplicateSourceSampleTransition>
{
    public DuplicateSourceSampleTransition() : base(new ClosedSampleState())
    {
        AllowedFrom(new OpenSampleState());
        AllowedFrom(new OpenSampleState());
    }
}
