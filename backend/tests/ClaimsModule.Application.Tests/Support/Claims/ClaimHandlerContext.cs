using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.ClaimDocuments;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Jobs;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents;
using ClaimsModule.Application.Results;
using ClaimsModule.Application.Storage;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.ReserveComponents;
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
    private readonly Mock<IClaimReserveReader> _reserves = new();
    private readonly List<ReserveComponent> _addedComponents = [];
    private readonly Mock<IReserveComponentRepository> _components = new();
    private readonly Mock<IPostingScheduler> _scheduler = new();
    private readonly List<ClaimDocument> _addedDocuments = [];
    private readonly Mock<IClaimDocumentRepository> _documents = new();
    private readonly Mock<IStorageService> _storage = new();
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
        _reserves.Setup(mock => mock.GetFactsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimReserveFacts(false, false, false, false, 0m));
        _components.Setup(mock => mock.ListByClaimAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _components.Setup(mock => mock.AddAsync(It.IsAny<ReserveComponent>(), It.IsAny<CancellationToken>()))
            .Callback<ReserveComponent, CancellationToken>((component, _) => _addedComponents.Add(component))
            .Returns(Task.CompletedTask);
        _scheduler.Setup(mock => mock.EnqueueAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _documents.Setup(mock => mock.AddAsync(It.IsAny<ClaimDocument>(), It.IsAny<CancellationToken>()))
            .Callback<ClaimDocument, CancellationToken>((document, _) => _addedDocuments.Add(document))
            .Returns(Task.CompletedTask);
        _storage.Setup(mock => mock.UploadAsync(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
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

    public IClaimReserveReader Reserves => _reserves.Object;

    public IReserveComponentRepository ReserveComponents => _components.Object;

    public IPostingScheduler Scheduler => _scheduler.Object;

    public IClaimDocumentRepository Documents => _documents.Object;

    public IStorageService Storage => _storage.Object;

    public IReadOnlyList<ClaimDocument> AddedDocuments => _addedDocuments;

    public IReadOnlyList<ReserveComponent> AddedComponents => _addedComponents;

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

    public ClaimHandlerContext WithReserveFacts(ClaimReserveFacts facts)
    {
        _reserves.Setup(mock => mock.GetFactsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(facts);

        return this;
    }

    public ClaimHandlerContext WithComponents(params ReserveComponent[] components)
    {
        _components.Setup(mock => mock.ListByClaimAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(components);

        return this;
    }

    public ClaimHandlerContext WithComponent(ReserveComponent component)
    {
        _components.Setup(mock => mock.GetAsync(It.IsAny<Guid>(), component.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReserveComponent>.Success(component));

        foreach (var history in component.History)
        {
            _components.Setup(mock => mock.GetByHistoryAsync(
                    It.IsAny<Guid>(),
                    history.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ReserveComponent>.Success(component));
        }

        return this;
    }

    public ClaimHandlerContext WithoutComponent(Guid componentId)
    {
        _components.Setup(mock => mock.GetAsync(It.IsAny<Guid>(), componentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReserveComponent>.Failure([ReserveErrors.ComponentNotFound(componentId)]));

        return this;
    }

    public ClaimHandlerContext WithoutHistory(Guid reserveHistoryId)
    {
        _components.Setup(mock => mock.GetByHistoryAsync(
                It.IsAny<Guid>(),
                reserveHistoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReserveComponent>.Failure([ReserveErrors.HistoryNotFound(reserveHistoryId)]));

        return this;
    }

    public ClaimHandlerContext WithDocument(ClaimDocument document)
    {
        _documents.Setup(mock => mock.GetAsync(It.IsAny<Guid>(), document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimDocument>.Success(document));

        return this;
    }

    public ClaimHandlerContext WithoutDocument(Guid documentId)
    {
        _documents.Setup(mock => mock.GetAsync(It.IsAny<Guid>(), documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimDocument>.Failure([DocumentErrors.NotFound(documentId)]));

        return this;
    }

    public void VerifyUploaded(Func<string, bool> isPath, Times times)
    {
        _storage.Verify(
            mock => mock.UploadAsync(
                It.Is<string>(path => isPath(path)),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            times);
    }

    public void VerifyAnyUpload(Times times)
    {
        _storage.Verify(
            mock => mock.UploadAsync(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            times);
    }

    public void VerifyEnqueued(Times times)
    {
        _scheduler.Verify(
            mock => mock.EnqueueAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            times);
    }

    public void VerifyEnqueuedWithJobId(string jobId)
    {
        _scheduler.Verify(
            mock => mock.EnqueueAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), jobId, It.IsAny<CancellationToken>()),
            Times.Once());
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
