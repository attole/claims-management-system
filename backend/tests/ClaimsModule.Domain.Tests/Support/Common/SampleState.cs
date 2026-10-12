using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Tests.Support.Common;

public abstract record SampleState(SampleStatus Marker) : Marked<SampleStatus>(Marker);
