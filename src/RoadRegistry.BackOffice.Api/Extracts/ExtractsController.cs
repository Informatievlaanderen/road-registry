namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Configuration;
    using Editor.Schema;
    using FluentValidation;
    using FluentValidation.Results;
    using Framework;
    using Messages;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using NetTopologySuite.IO;
    using NodaTime;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extracts")]
    [ApiExplorerSettings(GroupName = "Extracts")]
    public class ExtractsController : ControllerBase
    {
        private readonly CommandHandlerDispatcher _dispatcher;
        private readonly ExtractDownloadsBlobClient _client;
        private readonly WKTReader _reader;
        private readonly IValidator<DownloadExtractRequestBody> _downloadExtractRequestBodyValidator;
        private readonly IClock _clock;

        public ExtractsController(
            IClock clock,
            CommandHandlerDispatcher dispatcher,
            ExtractDownloadsBlobClient client,
            WKTReader reader,
            IValidator<DownloadExtractRequestBody> downloadExtractRequestBodyValidator)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _downloadExtractRequestBodyValidator = downloadExtractRequestBodyValidator ?? throw new ArgumentNullException(nameof(downloadExtractRequestBodyValidator));
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
                        (NetTopologySuite.Geometries.MultiPolygon) _reader.Read(body.Contour)),
                    DownloadId = downloadId
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

                if(!await _client.BlobExistsAsync(blobName, HttpContext.RequestAborted))
                {
                    // NOTE: This condition can only occur if the blob no longer exists in the bucket
                    return StatusCode((int)HttpStatusCode.Gone);
                }

                var blob = await _client.GetBlobAsync(blobName, HttpContext.RequestAborted);

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
                    "'DownloadId' querystring parameter is not a global unique identifier without dashes.")
            });
        }
    }
}
