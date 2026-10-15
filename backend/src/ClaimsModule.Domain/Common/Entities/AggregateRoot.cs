namespace ClaimsModule.Domain.Common.Entities;

public abstract class AggregateRoot : Entity
{
    protected AggregateRoot()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }

    public byte[] RowVer { get; protected set; } = [];
}
