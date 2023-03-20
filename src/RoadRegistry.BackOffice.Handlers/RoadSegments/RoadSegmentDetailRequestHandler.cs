namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using BackOffice.Extracts.Dbase.RoadSegments;
using Editor.Projections;
using Editor.Schema;
using Framework;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

public class RoadSegmentDetailRequestHandler : EndpointRequestHandler<RoadSegmentDetailRequest, RoadSegmentDetailResponse>
{
    private readonly EditorContext _editorContext;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IStreetNameCache _streetNameCache;

    public RoadSegmentDetailRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<RoadSegmentDetailRequestHandler> logger,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager,
        IStreetNameCache streetNameCache)
        : base(dispatcher, logger)
    {
        _editorContext = editorContext;
        _manager = manager;
        _streetNameCache = streetNameCache;
    }

    public override async Task<RoadSegmentDetailResponse> HandleAsync(RoadSegmentDetailRequest request, CancellationToken cancellationToken)
    {
        var roadSegment = await _editorContext.RoadSegments.FindAsync(request.WegsegmentId);
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        var dbfRecord = new RoadSegmentDbaseRecord().FromBytes(roadSegment.DbaseRecord, _manager, WellKnownEncodings.WindowsAnsi);

        var streetNameIds = new[] { dbfRecord.LSTRNMID?.Value, dbfRecord.RSTRNMID?.Value }
            .Where(x => x != null)
            .Select(x => x.Value)
            .Distinct()
            .ToArray();
        var streetNames = await _streetNameCache.GetStreetNamesById(streetNameIds, cancellationToken);

        string GetStreetName(int? id)
        {
            if (id != null && streetNames.TryGetValue(id.Value, out var streetName))
            {
                return streetName;
            }

            return null;
        }

        return new RoadSegmentDetailResponse(
            roadSegment.Id,
            dbfRecord.BEGINTIJD.Value,
            dbfRecord.LSTRNMID?.Value,
            GetStreetName(dbfRecord.LSTRNMID?.Value),
            dbfRecord.RSTRNMID?.Value,
            GetStreetName(dbfRecord.RSTRNMID?.Value),
            roadSegment.LastEventHash
        );
    }
}
