namespace ClaimsModule.Domain.Common.Capabilities;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }

    DateTimeOffset UpdatedAt { get; }

    Guid UserCreated { get; }

    Guid UserModified { get; }
}
