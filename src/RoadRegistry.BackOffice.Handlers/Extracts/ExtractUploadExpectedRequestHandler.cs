namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Exceptions;
using Framework;
using Microsoft.Extensions.Logging;

public class ExtractUploadExpectedRequestHandler : EndpointRequestHandler<ExtractUploadExpectedRequest, ExtractUploadExpectedResponse>
{
    private readonly EditorContext _context;

    public ExtractUploadExpectedRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
    }

    public override async Task<ExtractUploadExpectedResponse> HandleAsync(ExtractUploadExpectedRequest request, CancellationToken cancellationToken)
    {
        var record = await _context.ExtractRequests.FindAsync(new object[] { request.DownloadId.ToGuid() }, cancellationToken)
                     ?? throw new ExtractRequestNotFoundException(request.DownloadId);

        record.UploadExpected = request.UploadExpected;

        _context.ExtractRequests.Update(record);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExtractUploadExpectedResponse
        {
            DownloadId = new DownloadId(record.DownloadId),
            Description = record.Description,
            UploadExpected = record.UploadExpected
        };
    }
}
