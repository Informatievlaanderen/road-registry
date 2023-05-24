namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Exceptions;
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
        _context = context;
    }

    public override async Task<ExtractDetailsResponse> HandleAsync(ExtractDetailsRequest request, CancellationToken cancellationToken)
    {
        var record = await _context.ExtractRequests.FindAsync(new object[] { request.DownloadId.ToString() }, cancellationToken)
                     ?? throw new ExtractRequestNotFoundException(request.DownloadId);

        return new ExtractDetailsResponse
        {
            DownloadId = new DownloadId(record.DownloadId),
            Description = record.Description,
            ExtractRequestId = ExtractRequestId.FromString(record.RequestId),
            ExternalRequestId = record.ExternalRequestId,
            Available = record.Available,
            AvailableOn = new DateTime(record.AvailableOn),
            RequestId = record.RequestId,
            RequestOn = new DateTime(record.RequestedOn),
            UploadExpected = record.UploadExpected
        };
    }
}
