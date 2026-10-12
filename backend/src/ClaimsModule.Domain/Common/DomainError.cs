namespace ClaimsModule.Domain.Common;

public abstract record DomainError(string Code, string Message);
