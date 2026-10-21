using ClaimsModule.API.Controllers.Requests;
using ClaimsModule.Application.Audit.Dtos;
using ClaimsModule.Application.Claims.Dtos;
using ClaimsModule.Application.Claims.Loss;
using ClaimsModule.Application.Claims.Queries;
using ClaimsModule.Application.Claims.StateChanges;
using ClaimsModule.Application.Paging;
using ClaimsModule.Domain.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims")]
public sealed class ClaimsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType<PagedResult<ClaimSummaryDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> List([FromQuery] ListClaimsRequest request, CancellationToken cancellationToken)
    {
        return QueryAsync(request.ToQuery(), cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType<CommandResponse<ClaimDto>>(StatusCodes.Status201Created)]
    public Task<IActionResult> Create([FromBody] CreateClaimRequest request, CancellationToken cancellationToken)
    {
        return CommandAsync(request.ToCommand(), cancellationToken);
    }

    [HttpGet("{claimId:guid}")]
    [ProducesResponseType<ClaimDetailDto>(StatusCodes.Status200OK)]
    public Task<IActionResult> Get(Guid claimId, CancellationToken cancellationToken)
    {
        return QueryAsync(new GetClaimQuery(claimId), cancellationToken);
    }

    [HttpPut("{claimId:guid}/status")]
    [ProducesResponseType<CommandResponse<ClaimStatusDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> ChangeStatus(
        Guid claimId,
        [FromBody] ChangeClaimStatusRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(request.ToCommand(claimId), cancellationToken);
    }

    [HttpGet("{claimId:guid}/status-check")]
    [ProducesResponseType<ClaimStatusCheckDto>(StatusCodes.Status200OK)]
    public Task<IActionResult> CheckStatus(
        Guid claimId,
        [FromQuery] ClaimStateKind targetStatus,
        CancellationToken cancellationToken)
    {
        return QueryAsync(new CheckClaimStatusQuery(claimId, targetStatus), cancellationToken);
    }

    [HttpPut("{claimId:guid}/handler")]
    [ProducesResponseType<CommandResponse<ClaimDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> AssignHandler(
        Guid claimId,
        [FromBody] AssignHandlerRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(request.ToCommand(claimId), cancellationToken);
    }

    [HttpPut("{claimId:guid}/policy")]
    [ProducesResponseType<CommandResponse<ClaimDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> LinkPolicy(
        Guid claimId,
        [FromBody] LinkPolicyRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(request.ToCommand(claimId), cancellationToken);
    }

    [HttpPut("{claimId:guid}/loss")]
    [ProducesResponseType<CommandResponse<LossDetailsDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateLoss(
        Guid claimId,
        [FromBody] LossRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(new UpdateLossDetailsCommand(claimId, request), cancellationToken);
    }

    [HttpPut("{claimId:guid}/notes")]
    [ProducesResponseType<CommandResponse<ClaimDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> UpdateNotes(
        Guid claimId,
        [FromBody] UpdateNotesRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(request.ToCommand(claimId), cancellationToken);
    }

    [HttpGet("{claimId:guid}/audit")]
    [ProducesResponseType<PagedResult<AuditEntryDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> ListAudit(
        Guid claimId,
        [FromQuery] ListClaimAuditRequest request,
        CancellationToken cancellationToken)
    {
        return QueryAsync(request.ToQuery(claimId), cancellationToken);
    }
}
