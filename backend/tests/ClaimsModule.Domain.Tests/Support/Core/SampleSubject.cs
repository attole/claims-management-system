using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SampleSubject
{
    public SampleState State { get; set; } = new DraftSampleState();

    public List<string> Log { get; } = [];
}
