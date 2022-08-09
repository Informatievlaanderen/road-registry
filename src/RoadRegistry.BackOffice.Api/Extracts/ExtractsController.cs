namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Configuration;
using Framework;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        DownloadExtractRequest request = new(body.RequestId, body.Contour);
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
    public async Task<ActionResult> GetDownload(
        [FromRoute] string downloadId,
        [FromServices] ExtractDownloadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            DownloadFileContentRequest request = new(downloadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
        }
        catch (BlobNotFoundException) // This condition can only occur if the blob no longer exists in the bucket
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
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
            UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));

            var request = featureCompare
                ? new UploadExtractFeatureCompareRequest(downloadId, requestArchive)
                : new UploadExtractRequest(downloadId, requestArchive);
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
        [FromServices] ExtractUploadsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            UploadStatusRequest request = new(uploadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
        catch (UploadExtractNotFoundException exception)
        {
            if (exception.RetryAfterSeconds > 0) Response.Headers.Add("Retry-After", exception.RetryAfterSeconds.ToString(CultureInfo.InvariantCulture));
            return NotFound();
        }
    }
}
