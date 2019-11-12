namespace RoadRegistry.Api.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using BackOffice.Schema;
    using BackOffice.Translation;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json.Converters;
    using Responses;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Swashbuckle.AspNetCore.Filters;
    using ZipArchiveWriters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("upload")]
    [ApiExplorerSettings(GroupName = "Uploads")]
    public class UploadController : ControllerBase
    {
        /// <summary>
        /// Request an archive of the entire road registry for shape editing purposes.
        /// </summary>
        /// <param name="dispatcher">The command handler dispatcher.</param>
        /// <param name="client">The blob client to upload the file with.</param>
        /// <param name="archive">The file to upload.</param>
        /// <response code="200">Returned if the file was uploaded.</response>
        /// <response code="415">Returned if the file can not be uploaded due to an unsupported media type.</response>
        /// <response code="500">Returned if the file can not be uploaded due to an unforeseen server error.</response>
        [HttpPost("")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnsupportedMediaTypeResult), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UploadResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status415UnsupportedMediaType, typeof(UnsupportedMediaTypeResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Post(
            [FromServices] CommandHandlerDispatcher dispatcher,
            [FromServices] IBlobClient client,
            IFormFile archive)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (archive == null) throw new ArgumentNullException(nameof(archive));

            if (!ContentType.TryParse(archive.ContentType, out var parsed) ||
                parsed != ContentType.Parse("application/zip"))
            {
                return new UnsupportedMediaTypeResult();
            }

            using (var readStream = archive.OpenReadStream())
            {
                var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));

                var metadata = Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                    string.IsNullOrEmpty(archive.FileName) ? archiveId + ".zip" : archive.FileName));

                await client.CreateBlobAsync(
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

                await dispatcher(message, HttpContext.RequestAborted);
            }

            return Ok();
        }

        [HttpGet("{identifier}")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromServices] IBlobClient client, string identifier)
        {
            if (identifier == null)
            {
                return BadRequest();
            }

            var archiveId = new ArchiveId(identifier);
            var blobName = new BlobName(archiveId.ToString());

            if(!await client.BlobExistsAsync(blobName, HttpContext.RequestAborted))
            {
                return NotFound();
            }

            var blob = await client.GetBlobAsync(blobName, HttpContext.RequestAborted);

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
