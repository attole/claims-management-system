namespace ClaimsModule.Domain.Common.Capabilities;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
}
