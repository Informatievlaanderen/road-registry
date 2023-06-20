namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using BackOffice.Extracts.Dbase.RoadSegments;
using Editor.Projections;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite.Geometries;

public class RoadSegmentDetailRequestHandler : EndpointRequestHandler<RoadSegmentDetailRequest, RoadSegmentDetailResponse>
{
    private readonly EditorContext _editorContext;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly IStreetNameCache _streetNameCache;

    public RoadSegmentDetailRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<RoadSegmentDetailRequestHandler> logger,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        IStreetNameCache streetNameCache)
        : base(dispatcher, logger)
    {
        _editorContext = editorContext;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _streetNameCache = streetNameCache;
    }

    public override async Task<RoadSegmentDetailResponse> HandleAsync(RoadSegmentDetailRequest request, CancellationToken cancellationToken)
    {
        var roadSegment = await _editorContext.RoadSegments.FindAsync(new object[]{ request.WegsegmentId }, cancellationToken);
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        var dbfRecord = new RoadSegmentDbaseRecord().FromBytes(roadSegment.DbaseRecord, _manager, _fileEncoding);

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

        var surfaceTypes = (await _editorContext.RoadSegmentSurfaceAttributes
            .Where(x => x.RoadSegmentId == roadSegment.Id)
            .ToListAsync(cancellationToken: cancellationToken))
            .Select(x => new RoadSegmentSurfaceAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();
        
        var widths = (await _editorContext.RoadSegmentWidthAttributes
            .Where(x => x.RoadSegmentId == roadSegment.Id)
            .ToListAsync(cancellationToken: cancellationToken))
            .Select(x => new RoadSegmentWidthAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        var lanes = (await _editorContext.RoadSegmentLaneAttributes
            .Where(x => x.RoadSegmentId == roadSegment.Id)
            .ToListAsync(cancellationToken: cancellationToken))
            .Select(x => new RoadSegmentLaneAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        return new RoadSegmentDetailResponse(
            roadSegment.Id,
            dbfRecord.BEGINTIJD.Value,
            roadSegment.LastEventHash
        ) {
            Geometry = GeometryTranslator.Translate((MultiLineString)roadSegment.Geometry),
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[dbfRecord.METHODE.Value],
            StartNodeId = dbfRecord.B_WK_OIDN.Value,
            EndNodeId = dbfRecord.E_WK_OIDN.Value,
            LeftStreetNameId = dbfRecord.LSTRNMID?.Value,
            LeftStreetName = GetStreetName(dbfRecord.LSTRNMID?.Value),
            RightStreetNameId = dbfRecord.RSTRNMID?.Value,
            RightStreetName = GetStreetName(dbfRecord.RSTRNMID?.Value),
            Status = RoadSegmentStatus.ByIdentifier[dbfRecord.STATUS.Value],
            Morphology = RoadSegmentMorphology.ByIdentifier[dbfRecord.MORFOLOGIE.Value],
            AccessRestriction = RoadSegmentAccessRestriction.ByIdentifier[dbfRecord.TGBEP.Value],
            MaintenanceAuthority = new MaintenanceAuthority
            {
                Code = dbfRecord.BEHEERDER.Value,
                Name = dbfRecord.BEHEERDER.Value
            },
            Category = RoadSegmentCategory.ByIdentifier[dbfRecord.CATEGORIE.Value],
            SurfaceTypes = surfaceTypes.Select(x => new RoadSegmentSurfaceTypeDetailResponse
            {
                FromPosition = x.VANPOS.Value!.Value,
                ToPosition = x.TOTPOS.Value!.Value,
                SurfaceType = RoadSegmentSurfaceType.ByIdentifier[x.TYPE.Value]
            }).OrderBy(x => x.FromPosition).ToList(),
            Widths = widths.Select(x => new RoadSegmentWidthDetailResponse
            {
                FromPosition = x.VANPOS.Value!.Value,
                ToPosition = x.TOTPOS.Value!.Value,
                Width = new RoadSegmentWidth(x.BREEDTE.Value)
            }).OrderBy(x => x.FromPosition).ToList(),
            LaneCounts = lanes.Select(x => new RoadSegmentLaneCountDetailResponse
            {
                FromPosition = x.VANPOS.Value!.Value,
                ToPosition = x.TOTPOS.Value!.Value,
                Count = new RoadSegmentLaneCount(x.AANTAL.Value),
                Direction = RoadSegmentLaneDirection.ByIdentifier[x.RICHTING.Value]
            }).OrderBy(x => x.FromPosition).ToList()
        };
    }
}
