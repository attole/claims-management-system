namespace ClaimsModule.Domain.Common.Capabilities;

public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTimeOffset DeletedAt { get; }
}
