using ClaimsModule.API.Controllers.Requests;
using ClaimsModule.Application.ReserveComponents.Commands;
using ClaimsModule.Application.ReserveComponents.Dtos;
using ClaimsModule.Application.ReserveComponents.History.Commands;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.ReserveComponents.Queries;
using ClaimsModule.Application.ReserveComponents.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/reserves")]
public sealed class ClaimReservesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType<ClaimReservesDto>(StatusCodes.Status200OK)]
    public Task<IActionResult> Get(Guid claimId, CancellationToken cancellationToken)
    {
        return QueryAsync(new GetClaimReservesQuery(claimId), cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType<CommandResponse<ReserveHistoryDto>>(StatusCodes.Status201Created)]
    public Task<IActionResult> Submit(
        Guid claimId,
        [FromBody] ReserveRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(new SubmitReserveCommand(claimId, request), cancellationToken);
    }

    [HttpPost("{reserveHistoryId:guid}/approve")]
    [ProducesResponseType<CommandResponse<ReserveHistoryStatusDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Approve(Guid claimId, Guid reserveHistoryId, CancellationToken cancellationToken)
    {
        return CommandAsync(new ApproveReserveCommand(claimId, reserveHistoryId), cancellationToken);
    }

    [HttpPost("{reserveHistoryId:guid}/reject")]
    [ProducesResponseType<CommandResponse<ReserveHistoryStatusDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Reject(
        Guid claimId,
        Guid reserveHistoryId,
        [FromBody] RejectReserveRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(request.ToCommand(claimId, reserveHistoryId), cancellationToken);
    }

    [HttpPost("{reserveHistoryId:guid}/retract")]
    [ProducesResponseType<CommandResponse<ReserveHistoryStatusDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> Retract(Guid claimId, Guid reserveHistoryId, CancellationToken cancellationToken)
    {
        return CommandAsync(new RetractReserveCommand(claimId, reserveHistoryId), cancellationToken);
    }

    [HttpPost("{reserveHistoryId:guid}/retry-posting")]
    [ProducesResponseType<CommandResponse<ReserveHistoryStatusDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> RetryPosting(Guid claimId, Guid reserveHistoryId, CancellationToken cancellationToken)
    {
        return CommandAsync(new RetryReservePostingCommand(claimId, reserveHistoryId), cancellationToken);
    }
}
