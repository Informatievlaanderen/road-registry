namespace RoadRegistry.BackOffice.Api.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Framework;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Framework;
    using Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("upload")]
    [ApiExplorerSettings(GroupName = "Uploads")]
    public class UploadController : ControllerBase
    {
        private static readonly ContentType[] SupportedContentTypes =
            {
                ContentType.Parse("application/zip"),
                ContentType.Parse("application/x-zip-compressed")
            };
        private readonly CommandHandlerDispatcher _dispatcher;
        private readonly IBlobClient _client;

        public UploadController(CommandHandlerDispatcher dispatcher, IBlobClient client)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        [HttpPost("")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Post(IFormFile archive)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));

            if (!ContentType.TryParse(archive.ContentType, out var parsed) ||
                !SupportedContentTypes.Contains(parsed))
            {
                return new UnsupportedMediaTypeResult();
            }

            using (var readStream = archive.OpenReadStream())
            {
                var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));

                var metadata = Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                    string.IsNullOrEmpty(archive.FileName) ? archiveId + ".zip" : archive.FileName));

                await _client.CreateBlobAsync(
                    new BlobName(archiveId.ToString()),
                    metadata,
                    ContentType.Parse("application/zip"),
                    readStream,
                    HttpContext.RequestAborted
                );

                var message = new Command(
                    new UploadRoadNetworkChangesArchive
                    {
                        ArchiveId = archiveId.ToString()
                    });

                await _dispatcher(message, HttpContext.RequestAborted);
            }

            return Ok();
        }

        [HttpGet("{identifier}")]
        public async Task<IActionResult> Get(string identifier)
        {
            if (!ArchiveId.Accepts(identifier))
            {
                return BadRequest();
            }

            var archiveId = new ArchiveId(identifier);
            var blobName = new BlobName(archiveId.ToString());

            if(!await _client.BlobExistsAsync(blobName, HttpContext.RequestAborted))
            {
                return NotFound();
            }

            var blob = await _client.GetBlobAsync(blobName, HttpContext.RequestAborted);

            var filename = blob.Metadata
                .Single(pair => pair.Key == new MetadataKey("filename"));

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
                FileDownloadName = filename.Value
            };
        }
    }
}
