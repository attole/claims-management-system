using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Common.Entities;

public sealed class EntityTests
{
    private static readonly DateTimeOffset _now = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Equals_SameIdAndType_IsTrue()
    {
        Guid id = Guid.NewGuid();

        new SampleEntity(id).Equals(new SampleEntity(id)).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameIdDifferentType_IsFalse()
    {
        Guid id = Guid.NewGuid();

        new SampleEntity(id).Equals(new OtherSampleEntity(id)).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentId_IsFalse()
    {
        new SampleEntity(Guid.NewGuid()).Equals(new SampleEntity(Guid.NewGuid())).Should().BeFalse();
    }

    [Fact]
    public void PopEvents_ReturnsRaisedEventsAndClearsThem()
    {
        SampleEntity entity = new(Guid.NewGuid());
        SampleEvent domainEvent = new(Guid.NewGuid(), _now, Guid.Empty);
        entity.Add(domainEvent);

        entity.PopEvents().Should().ContainSingle().Which.Should().Be(domainEvent);
        entity.PopEvents().Should().BeEmpty();
    }
}
