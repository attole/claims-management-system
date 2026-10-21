using ClaimsModule.API.Errors;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsModule.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/documents/download")]
public sealed class DocumentDownloadController(ILocalDocumentDownloads downloads) : ControllerBase
{
    private const string InvalidLinkMessage = "The download link is invalid or has expired.";

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Download([FromQuery] string token)
    {
        var download = downloads.Open(token ?? string.Empty);

        if (!download.IsFound)
        {
            return ErrorBody.Create(ErrorKind.NotFound, InvalidLinkMessage).ToResult();
        }

        return File(download.Content, download.ContentType, download.FileName);
    }
}
