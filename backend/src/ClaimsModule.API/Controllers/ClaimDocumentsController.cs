using ClaimsModule.API.Controllers.Requests;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.ClaimDocuments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[Route("api/claims/{claimId:guid}/documents")]
public sealed class ClaimDocumentsController(ISender sender) : ApiControllerBase(sender)
{
    private const long MaxUploadBytes = 51L * 1024 * 1024;

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ClaimDocumentLinkDto>>(StatusCodes.Status200OK)]
    public Task<IActionResult> List(Guid claimId, CancellationToken cancellationToken)
    {
        return QueryAsync(new ListClaimDocumentsQuery(claimId), cancellationToken);
    }

    [HttpPost]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    [ProducesResponseType<CommandResponse<ClaimDocumentDto>>(StatusCodes.Status201Created)]
    public Task<IActionResult> Upload(
        Guid claimId,
        [FromForm] UploadDocumentRequest request,
        CancellationToken cancellationToken)
    {
        return CommandAsync(request.ToCommand(claimId), cancellationToken);
    }
}
