using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Domain.Tests.Support.Common;

public sealed class SampleEntity(Guid id) : Entity(id)
{
    public void Add(IDomainEvent domainEvent)
    {
        Raise(domainEvent);
    }
}
