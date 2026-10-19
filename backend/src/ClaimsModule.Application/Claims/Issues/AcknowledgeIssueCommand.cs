using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Policies;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Issues;

public sealed record AcknowledgeIssueCommand(Guid ClaimId, Guid IssueId) : ICommand<Result<IssueDto>>
{
    public AcknowledgeIssueInput ToInput(PolicyPeriod period, CauseCodes codes, Guid actorId, DateTimeOffset now)
    {
        return new AcknowledgeIssueInput(IssueId, actorId, now, period, codes);
    }
}

public sealed class AcknowledgeIssueCommandValidator : AbstractValidator<AcknowledgeIssueCommand>
{
    public AcknowledgeIssueCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.IssueId).NotEmpty();
    }
}

public sealed class AcknowledgeIssueCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user,
    IMapper mapper) : IRequestHandler<AcknowledgeIssueCommand, Result<IssueDto>>
{
    public async Task<Result<IssueDto>> Handle(AcknowledgeIssueCommand command, CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<IssueDto>();
        }

        var claim = loaded.Value;

        bool hasIssue = claim.Issues.Any(issue => issue.Id == command.IssueId);

        if (!hasIssue)
        {
            return Result<IssueDto>.Failure([ClaimErrors.IssueNotFound(command.IssueId)]);
        }

        var period = await policies.GetPeriodAsync(claim.PolicyId, cancellationToken);
        var codes = await causes.GetActiveAsync(cancellationToken);
        var outcome = claim.AcknowledgeIssue(command.ToInput(period, codes, user.Id, clock.UtcNow));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<IssueDto>(value));
    }
}
