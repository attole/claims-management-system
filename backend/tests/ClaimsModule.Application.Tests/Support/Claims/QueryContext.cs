using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Audit.Contracts;
using ClaimsModule.Application.Audit.Dtos;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.Claims;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Paging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;

namespace ClaimsModule.Application.Tests.Support.Claims;

public sealed class QueryContext
{
    private readonly Mock<IClaimReader> _claims = new();
    private readonly Mock<IClaimReserveReader> _reserves = new();
    private readonly Mock<IClaimDocumentReader> _documents = new();
    private readonly Mock<IAuditLogReader> _audit = new();
    private readonly Mock<IPolicyReader> _policies = new();
    private readonly Mock<ICauseCodeReader> _causes = new();
    private readonly Mock<IStorageService> _storage = new();

    public QueryContext()
    {
        _storage.Setup(mock => mock.GetDownloadUrlAsync(
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string path, TimeSpan _, CancellationToken _) => $"https://files/{path}");
        _documents.Setup(mock => mock.ListAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _audit.Setup(mock => mock.ListAsync(It.IsAny<Guid>(), It.IsAny<PageWindow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<AuditEntryDto>([], 1, 10, 0));
        _reserves.Setup(mock => mock.GetReservesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimReservesDto());
    }

    public IClaimReader Claims => _claims.Object;

    public IClaimReserveReader Reserves => _reserves.Object;

    public IClaimDocumentReader Documents => _documents.Object;

    public IAuditLogReader Audit => _audit.Object;

    public IPolicyReader Policies => _policies.Object;

    public ICauseCodeReader Causes => _causes.Object;

    public IStorageService Storage => _storage.Object;

    public QueryContext WithKnownClaim(Guid claimId)
    {
        _claims.Setup(mock => mock.ExistsAsync(claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        return this;
    }

    public QueryContext WithUnknownClaim(Guid claimId)
    {
        _claims.Setup(mock => mock.ExistsAsync(claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _claims.Setup(mock => mock.GetDetailAsync(claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimDetailDto>.Failure([ClaimErrors.ClaimNotFound(claimId)]));

        return this;
    }

    public QueryContext WithDetail(Guid claimId, ClaimDetailDto detail)
    {
        _claims.Setup(mock => mock.GetDetailAsync(claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ClaimDetailDto>.Success(detail));

        return this;
    }

    public QueryContext WithClaimPage(PagedResult<ClaimSummaryDto> page)
    {
        _claims.Setup(mock => mock.ListAsync(
                It.IsAny<ClaimListFilter>(),
                It.IsAny<PageWindow>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        return this;
    }

    public QueryContext WithReserves(ClaimReservesDto reserves)
    {
        _reserves.Setup(mock => mock.GetReservesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reserves);

        return this;
    }

    public QueryContext WithDocuments(params ClaimDocumentFile[] files)
    {
        _documents.Setup(mock => mock.ListAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        return this;
    }

    public QueryContext WithAuditPage(PagedResult<AuditEntryDto> page)
    {
        _audit.Setup(mock => mock.ListAsync(It.IsAny<Guid>(), It.IsAny<PageWindow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        return this;
    }

    public QueryContext WithPolicyPage(PagedResult<PolicyDto> page)
    {
        _policies.Setup(mock => mock.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<PageWindow>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        return this;
    }

    public QueryContext WithCauseCodes(params CauseCodeDto[] codes)
    {
        _causes.Setup(mock => mock.ListActiveAsync(
                It.IsAny<IReadOnlyList<PerilCategory>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(codes);

        return this;
    }

    public void VerifyClaimListed(Func<ClaimListFilter, bool> matches, PageWindow page)
    {
        _claims.Verify(
            mock => mock.ListAsync(
                It.Is<ClaimListFilter>(filter => matches(filter)),
                page,
                It.IsAny<CancellationToken>()),
            Times.Once());
    }

    public void VerifyPoliciesSearched(string text, PageWindow page)
    {
        _policies.Verify(
            mock => mock.SearchAsync(text, page, It.IsAny<CancellationToken>()),
            Times.Once());
    }

    public void VerifyCausesListed(params PerilCategory[] perils)
    {
        _causes.Verify(
            mock => mock.ListActiveAsync(
                It.Is<IReadOnlyList<PerilCategory>>(listed => listed.SequenceEqual(perils)),
                It.IsAny<CancellationToken>()),
            Times.Once());
    }

    public void VerifyAuditListed(Guid claimId, PageWindow page)
    {
        _audit.Verify(mock => mock.ListAsync(claimId, page, It.IsAny<CancellationToken>()), Times.Once());
    }

    public void VerifyNoDocumentRead()
    {
        _documents.Verify(
            mock => mock.ListAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    public void VerifyDownloadUrlRequested(string path, TimeSpan lifetime)
    {
        _storage.Verify(
            mock => mock.GetDownloadUrlAsync(path, lifetime, It.IsAny<CancellationToken>()),
            Times.Once());
    }
}
