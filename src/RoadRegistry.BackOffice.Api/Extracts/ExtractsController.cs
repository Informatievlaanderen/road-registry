namespace RoadRegistry.BackOffice.Api.Extracts;

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Abstractions.Uploads;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core.ProblemCodes;
using FeatureToggles;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DownloadExtractRequest = Abstractions.Extracts.DownloadExtractRequest;
using UploadExtractFeatureCompareRequest = Abstractions.Extracts.UploadExtractFeatureCompareRequest;
using UploadExtractRequest = Abstractions.Extracts.UploadExtractRequest;
using Version = Infrastructure.Version;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("extracts")]
[ApiExplorerSettings(GroupName = "Extracts")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
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
            var request = new DownloadFileContentRequest(downloadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
            var response = await _mediator.Send(request, cancellationToken);
            return new FileCallbackResult(response);
        }
        catch (BlobNotFoundException) // This condition can only occur if the blob no longer exists in the bucket
        {
            return StatusCode((int)HttpStatusCode.Gone);
        }
        catch (DownloadExtractNotFoundException)
        {
            return NotFound();
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
            if (retryAfter > 0)
            {
                Response.Headers.Add("Retry-After", retryAfter.ToString(CultureInfo.InvariantCulture));
            }
        }

        try
        {
            var request = new UploadStatusRequest(uploadId, options.DefaultRetryAfter, options.RetryAfterAverageWindowInDays);
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
        var request = new DownloadExtractRequest(body.RequestId, body.Contour);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody { DownloadId = response.DownloadId.ToString() });
    }

    [HttpPost("downloadrequests/bycontour")]
    public async Task<ActionResult> PostDownloadRequestByContour([FromBody] DownloadExtractByContourRequestBody body, CancellationToken cancellationToken)
    {
        var request = new DownloadExtractByContourRequest(body.Contour, body.Buffer, body.Description);
        var response = await _mediator.Send(request, cancellationToken);
        return Accepted(new DownloadExtractResponseBody { DownloadId = response.DownloadId.ToString() });
    }

    [HttpPost("downloadrequests/byfile")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<ActionResult> PostDownloadRequestByFile(DownloadExtractByFileRequestBody body, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadExtractByFileRequest(BuildRequestItem(".shp"), BuildRequestItem(".prj"), body.Buffer, body.Description);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new DownloadExtractResponseBody { DownloadId = response.DownloadId.ToString() });

            DownloadExtractByFileRequestItem BuildRequestItem(string extension)
            {
                var file = body.Files.SingleOrDefault(formFile => formFile.FileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)) ?? throw new InvalidOperationException($"No file ends with extension {extension}");
                var fileStream = new MemoryStream();
                file.CopyTo(fileStream);
                fileStream.Position = 0;
                return new DownloadExtractByFileRequestItem(file.FileName, fileStream, ContentType.Parse(file.ContentType));
            }
        }
        catch (DownloadExtractByFileNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("downloadrequests/byniscode")]
    public async Task<ActionResult> PostDownloadRequestByNisCode([FromBody] DownloadExtractByNisCodeRequestBody body, CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadExtractByNisCodeRequest(body.NisCode, body.Buffer, body.Description);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new DownloadExtractResponseBody { DownloadId = response.DownloadId.ToString() });
        }
        catch (DownloadExtractByNisCodeNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("download/{downloadId}/uploads/fc")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public Task<IActionResult> PostUploadBeforeFeatureCompare(
        [FromServices] UseZipArchiveFeatureCompareTranslatorFeatureToggle useZipArchiveFeatureCompareTranslatorFeatureToggle,
        [FromRoute] string downloadId,
        IFormFile archive,
        CancellationToken cancellationToken)
    {
        return PostUpload(archive, async () =>
        {
            if (useZipArchiveFeatureCompareTranslatorFeatureToggle.FeatureEnabled)
            {
                var requestArchive = new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
                var request = new UploadExtractRequest(downloadId, requestArchive)
                {
                    UseZipArchiveFeatureCompareTranslator = useZipArchiveFeatureCompareTranslatorFeatureToggle.FeatureEnabled
                };
                var response = await _mediator.Send(request, cancellationToken);
                return Accepted(new UploadExtractResponseBody { UploadId = response.UploadId.ToString() });
            }
            else
            {
                var requestArchive = new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
                var request = new UploadExtractFeatureCompareRequest(downloadId, requestArchive);
                var response = await _mediator.Send(request, cancellationToken);
                return Accepted(new UploadExtractBeforeFeatureCompareResponseBody { ArchiveId = response.ArchiveId });
            }
        });
    }

    [HttpPost("download/{downloadId}/uploads")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public Task<IActionResult> PostUploadAfterFeatureCompare(
        [FromRoute] string downloadId,
        IFormFile archive,
        CancellationToken cancellationToken)
    {
        return PostUpload(archive, async () =>
        {
            var requestArchive = new UploadExtractArchiveRequest(archive.FileName, archive.OpenReadStream(), ContentType.Parse(archive.ContentType));
            var request = new UploadExtractRequest(downloadId, requestArchive);
            var response = await _mediator.Send(request, cancellationToken);
            return Accepted(new UploadExtractResponseBody { UploadId = response.UploadId.ToString() });
        });
    }

    private async Task<IActionResult> PostUpload(IFormFile archive, Func<Task<IActionResult>> callback)
    {
        if (archive == null)
        {
            throw new ValidationException("Archive is required", new[]
            {
                new ValidationFailure
                {
                    PropertyName = nameof(archive),
                    ErrorCode = ProblemCode.Common.IsRequired
                }
            });
        }

        try
        {
            return await callback.Invoke();
        }
        catch (UnsupportedMediaTypeException)
        {
            return new UnsupportedMediaTypeResult();
        }
        catch (ExtractDownloadNotFoundException)
        {
            return NotFound();
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException exception)
        {
            throw new ApiProblemDetailsException(
                "Can not upload roadnetwork extract changes archive for superseded download",
                409,
                new ExceptionProblemDetails(exception), exception);
        }
        catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException exception)
        {
            throw new ApiProblemDetailsException(
                "Can not upload roadnetwork extract changes archive for same download more than once",
                409,
                new ExceptionProblemDetails(exception), exception);
        }
    }
}
