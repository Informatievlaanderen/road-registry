namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Framework;
using Microsoft.Extensions.Logging;

public class ExtractDetailsRequestHandler : EndpointRequestHandler<ExtractDetailsRequest, ExtractDetailsResponse>
{
    private readonly EditorContext _context;

    public ExtractDetailsRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public override async Task<ExtractDetailsResponse> HandleAsync(ExtractDetailsRequest request, CancellationToken cancellationToken)
    {
        var record = await _context.ExtractDownloads.FindAsync(new object[] { request.DownloadId.ToString() }, cancellationToken);

        return new ExtractDetailsResponse
        {
            //Description = record.Description
            DownloadId = record.DownloadId,
            ExtractRequestId = ExtractRequestId.FromString(record.RequestId),
            ArchiveId = record.ArchiveId,
            Available = record.Available,
            AvailableOn = record.AvailableOn,
            ExternalRequestId = record.ExternalRequestId,
            RequestId = record.RequestId,
            RequestOn = record.RequestedOn,
            DownloadId = new DownloadId(record.DownloadId),
            
            UploadExpected = record.UploadExpected,
        };
    }
}
