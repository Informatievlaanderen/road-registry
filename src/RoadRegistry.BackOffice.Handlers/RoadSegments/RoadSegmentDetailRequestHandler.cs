namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using BackOffice.Extracts.Dbase.RoadSegments;
using Editor.Schema;
using Editor.Schema.Extensions;
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

    protected override async Task<RoadSegmentDetailResponse> InnerHandleAsync(RoadSegmentDetailRequest request, CancellationToken cancellationToken)
    {
        var roadSegment = await _editorContext.RoadSegments
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == request.WegsegmentId, cancellationToken);
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        var streetNameIds = new[] { roadSegment.LeftSideStreetNameId, roadSegment.RightSideStreetNameId }
            .Where(x => x.HasValue)
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

        var europeanRoads = (await _editorContext.RoadSegmentEuropeanRoadAttributes
            .Where(x => x.RoadSegmentId == roadSegment.Id)
            .ToListAsync(cancellationToken: cancellationToken))
            .Select(x => new RoadSegmentEuropeanRoadAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        var nationalRoads = (await _editorContext.RoadSegmentNationalRoadAttributes
            .Where(x => x.RoadSegmentId == roadSegment.Id)
            .ToListAsync(cancellationToken: cancellationToken))
            .Select(x => new RoadSegmentNationalRoadAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        var numberedRoads = (await _editorContext.RoadSegmentNumberedRoadAttributes
            .Where(x => x.RoadSegmentId == roadSegment.Id)
            .ToListAsync(cancellationToken: cancellationToken))
            .Select(x => new RoadSegmentNumberedRoadAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        return new RoadSegmentDetailResponse(
            roadSegment.Id,
            roadSegment.BeginTime,
            roadSegment.Version,
            roadSegment.LastEventHash
        ) {
            Geometry = GeometryTranslator.Translate((MultiLineString)roadSegment.Geometry),
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegment.MethodId],
            StartNodeId = roadSegment.StartNodeId,
            EndNodeId = roadSegment.EndNodeId,
            LeftStreetNameId = roadSegment.LeftSideStreetNameId,
            LeftStreetName = GetStreetName(roadSegment.LeftSideStreetNameId),
            RightStreetNameId = roadSegment.RightSideStreetNameId,
            RightStreetName = GetStreetName(roadSegment.RightSideStreetNameId),
            Status = RoadSegmentStatus.ByIdentifier[roadSegment.StatusId],
            Morphology = RoadSegmentMorphology.ByIdentifier[roadSegment.MorphologyId],
            AccessRestriction = RoadSegmentAccessRestriction.ByIdentifier[roadSegment.AccessRestrictionId],
            MaintenanceAuthority = new MaintenanceAuthority
            {
                Code = roadSegment.MaintainerId,
                Name = roadSegment.MaintainerName
            },
            Category = RoadSegmentCategory.ByIdentifier[roadSegment.CategoryId],
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
            }).OrderBy(x => x.FromPosition).ToList(),
            EuropeanRoads = europeanRoads.Select(x => new RoadSegmentEuropeanRoadDetailResponse
            {
                Number = EuropeanRoadNumber.Parse(x.EUNUMMER.Value)
            }).ToList(),
            NationalRoads = nationalRoads.Select(x => new RoadSegmentNationalRoadDetailResponse
            {
                Number = NationalRoadNumber.Parse(x.IDENT2.Value)
            }).ToList(),
            NumberedRoads = numberedRoads.Select(x => new RoadSegmentNumberedRoadDetailResponse
            {
                Number = NumberedRoadNumber.Parse(x.IDENT8.Value),
                Direction = RoadSegmentNumberedRoadDirection.ByIdentifier[x.RICHTING.Value],
                Ordinal = new RoadSegmentNumberedRoadOrdinal(x.VOLGNUMMER.Value)
            }).ToList(),
            IsRemoved = roadSegment.IsRemoved
        };
    }
}
