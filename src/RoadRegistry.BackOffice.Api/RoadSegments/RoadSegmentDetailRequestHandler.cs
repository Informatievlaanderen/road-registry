namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly IDbContextFactory<EditorContext> _editorContextFactory;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly IStreetNameCache _streetNameCache;

    public RoadSegmentDetailRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<RoadSegmentDetailRequestHandler> logger,
        IDbContextFactory<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        IStreetNameCache streetNameCache)
        : base(dispatcher, logger)
    {
        _editorContextFactory = editorContextFactory;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _streetNameCache = streetNameCache;
    }

    protected override async Task<RoadSegmentDetailResponse> InnerHandleAsync(RoadSegmentDetailRequest request, CancellationToken cancellationToken)
    {
        var roadSegment = await (await _editorContextFactory.CreateDbContextAsync(cancellationToken)).RoadSegments
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

        var surfaceTask = Task.Run(async () =>
        {
            await using var dbContext = await _editorContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.RoadSegmentSurfaceAttributes
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .ToListAsync(cancellationToken);
        }, cancellationToken);

        var widthTask = Task.Run(async () =>
        {
            await using var dbContext = await _editorContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.RoadSegmentWidthAttributes
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .ToListAsync(cancellationToken);
        }, cancellationToken);

        var laneTask = Task.Run(async () =>
        {
            await using var dbContext = await _editorContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.RoadSegmentLaneAttributes
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .ToListAsync(cancellationToken);
        }, cancellationToken);

        var europeanRoadTask = Task.Run(async () =>
        {
            await using var dbContext = await _editorContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.RoadSegmentEuropeanRoadAttributes
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .ToListAsync(cancellationToken);
        }, cancellationToken);

        var nationalRoadTask = Task.Run(async () =>
        {
            await using var dbContext = await _editorContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.RoadSegmentNationalRoadAttributes
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .ToListAsync(cancellationToken);
        }, cancellationToken);

        var numberedRoadTask = Task.Run(async () =>
        {
            await using var dbContext = await _editorContextFactory.CreateDbContextAsync(cancellationToken);
            return await dbContext.RoadSegmentNumberedRoadAttributes
                .Where(x => x.RoadSegmentId == roadSegment.Id)
                .ToListAsync(cancellationToken);
        }, cancellationToken);

        await Task.WhenAll(surfaceTask, widthTask, laneTask, europeanRoadTask, nationalRoadTask, numberedRoadTask);

        var surfaceTypes = surfaceTask.Result
            .Select(x => new RoadSegmentSurfaceAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        var widths = widthTask.Result
            .Select(x => new RoadSegmentWidthAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        var lanes = laneTask.Result
            .Select(x => new RoadSegmentLaneAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        var europeanRoads = europeanRoadTask.Result
            .Select(x => new RoadSegmentEuropeanRoadAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        var nationalRoads = nationalRoadTask.Result
            .Select(x => new RoadSegmentNationalRoadAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        var numberedRoads = numberedRoadTask.Result
            .Select(x => new RoadSegmentNumberedRoadAttributeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .ToList();

        return new RoadSegmentDetailResponse(
            roadSegment.Id,
            roadSegment.BeginTime,
            roadSegment.Version,
            roadSegment.LastEventHash
        )
        {
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
