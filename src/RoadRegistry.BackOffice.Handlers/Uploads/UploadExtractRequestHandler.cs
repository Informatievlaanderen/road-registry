namespace RoadRegistry.BackOffice.Handlers.Uploads;

using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Contracts.Uploads;
using Editor.Schema;
using Exceptions;
using Framework;
using MediatR.Pipeline;
using Messages;
using Microsoft.Extensions.Logging;

/// <summary>
///     Post upload extract controller
/// </summary>
/// <exception cref="UploadExtractNotFoundException"></exception>
/// <exception cref="UploadExtractBlobClientNotFoundException"></exception>
/// <exception cref="EditorContextNotFoundException"></exception>
/// <exception cref="UploadExtractBlobClientNotFoundException"></exception>
/// <exception cref="UnsupportedMediaTypeException"></exception>
internal class UploadExtractRequestHandler : EndpointRequestHandler<UploadExtractRequest, UploadExtractResponse>,
    IRequestExceptionHandler<UploadExtractRequest, UploadExtractResponse, UploadExtractNotFoundException>
{
    private static readonly ContentType[] SupportedContentTypes =
    {
        ContentType.Parse("application/zip"),
        ContentType.Parse("application/x-zip-compressed")
    };

    private readonly RoadNetworkExtractUploadsBlobClient _client;
    private readonly EditorContext _context;
    private readonly ILogger<UploadExtractRequestHandler> _logger;

    public UploadExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        EditorContext context,
        ILogger<UploadExtractRequestHandler> logger) : base(dispatcher, logger)
    {
        _client = client ?? throw new UploadExtractBlobClientNotFoundException(nameof(client));
        _context = context ?? throw new EditorContextNotFoundException(nameof(context));
        _logger = logger ?? throw new LoggerNotFoundException<UploadExtractRequestHandler>();
    }

    public Task Handle(
        UploadExtractRequest request,
        UploadExtractNotFoundException exception,
        RequestExceptionHandlerState<UploadExtractResponse> state,
        CancellationToken cancellationToken)
    {
        return Task.FromException(exception);
    }

    public override async Task<UploadExtractResponse> HandleAsync(UploadExtractRequest request, CancellationToken cancellationToken)
    {
        if (request.DownloadId is null) throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");

        if (Guid.TryParseExact(request.DownloadId, "N", out var parsedDownloadId))
        {
            var download = await _context.ExtractDownloads.FindAsync(new object[] { parsedDownloadId }, cancellationToken)
                           ?? throw new ExtractDownloadNotFoundException(DownloadId.Parse(parsedDownloadId.ToString()));

            if (!ContentType.TryParse(request.Archive.ContentType, out var parsedContentType) || !SupportedContentTypes.Contains(parsedContentType))
                throw new UnsupportedMediaTypeException();

            await using var readStream = request.Archive.ReadStream;

            var uploadId = new UploadId(Guid.NewGuid());
            var archiveId = new ArchiveId(uploadId.ToString());
            var metadata = Metadata.None;

            await _client.CreateBlobAsync(
                new BlobName(archiveId.ToString()),
                metadata,
                ContentType.Parse("application/zip"),
                readStream,
                cancellationToken
            );

            var message = new Command(
                new UploadRoadNetworkExtractChangesArchive
                {
                    RequestId = download.RequestId,
                    DownloadId = download.DownloadId,
                    UploadId = uploadId.ToGuid(),
                    ArchiveId = archiveId.ToString(),
                    IsFeatureCompare = request.FeatureCompare
                });

            await Dispatcher(message, cancellationToken);

            return new UploadExtractResponse(uploadId);
        }

        throw new UploadExtractNotFoundException($"Could not upload the extract with filename {request.Archive.FileName}");
    }
}

public class EditorContextNotFoundException : ApplicationException
{
    public EditorContextNotFoundException(string argumentName) : base("Could not resolve an editor context")
    {
        ArgumentName = argumentName;
    }

    public string ArgumentName { get; init; }
}
