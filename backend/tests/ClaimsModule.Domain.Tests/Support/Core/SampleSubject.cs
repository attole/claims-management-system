using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed class SampleSubject : IStateful<SampleState>
{
    public SampleState State { get; set; } = new DraftSampleState();

    public List<string> Log { get; } = [];
}
