namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using BackOffice.Handlers;
using Editor.Schema;
using Exceptions;
using Framework;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NodaTime;
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

    protected override async Task<ExtractDetailsResponse> InnerHandleAsync(ExtractDetailsRequest request, CancellationToken cancellationToken)
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
