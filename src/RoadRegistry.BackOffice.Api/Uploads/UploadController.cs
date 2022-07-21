namespace RoadRegistry.BackOffice.Api.Uploads;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;
using FluentValidation.Results;
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

    private readonly RoadNetworkUploadsBlobClient _client;
    private readonly CommandHandlerDispatcher _dispatcher;

    public UploadController(CommandHandlerDispatcher dispatcher, RoadNetworkUploadsBlobClient client)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    [HttpPost("")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> PostUpload([FromBody] IFormFile archive, [FromBody] bool isFeatureCompare = false)
    {
        if (archive == null)
            throw new ArgumentNullException(nameof(archive));

        if (!ContentType.TryParse(archive.ContentType, out var parsed) || !SupportedContentTypes.Contains(parsed))
            return new UnsupportedMediaTypeResult();

        await using (var readStream = archive.OpenReadStream())
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
                    ArchiveId = archiveId.ToString(),
                    IsFeatureCompare = isFeatureCompare
                });

            await _dispatcher(message, HttpContext.RequestAborted);
        }

        return Ok();
    }

    [HttpGet("{identifier}")]
    public async Task<IActionResult> Get(string identifier)
    {
        if (!ArchiveId.Accepts(identifier))
            throw new ValidationException(new[]
            {
                new ValidationFailure("identifier",
                    $"'identifier' path parameter cannot be empty and must be less or equal to {ArchiveId.MaxLength} characters.")
            });

        var archiveId = new ArchiveId(identifier);
        var blobName = new BlobName(archiveId.ToString());

        if (!await _client.BlobExistsAsync(blobName, HttpContext.RequestAborted)) return NotFound();

        var blob = await _client.GetBlobAsync(blobName, HttpContext.RequestAborted);

        var metadata = blob.Metadata
            .Where(pair => pair.Key == new MetadataKey("filename"))
            .ToArray();
        var filename = metadata.Length == 1
            ? metadata[0].Value
            : archiveId + ".zip";

        return new FileCallbackResult(
            new MediaTypeHeaderValue("application/zip"),
            async (stream, actionContext) =>
            {
                await using var blobStream = await blob.OpenAsync(actionContext.HttpContext.RequestAborted);
                await blobStream.CopyToAsync(stream, actionContext.HttpContext.RequestAborted);
            })
        {
            FileDownloadName = filename
        };
    }
}
