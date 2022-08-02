namespace RoadRegistry.BackOffice.Api.Extracts;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Downloads;
using Abstractions.Extracts;
using Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Framework;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UploadExtractArchiveRequest = Abstractions.Extracts.UploadExtractArchiveRequest;

[ApiVersion("2.0")]
[AdvertiseApiVersions("2.0")]
[ApiRoute("extracts")]
[ApiExplorerSettings(GroupName = "Extracts")]
public class ExtractsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExtractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("downloadrequests")]
    public async Task<IActionResult> PostDownloadRequest([FromBody] DownloadExtractRequestBody body, CancellationToken cancellationToken)
    {
        var request = new DownloadExtractRequest(body.RequestId, body.Contour);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(response);
    }

    [HttpPost("downloadrequests/bycontour")]
    public async Task<ActionResult> PostDownloadRequestByContour([FromBody] DownloadExtractByContourRequestBody body, CancellationToken cancellationToken)
    {
        DownloadExtractByContourRequest request = new(body.Contour, body.Buffer, body.Description);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(response);
    }

    [HttpPost("downloadrequests/byniscode")]
    public async Task<ActionResult> PostDownloadRequestByNisCode([FromBody] DownloadExtractByNisCodeRequestBody body, CancellationToken cancellationToken)
    {
        DownloadExtractByNisCodeRequest request = new(body.NisCode, body.Buffer, body.Description);
        try
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(response);
        }
        catch (DownloadExtractByNisCodeNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("download/{downloadId}")]
    public async Task<ActionResult> GetDownload([FromRoute] string downloadId, CancellationToken cancellationToken)
    {
        DownloadEditorRequest request = new();
        var response = await _mediator.Send(request, cancellationToken);
        return new FileCallbackResult(response);
    }

    [HttpPost("download/{downloadId}/uploads")]
    public async Task<ActionResult> PostUpload(
        [FromRoute] string downloadId,
        [FromBody] IFormFile archive,
        [FromBody] bool featureCompare,
        CancellationToken cancellationToken)
    {
        MemoryStream ms = new();
        await archive.CopyToAsync(ms, cancellationToken);

        try
        {
            Abstractions.Extracts.UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            Abstractions.Extracts.UploadExtractRequest request = new(downloadId, requestArchive);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(response);
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("upload/{uploadId}/status")]
    public async Task<IActionResult> GetUploadStatus(
        [FromRoute] string uploadId,
        CancellationToken cancellationToken)
    {
        try
        {
            
        }
        UploadStatusRequest request = new(uploadId);
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
