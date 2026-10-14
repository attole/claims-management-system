using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Tests.Support.Common;

public abstract record SampleState(SampleStateKind Marker) : Marked<SampleStateKind>(Marker);
