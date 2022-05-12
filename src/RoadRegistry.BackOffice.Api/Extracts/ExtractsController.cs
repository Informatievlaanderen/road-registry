namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using BackOffice.Extracts;
    using BackOffice.Framework;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Configuration;
    using Editor.Schema;
    using Editor.Schema.Extracts;
    using FluentValidation;
    using FluentValidation.Results;
    using Framework;
    using Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Net.Http.Headers;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extracts")]
    [ApiExplorerSettings(GroupName = "Extracts")]
    public class ExtractsController : ControllerBase
    {
        private static readonly ContentType[] SupportedContentTypes =
        {
            ContentType.Parse("application/zip"),
            ContentType.Parse("application/x-zip-compressed")
        };

        private readonly CommandHandlerDispatcher _dispatcher;
        private readonly RoadNetworkExtractDownloadsBlobClient _downloadsClient;
        private readonly RoadNetworkExtractUploadsBlobClient _uploadsClient;
        private readonly WKTReader _reader;
        private readonly IValidator<DownloadExtractRequestBody> _downloadExtractRequestBodyValidator;
        private readonly IValidator<DownloadExtractByContourRequestBody> _downloadExtractByContourRequestBodyValidator;
        private readonly IValidator<DownloadExtractByNisCodeRequestBody> _downloadExtractByNisCodeRequestBodyValidator;
        private readonly EditorContext _editorContext;
        private readonly IClock _clock;

        public ExtractsController(
            IClock clock,
            CommandHandlerDispatcher dispatcher,
            RoadNetworkExtractDownloadsBlobClient downloadsClient,
            RoadNetworkExtractUploadsBlobClient uploadsClient,
            WKTReader reader,
            IValidator<DownloadExtractRequestBody> downloadExtractRequestBodyValidator,
            IValidator<DownloadExtractByContourRequestBody> downloadExtractByContourRequestBodyValidator,
            IValidator<DownloadExtractByNisCodeRequestBody> downloadExtractByNisCodeRequestBodyValidator,
            EditorContext editorContext)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _downloadsClient = downloadsClient ?? throw new ArgumentNullException(nameof(downloadsClient));
            _uploadsClient = uploadsClient ?? throw new ArgumentNullException(nameof(uploadsClient));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _downloadExtractRequestBodyValidator = downloadExtractRequestBodyValidator ?? throw new ArgumentNullException(nameof(downloadExtractRequestBodyValidator));
            _downloadExtractByContourRequestBodyValidator = downloadExtractByContourRequestBodyValidator ?? throw new ArgumentNullException(nameof(downloadExtractByContourRequestBodyValidator));
            _downloadExtractByNisCodeRequestBodyValidator = downloadExtractByNisCodeRequestBodyValidator ?? throw new ArgumentNullException(nameof(downloadExtractByNisCodeRequestBodyValidator));
            _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
        }

        [HttpPost("downloadrequests")]
        public async Task<IActionResult> PostDownloadRequest([FromBody]DownloadExtractRequestBody body)
        {
            await _downloadExtractRequestBodyValidator.ValidateAndThrowAsync(body, HttpContext.RequestAborted);

            var downloadId = new DownloadId(Guid.NewGuid());
            var message = new Command(
                new RequestRoadNetworkExtract
                {
                    ExternalRequestId = body.RequestId,
                    Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(
                        (NetTopologySuite.Geometries.IPolygonal) _reader.Read(body.Contour)),
                    DownloadId = downloadId,
                    Description = string.Empty
                });
            await _dispatcher(message, HttpContext.RequestAborted);
            return Accepted(new DownloadExtractResponseBody {DownloadId = downloadId.ToString()});
        }

        [HttpPost("downloadrequests/bycontour")]
        public async Task<IActionResult> PostDownloadRequestByContour([FromBody]DownloadExtractByContourRequestBody body)
        {
            await _downloadExtractByContourRequestBodyValidator.ValidateAndThrowAsync(body, HttpContext.RequestAborted);

            var downloadId = new DownloadId(Guid.NewGuid());
            var randomExternalRequestId = Guid.NewGuid().ToString("N");
            var message = new Command(
                new RequestRoadNetworkExtract
                {
                    ExternalRequestId = randomExternalRequestId,
                    Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(
                        _reader.Read(body.Contour) as NetTopologySuite.Geometries.IPolygonal, body.Buffer),
                    DownloadId = downloadId,
                    Description = body.Description
                });
            await _dispatcher(message, HttpContext.RequestAborted);
            return Accepted(new DownloadExtractResponseBody {DownloadId = downloadId.ToString()});
        }

        [HttpPost("downloadrequests/byniscode")]
        public async Task<IActionResult> PostDownloadRequestByNisCode([FromBody]DownloadExtractByNisCodeRequestBody body)
        {
            await _downloadExtractByNisCodeRequestBodyValidator.ValidateAndThrowAsync(body, HttpContext.RequestAborted);
            var municipalityGeometry = await _editorContext.MunicipalityGeometries.SingleAsync(x => x.NisCode == body.NisCode, HttpContext.RequestAborted);

            var downloadId = new DownloadId(Guid.NewGuid());
            var randomExternalRequestId = Guid.NewGuid().ToString("N");
            var message = new Command(
                new RequestRoadNetworkExtract
                {
                    ExternalRequestId = randomExternalRequestId,
                    Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry(municipalityGeometry.Geometry as MultiPolygon, body.Buffer),
                    DownloadId = downloadId,
                    Description = body.Description
                });
            await _dispatcher(message, HttpContext.RequestAborted);
            return Accepted(new DownloadExtractResponseBody {DownloadId = downloadId.ToString()});
        }

        [HttpGet("download/{downloadid}")]
        public async Task<IActionResult> GetDownload(
            [FromServices]EditorContext context,
            [FromServices]ExtractDownloadsOptions options,
            [FromRoute]string downloadid)
        {
            if (Guid.TryParseExact(downloadid, "N", out var parsed))
            {
                var record = await context.ExtractDownloads.FindAsync(new object[] { parsed }, HttpContext.RequestAborted);
                if (record == null || !record.Available)
                {
                    var retryAfterSeconds =
                        await context.ExtractDownloads.TookAverageAssembleDuration(
                            _clock
                                .GetCurrentInstant()
                                .Minus(Duration.FromDays(options.RetryAfterAverageWindowInDays)),
                            options.DefaultRetryAfter);

                    Response.Headers.Add("Retry-After", retryAfterSeconds.ToString(CultureInfo.InvariantCulture));
                    return NotFound();
                }

                // FOUND

                var blobName = new BlobName(record.ArchiveId);

                if(!await _downloadsClient.BlobExistsAsync(blobName, HttpContext.RequestAborted))
                {
                    // NOTE: This condition can only occur if the blob no longer exists in the bucket
                    return StatusCode((int)HttpStatusCode.Gone);
                }

                var blob = await _downloadsClient.GetBlobAsync(blobName, HttpContext.RequestAborted);

                var filename = downloadid + ".zip";

                return new FileCallbackResult(
                    new MediaTypeHeaderValue("application/zip"),
                    async (stream, actionContext) =>
                    {
                        using (var blobStream = await blob.OpenAsync(actionContext.HttpContext.RequestAborted))
                        {
                            await blobStream.CopyToAsync(stream, actionContext.HttpContext.RequestAborted);
                        }
                    })
                {
                    FileDownloadName = filename
                };
            }

            // results in BAD REQUEST
            throw new ValidationException(new[]
            {
                new ValidationFailure("downloadid",
                    "'DownloadId' path parameter is not a global unique identifier without dashes.")
            });
        }

        [HttpPost("download/{downloadid}/uploads")]
        public async Task<IActionResult> PostUpload(
            [FromServices] EditorContext context,
            [FromRoute] string downloadid,
            IFormFile archive)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (downloadid == null) throw new ArgumentNullException(nameof(downloadid));

            if (Guid.TryParseExact(downloadid, "N", out var parsedDownloadId))
            {
                var formContentType = HttpContext.Request.ContentType?.Split(';');
                if (formContentType == null ||
                    formContentType.Length == 0 ||
                    !ContentType.TryParse(formContentType[0], out var parsedFormContentType) ||
                    !parsedFormContentType.Equals(ContentType.Parse("multipart/form-data")))
                {
                    return new UnsupportedMediaTypeResult();
                }

                var download =
                    await context.ExtractDownloads.FindAsync(new object[] { parsedDownloadId },
                        HttpContext.RequestAborted);
                if (download == null)
                {
                    return NotFound();
                }

                if (archive == null)
                {
                    throw new ValidationException(new[]
                    {
                        new ValidationFailure("archive",
                            "'Archive' body parameter is missing.")
                    });
                }

                if (!ContentType.TryParse(archive.ContentType, out var parsedContentType) ||
                    !SupportedContentTypes.Contains(parsedContentType))
                {
                    return new UnsupportedMediaTypeResult();
                }

                using (var readStream = archive.OpenReadStream())
                {
                    var uploadId = new UploadId(Guid.NewGuid());
                    var archiveId = new ArchiveId(uploadId.ToString());

                    var metadata = Metadata.None;

                    await _uploadsClient.CreateBlobAsync(
                        new BlobName(archiveId.ToString()),
                        metadata,
                        ContentType.Parse("application/zip"),
                        readStream,
                        HttpContext.RequestAborted
                    );

                    var message = new Command(
                    new UploadRoadNetworkExtractChangesArchive
                        {
                            RequestId = download.RequestId,
                            DownloadId = download.DownloadId,
                            UploadId = uploadId.ToGuid(),
                            ArchiveId = archiveId.ToString()
                        });

                    try
                    {
                        await _dispatcher(message, HttpContext.RequestAborted);
                    }
                    catch (CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload exception)
                    {
                        throw new ApiProblemDetailsException(
                            "Can not upload roadnetwork extract changes archive for superseded download",
                            409, new ExceptionProblemDetails(exception), exception);
                    }
                    catch (CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce exception)
                    {
                        throw new ApiProblemDetailsException(
                            "Can not upload roadnetwork extract changes archive for same download more than once",
                            409, new ExceptionProblemDetails(exception), exception);
                    }

                    return Accepted(new UploadExtractResponseBody { UploadId = uploadId.ToString() });
                }
            }

            // results in BAD REQUEST
            throw new ValidationException(new[]
            {
                new ValidationFailure("downloadid",
                    "'DownloadId' path parameter is not a global unique identifier without dashes.")
            });
        }

        [HttpGet("upload/{uploadid}/status")]
        public async Task<IActionResult> GetUploadStatus(
            [FromServices]EditorContext context,
            [FromServices]ExtractUploadsOptions options,
            [FromRoute]string uploadid)
        {
            if (Guid.TryParseExact(uploadid, "N", out var parsed))
            {
                var record = await context.ExtractUploads.FindAsync(new object[] { parsed }, HttpContext.RequestAborted);
                if (record == null)
                {
                    var retryAfterSeconds =
                        await context.ExtractUploads.TookAverageProcessDuration(
                            _clock
                                .GetCurrentInstant()
                                .Minus(Duration.FromDays(options.RetryAfterAverageWindowInDays)),
                            options.DefaultRetryAfter);

                    Response.Headers.Add("Retry-After", retryAfterSeconds.ToString(CultureInfo.InvariantCulture));
                    return NotFound();
                }

                // FOUND

                var body = new GetUploadStatusResponseBody();
                switch (record.Status)
                {
                    case ExtractUploadStatus.Received:
                    case ExtractUploadStatus.UploadAccepted:
                        var retryAfterSeconds =
                            await context.ExtractUploads.TookAverageProcessDuration(
                                _clock
                                    .GetCurrentInstant()
                                    .Minus(Duration.FromDays(options.RetryAfterAverageWindowInDays)),
                                options.DefaultRetryAfter);

                        Response.Headers.Add("Retry-After", retryAfterSeconds.ToString(CultureInfo.InvariantCulture));

                        body.Status = "Processing";
                        break;
                    case ExtractUploadStatus.UploadRejected:
                    case ExtractUploadStatus.ChangesRejected:
                        body.Status = "Rejected";
                        break;
                    case ExtractUploadStatus.ChangesAccepted:
                        body.Status = "Accepted";
                        break;
                    case ExtractUploadStatus.NoChanges:
                        body.Status = "No Changes";
                        break;
                    default:
                        body.Status = "Unknown";
                        break;
                }

                return Ok(body);
            }

            // results in BAD REQUEST
            throw new ValidationException(new[]
            {
                new ValidationFailure("uploadid",
                    "'UploadId' path parameter is not a global unique identifier without dashes.")
            });
        }
    }
}
