namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Extracts;
using Editor.Schema;
using Exceptions;
using Framework;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NodaTime;
using Handlers;
using SqlStreamStore;

public class ExtractDetailsRequestHandler : EndpointRetryableRequestHandler<ExtractDetailsRequest, ExtractDetailsResponse>
{
    public ExtractDetailsRequestHandler(
        EditorContext editorContext,
        CommandHandlerDispatcher dispatcher,
        IClock clock,
        IStreamStore streamStore,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, editorContext, streamStore, clock, logger)
    {
    }

    public override async Task<ExtractDetailsResponse> HandleAsync(ExtractDetailsRequest request, CancellationToken cancellationToken)
    {
        var record = await Context.ExtractRequests.FindAsync(new object[] { request.DownloadId.ToGuid() }, cancellationToken);

        if (record is null)
        {
            var retryAfter = await CalculateRetryAfterAsync(request, cancellationToken);
            throw new ExtractRequestNotFoundException(request.DownloadId, Convert.ToInt32(retryAfter.TotalSeconds));
        }

        return new ExtractDetailsResponse
        {
            DownloadId = new DownloadId(record.DownloadId),
            Description = record.Description,
            Contour = record.Contour.ToMultiPolygon(),
            ExtractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(record.ExternalRequestId)),
            RequestedOn = record.RequestedOn,
            IsInformative = record.IsInformative
        };
    }
}
