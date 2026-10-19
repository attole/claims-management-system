using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Application.Tests.Support.Claims;

public sealed class ClaimHandlerContext
{
    public static readonly Guid OrganisationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid UserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly DateTimeOffset Now = new(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

    private readonly List<Claim> _addedClaims = [];
    private readonly Mock<IClaimRepository> _claims = new();
    private readonly Mock<IClaimNumberGenerator> _numbers = new();
    private readonly Mock<IPolicyReader> _policies = new();
    private readonly Mock<ICauseCodeReader> _causes = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<ICurrentUser> _user = new();

    public ClaimHandlerContext()
    {
        _causes.Setup(mock => mock.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CauseCodes(["WATER"]));
        _policies.Setup(mock => mock.GetPeriodAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PolicyPeriod.Empty);
        _numbers.Setup(mock => mock.NextAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClaimNumber.From("CLM-2026-0000001"));
        _claims.Setup(mock => mock.AddAsync(It.IsAny<Claim>(), It.IsAny<CancellationToken>()))
            .Callback<Claim, CancellationToken>((claim, _) => _addedClaims.Add(claim))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(mock => mock.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _clock.SetupGet(mock => mock.UtcNow).Returns(Now);
        _user.SetupGet(mock => mock.Id).Returns(UserId);
        _user.SetupGet(mock => mock.OrganisationId).Returns(OrganisationId);
        _user.SetupGet(mock => mock.Role).Returns(new UserManagerRole());
        Mapper = new MapperConfiguration(configuration => configuration.AddMaps(typeof(ClaimMappingProfile).Assembly))
            .CreateMapper();
    }

    public IReadOnlyList<Claim> AddedClaims => _addedClaims;

    public IClaimRepository Claims => _claims.Object;

    public IClaimNumberGenerator Numbers => _numbers.Object;

    public IPolicyReader Policies => _policies.Object;

    public ICauseCodeReader Causes => _causes.Object;

    public IUnitOfWork UnitOfWork => _unitOfWork.Object;

    public IClock Clock => _clock.Object;

    public ICurrentUser User => _user.Object;

    public IMapper Mapper { get; }

    public ClaimHandlerContext WithClaim(Claim claim)
    {
        _claims.Setup(mock => mock.GetAsync(claim.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Claim>.Success(claim));

        return this;
    }

    public ClaimHandlerContext WithoutClaim(Guid claimId)
    {
        _claims.Setup(mock => mock.GetAsync(claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Claim>.Failure([ClaimErrors.ClaimNotFound(claimId)]));

        return this;
    }

    public ClaimHandlerContext WithPolicy(PolicyPeriod period)
    {
        _policies.Setup(mock => mock.ExistsAsync(period.PolicyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _policies.Setup(mock => mock.GetPeriodAsync(period.PolicyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(period);

        return this;
    }

    public ClaimHandlerContext WithoutPolicy(Guid policyId)
    {
        _policies.Setup(mock => mock.ExistsAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        return this;
    }

    public void VerifyCommitted(Times times)
    {
        _unitOfWork.Verify(mock => mock.CommitAsync(It.IsAny<CancellationToken>()), times);
    }

    public void VerifyAdded(Times times)
    {
        _claims.Verify(mock => mock.AddAsync(It.IsAny<Claim>(), It.IsAny<CancellationToken>()), times);
    }
}
