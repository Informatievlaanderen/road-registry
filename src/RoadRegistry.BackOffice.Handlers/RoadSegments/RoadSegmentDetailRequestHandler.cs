namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Dbase.RoadSegments;
using Editor.Projections;
using Editor.Schema;
using Framework;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

public class RoadSegmentDetailRequestHandler : EndpointRequestHandler<RoadSegmentDetailRequest, RoadSegmentDetailResponse>
{
    private readonly EditorContext _editorContext;
    private readonly RecyclableMemoryStreamManager _manager;

    public RoadSegmentDetailRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<RoadSegmentDetailRequestHandler> logger,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager)
        : base(dispatcher, logger)
    {
        _editorContext = editorContext;
        _manager = manager;
    }

    public override async Task<RoadSegmentDetailResponse> HandleAsync(RoadSegmentDetailRequest request, CancellationToken cancellationToken)
    {
        var roadSegment = await _editorContext.RoadSegments.FindAsync(request.WegsegmentId);
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        var dbfRecord = new RoadSegmentDbaseRecord().FromBytes(roadSegment.DbaseRecord, _manager, WellKnownEncodings.WindowsAnsi);
        
        return new RoadSegmentDetailResponse(
            roadSegment.Id,
            dbfRecord.BEGINTIJD.Value,
            dbfRecord.LSTRNMID?.Value,
            dbfRecord.LSTRNM?.Value,
            dbfRecord.RSTRNMID?.Value,
            dbfRecord.RSTRNM?.Value,
            roadSegment.LastEventHash
        );
    }
}
