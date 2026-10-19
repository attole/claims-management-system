using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.CauseOfLoss;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Policies;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.Claims.Issues;

public sealed record WaiveIssueCommand(Guid ClaimId, Guid IssueId) : ICommand<Result<IssueDto>>
{
    public WaiveIssueInput ToInput(
        PolicyPeriod period,
        CauseCodes codes,
        UserRole role,
        Guid actorId,
        DateTimeOffset now)
    {
        return new WaiveIssueInput(IssueId, role, actorId, now, period, codes);
    }
}

public sealed class WaiveIssueCommandValidator : AbstractValidator<WaiveIssueCommand>
{
    public WaiveIssueCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.IssueId).NotEmpty();
    }
}

public sealed class WaiveIssueCommandHandler(
    IClaimRepository claims,
    IPolicyReader policies,
    ICauseCodeReader causes,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user,
    IMapper mapper) : IRequestHandler<WaiveIssueCommand, Result<IssueDto>>
{
    public async Task<Result<IssueDto>> Handle(WaiveIssueCommand command, CancellationToken cancellationToken)
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
        var outcome = claim.WaiveIssue(command.ToInput(period, codes, user.Role, user.Id, clock.UtcNow));
        var result = outcome.ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(value => mapper.Map<IssueDto>(value));
    }
}
