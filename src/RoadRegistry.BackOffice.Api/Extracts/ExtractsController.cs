namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Messages;
    using Microsoft.AspNetCore.Mvc;
    using NetTopologySuite.IO;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extracts")]
    [ApiExplorerSettings(GroupName = "Extracts")]
    public class ExtractsController : ControllerBase
    {
        private readonly CommandHandlerDispatcher _dispatcher;
        private readonly WKTReader _reader;

        public ExtractsController(CommandHandlerDispatcher dispatcher, WKTReader reader)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        [HttpPost("downloadrequests")]
        public async Task<IActionResult> PostDownloadRequest([FromBody]DownloadExtractRequestBody body)
        {
            //TODO: Verify api key

            var validator = new DownloadExtractRequestBodyValidator(_reader);
            await validator.ValidateAndThrowAsync(body, cancellationToken: HttpContext.RequestAborted);

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
            return Ok(new DownloadExtractResponseBody {DownloadId = downloadId.ToString()});
        }

        [HttpGet("download/{downloadid}")]
        public Task<IActionResult> GetDownload([FromRoute]string downloadid)
        {
            return Task.FromResult<IActionResult>(null);
        }
    }
}
