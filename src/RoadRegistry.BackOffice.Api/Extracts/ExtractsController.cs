namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Framework;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("extracts")]
[ApiExplorerSettings(GroupName = "Extracts")]
[ApiKeyAuth("Road")]
public class ExtractsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExtractsController(IMediator mediator)
    {
        _mediator = mediator;
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

    [HttpGet("upload/{uploadId}/status")]
    public async Task<IActionResult> GetUploadStatus(
        [FromRoute] string uploadId,
        [FromServices] ExtractUploadsOptions options,
        CancellationToken cancellationToken)
    {
        void AddHeaderRetryAfter(int retryAfter)
        {
            if (retryAfter > 0) Response.Headers.Add("Retry-After", retryAfter.ToString(CultureInfo.InvariantCulture));
        }

        try
        {
            UploadStatusRequest request = new(uploadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
            var response = await _mediator.Send(request, cancellationToken);
            AddHeaderRetryAfter(response.RetryAfter);
            return Ok(new GetUploadStatusResponseBody { Status = response.Status });
        }
        catch (UploadExtractNotFoundException exception)
        {
            AddHeaderRetryAfter(exception.RetryAfterSeconds);
            return NotFound();
        }
    }

    [HttpPost("downloadrequests")]
    public async Task<IActionResult> PostDownloadRequest([FromBody] DownloadExtractRequestBody body, CancellationToken cancellationToken)
    {
        DownloadExtractRequest request = new(body.RequestId, body.Contour);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody { DownloadId = response.DownloadId.ToString() });
    }

    [HttpPost("downloadrequests/bycontour")]
    public async Task<ActionResult> PostDownloadRequestByContour([FromBody] DownloadExtractByContourRequestBody body, CancellationToken cancellationToken)
    {
        DownloadExtractByContourRequest request = new(body.Contour, body.Buffer, body.Description);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody { DownloadId = response.DownloadId.ToString() });
    }

    [HttpPost("downloadrequests/byniscode")]
    public async Task<ActionResult> PostDownloadRequestByNisCode([FromBody] DownloadExtractByNisCodeRequestBody body, CancellationToken cancellationToken)
    {
        DownloadExtractByNisCodeRequest request = new(body.NisCode, body.Buffer, body.Description);
        try
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new DownloadExtractResponseBody { DownloadId = response.DownloadId.ToString() });
        }
        catch (DownloadExtractByNisCodeNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("download/{downloadId}/uploads/feature-compare")]
    public async Task<ActionResult> PostFeatureCompareUpload(
        [FromRoute] string downloadId,
        IFormFile archive,
        CancellationToken cancellationToken)
    {
        try
        {
            UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            var request = new UploadExtractFeatureCompareRequest(downloadId, requestArchive);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(response);
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("download/{downloadId}/uploads")]
    public async Task<ActionResult> PostUpload(
        [FromRoute] string downloadId,
        IFormFile archive,
        CancellationToken cancellationToken)
    {
        try
        {
            UploadExtractArchiveRequest requestArchive = new(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            var request = new UploadExtractRequest(downloadId, requestArchive);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new UploadExtractResponseBody { UploadId = response.UploadId.ToString() });
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
    }
}
