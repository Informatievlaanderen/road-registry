namespace RoadRegistry.BackOffice.Api.Uploads;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Handlers.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Uploads;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;

/// <summary>
///     Upload controller, get upload
/// </summary>
/// <exception cref="BlobClientNotFoundException"></exception>
/// <exception cref="ExtractDownloadNotFoundException"></exception>
/// <exception cref="ValidationException"></exception>
public class DownloadExtractRequestHandler : EndpointRequestHandler<DownloadExtractRequest, DownloadExtractResponse>
{
    private readonly RoadNetworkExtractUploadsBlobClient _client;

    public DownloadExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        ILogger<UploadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
    }

    protected override async Task<DownloadExtractResponse> InnerHandleAsync(DownloadExtractRequest request, CancellationToken cancellationToken)
    {
        if (!ArchiveId.Accepts(request.Identifier))
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Identifier),
                    $"'{nameof(request.Identifier)}' path parameter cannot be empty and must be less or equal to {ArchiveId.MaxLength} characters.")
            });

        var archiveId = new ArchiveId(request.Identifier);
        var blobName = new BlobName(archiveId.ToString());

        if (!await _client.BlobExistsAsync(blobName, cancellationToken))
        {
            throw new ExtractDownloadNotFoundException(request.Identifier);
        }

        var blob = await _client.GetBlobAsync(blobName, cancellationToken);
        var metadata = blob.Metadata.Where(pair => pair.Key == new MetadataKey("filename")).ToArray();
        var filename = metadata.Length == 1 ? metadata[0].Value : archiveId + ".zip";

        return new DownloadExtractResponse(
            filename,
            new MediaTypeHeaderValue("application/zip"),
            async (stream, actionContext) =>
            {
                await using var blobStream = await blob.OpenAsync(cancellationToken);
                await blobStream.CopyToAsync(stream, cancellationToken);
            });
    }
}
