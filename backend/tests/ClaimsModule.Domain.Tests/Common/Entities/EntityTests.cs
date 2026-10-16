using ClaimsModule.Domain.Tests.Support.Common;

namespace ClaimsModule.Domain.Tests.Common.Entities;

public sealed class EntityTests
{
    [Fact]
    public void Equals_SameIdAndType_IsTrue()
    {
        var id = Guid.NewGuid();
        var first = new SampleEntity(id);
        var second = new SampleEntity(id);

        var result = first.Equals(second);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_SameIdDifferentType_IsFalse()
    {
        var id = Guid.NewGuid();
        var first = new SampleEntity(id);
        var other = new OtherSampleEntity(id);

        var result = first.Equals(other);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentId_IsFalse()
    {
        var first = new SampleEntity(Guid.NewGuid());
        var second = new SampleEntity(Guid.NewGuid());

        var result = first.Equals(second);

        result.Should().BeFalse();
    }

    [Fact]
    public void PopEvents_ReturnsRaisedEventsAndClearsThem()
    {
        var entity = new SampleEntity(Guid.NewGuid());
        var domainEvent = new SampleEvent();
        entity.Add(domainEvent);

        var result = entity.PopEvents();

        result.Should().ContainSingle().Which.Should().Be(domainEvent);
        entity.PopEvents().Should().BeEmpty();
    }
}
