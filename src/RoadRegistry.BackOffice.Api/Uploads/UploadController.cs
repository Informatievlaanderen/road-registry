namespace RoadRegistry.Api.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices.ComTypes;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using BackOffice.Schema;
    using BackOffice.Translation;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Converters;
    using Responses;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Swashbuckle.AspNetCore.Filters;

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
        /// <param name="archive">The file to upload.</param>
        /// <param name="client">The blob client to upload the file with.</param>
        /// <param name="cancellationToken">The token that controls request cancellation.</param>
        /// <response code="200">Returned if the file was uploaded.</response>
        /// <response code="500">Returned if the road registry can not be downloaded due to an unforeseen server error.</response>
        /// <response code="503">Returned if the road registry can not yet be downloaded (e.g. because the import has not yet completed).</response>
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
            IFormFile archive,
            CancellationToken cancellationToken)
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

                await client.CreateBlobAsync(
                    new BlobName(archiveId.ToString()),
                    Metadata.None,
                    ContentType.Parse("application/zip"),
                    readStream,
                    cancellationToken
                );

                var message = new Message(
                    new Dictionary<string, object>(),
                    new UploadRoadNetworkChangesArchive
                    {
                        ArchiveId = archiveId.ToString()
                    });

                await dispatcher(message, cancellationToken);
            }

            return Ok();
        }
    }
}
