using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Application.Claims;

public interface IClaimNumberGenerator
{
    Task<ClaimNumber> NextAsync(int year, CancellationToken cancellationToken);
}
