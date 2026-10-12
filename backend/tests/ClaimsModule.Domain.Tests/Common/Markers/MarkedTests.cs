using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Common.Markers;

public sealed class MarkedTests
{
    [Fact]
    public void Name_IsTheMarkerName()
    {
        new OpenSampleState().Name.Should().Be("Open");
    }

    [Fact]
    public void Records_OfTheSameMarkerAreEqual()
    {
        new OpenSampleState().Should().Be(new OpenSampleState());
    }

    [Fact]
    public void Records_OfDifferentMarkersAreNotEqual()
    {
        SampleState open = new OpenSampleState();

        open.Should().NotBe(new ClosedSampleState());
    }
}
