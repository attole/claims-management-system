namespace ClaimsModule.Domain.Common.Capabilities;

public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset UpdatedAt { get; set; }

    Guid UserCreated { get; set; }

    Guid UserModified { get; set; }
}
