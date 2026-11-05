namespace RoadRegistry.Extracts.ZipArchiveWriters
{
    using System.Linq;
    using Marten;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Editor.Schema;
    using RoadRegistry.Editor.Schema.Organizations;
    using RoadRegistry.Extensions;
    using RoadRegistry.Extracts.Projections;
    using RoadRegistry.Extracts.Schema;
    using RoadRegistry.ScopedRoadNetwork;
    using RoadRegistry.ValueObjects;

    public class ZipArchiveDataSession : IZipArchiveDataSession
    {
        private readonly IDocumentSession _session;
        private readonly IRoadNetworkRepository _roadNetworkRepository;
        private readonly EditorContext _editorContext;
        private readonly ExtractsDbContext _extractsDbContext;

        private readonly Dictionary<Geometry, RoadNetworkExtractDocuments<RoadNodeExtractItem, RoadSegmentExtractItem, GradeSeparatedJunctionExtractItem>> _documentsCache = [];
        private readonly Dictionary<IPolygonal, IReadOnlyList<RoadNodeExtractItem>> _roadNodesCache = [];
        private readonly Dictionary<IPolygonal, IReadOnlyList<RoadSegmentExtractItem>> _roadSegmentsCache = [];
        private readonly Dictionary<IPolygonal, IReadOnlySet<RoadSegmentId>> _editableRoadSegmentIdsCache = [];

        public ZipArchiveDataSession(IDocumentSession session, IRoadNetworkRepository roadNetworkRepository, EditorContext editorContext, ExtractsDbContext extractsDbContext)
        {
            _session = session;
            _roadNetworkRepository = roadNetworkRepository;
            _editorContext = editorContext;
            _extractsDbContext = extractsDbContext;
        }

        public async Task<IReadOnlySet<RoadSegmentId>> GetEditableRoadSegmentIds(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            if (_editableRoadSegmentIdsCache.TryGetValue(contour, out var editableRoadSegmentIds))
            {
                return editableRoadSegmentIds;
            }

            var roadSegments = await GetRoadSegments(contour, cancellationToken);
            var connectedRoadSegments = await GetRoadSegmentsConnectedOutsideOfContour(contour, roadSegments, cancellationToken);
            var candidates = roadSegments.Concat(connectedRoadSegments).ToList();

            // Which of these are V2 is answered from the segments already in hand rather than by querying for them
            // again: a V2 segment that takes part in no inwinning is compleet all the same.
            var inwinningsstatus = await _extractsDbContext.GetInwinningsstatus(
                candidates.Select(x => x.RoadSegmentId),
                SelectV2RoadSegmentIds(candidates),
                cancellationToken);

            editableRoadSegmentIds = EditableRoadSegments.Select(roadSegments, connectedRoadSegments, inwinningsstatus);
            _editableRoadSegmentIdsCache[contour] = editableRoadSegmentIds;
            return editableRoadSegmentIds;
        }

        // The road segments outside the contour that meet a road segment of the contour at a road node. A road segment
        // meeting one at a road node inside the contour is in the contour itself, so only the road nodes outside of it
        // are looked around - with a little room, as a vertex is not always exactly on its road node.
        private async Task<IReadOnlyList<RoadSegmentExtractItem>> GetRoadSegmentsConnectedOutsideOfContour(
            IPolygonal contour,
            IReadOnlyList<RoadSegmentExtractItem> roadSegments,
            CancellationToken cancellationToken)
        {
            const double RoadNodeToleranceInMeters = 1;

            var contourGeometry = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare((Geometry)contour);
            var roadNodesOutsideOfContour = (await GetRoadNodes(contour, cancellationToken))
                .Where(x => !contourGeometry.Intersects(x.Geometry.Value))
                .ToList();
            if (roadNodesOutsideOfContour.Count == 0)
            {
                return [];
            }

            var aroundRoadNodes = ((Geometry)contour).Factory
                .CreateMultiPoint(roadNodesOutsideOfContour.Select(x => x.Geometry.Value).ToArray())
                .Buffer(RoadNodeToleranceInMeters);

            var roadNodeIds = roadNodesOutsideOfContour.Select(x => x.RoadNodeId).ToHashSet();
            var roadSegmentIdsInContour = roadSegments.Select(x => x.RoadSegmentId).ToHashSet();

            return (await GetExtractDocuments(aroundRoadNodes)).RoadSegments
                .Where(x => !roadSegmentIdsInContour.Contains(x.RoadSegmentId))
                .Where(x => EditableRoadSegments.RoadNodeIdsOf(x).Any(roadNodeIds.Contains))
                .ToList();
        }

        private static Func<IReadOnlyCollection<RoadSegmentId>, CancellationToken, Task<IReadOnlyCollection<RoadSegmentId>>> SelectV2RoadSegmentIds(
            IReadOnlyList<RoadSegmentExtractItem> roadSegments)
        {
            var v2RoadSegmentIds = roadSegments.Where(x => x.IsV2).Select(x => x.RoadSegmentId).ToHashSet();

            return (roadSegmentIds, _) => Task.FromResult<IReadOnlyCollection<RoadSegmentId>>(roadSegmentIds
                .Where(v2RoadSegmentIds.Contains)
                .ToList());
        }

        public async Task<IReadOnlyList<RoadNodeExtractItem>> GetRoadNodes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            if (_roadNodesCache.TryGetValue(contour, out var roadNodes))
            {
                return roadNodes.ToList();
            }

            var result = (await GetExtractDocuments((Geometry)contour)).RoadNodes;
            _roadNodesCache[contour] = result;
            return result;
        }

        public async Task<IReadOnlyList<RoadSegmentExtractItem>> GetRoadSegments(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            if (_roadSegmentsCache.TryGetValue(contour, out var roadSegments))
            {
                return roadSegments.ToList();
            }

            var result = (await GetExtractDocuments((Geometry)contour)).RoadSegments;
            await SnapToRoadNodeGeometries(result, contour, cancellationToken);

            _roadSegmentsCache[contour] = result;
            return result;
        }

        private async Task SnapToRoadNodeGeometries(IReadOnlyList<RoadSegmentExtractItem> roadSegments, IPolygonal contour, CancellationToken cancellationToken)
        {
            var roadNodes = await GetRoadNodes(contour, cancellationToken);
            var roadNodeById = roadNodes.ToDictionary(n => n.RoadNodeId);

            foreach (var roadSegment in roadSegments)
            {
                var roadSegmentGeometry = roadSegment.Geometry.Value.GetSingleLineString();
                var factory = roadSegmentGeometry.Factory;
                var modified = false;

                if (roadSegment.StartNodeId is { } startNodeId && roadNodeById.TryGetValue(startNodeId, out var startNode))
                {
                    var nodePoint = startNode.Geometry.Value;
                    var firstCoord = roadSegmentGeometry.Coordinates[0];
                    if (firstCoord.X != nodePoint.X || firstCoord.Y != nodePoint.Y)
                    {
                        var coords = roadSegmentGeometry.Coordinates.ToArray();
                        coords[0] = new Coordinate(nodePoint.X, nodePoint.Y);
                        roadSegmentGeometry = factory.CreateLineString(coords);
                        modified = true;
                    }
                }

                if (roadSegment.EndNodeId is { } endNodeId && roadNodeById.TryGetValue(endNodeId, out var endNode))
                {
                    var nodePoint = endNode.Geometry.Value;
                    var lastCoord = roadSegmentGeometry.Coordinates[^1];
                    if (lastCoord.X != nodePoint.X || lastCoord.Y != nodePoint.Y)
                    {
                        var coords = roadSegmentGeometry.Coordinates.ToArray();
                        coords[^1] = new Coordinate(nodePoint.X, nodePoint.Y);
                        roadSegmentGeometry = factory.CreateLineString(coords);
                        modified = true;
                    }
                }

                if (modified)
                {
                    roadSegment.Geometry = RoadSegmentGeometry.Create(roadSegmentGeometry.ToMultiLineString()).RoundToCm();
                }
            }
        }

        public async Task<IReadOnlyList<GradeSeparatedJunctionExtractItem>> GetGradeSeparatedJunctions(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return (await GetExtractDocuments((Geometry)contour)).GradeSeparatedJunctions;
        }

        public async Task<IReadOnlyList<OrganizationRecordV2>> GetOrganizations(CancellationToken cancellationToken)
        {
            return await EntityFrameworkQueryableExtensions.ToListAsync(_editorContext.OrganizationsV2
                    .Where(x => x.IsMaintainer)
                    .OrderBy(x => x.Code), cancellationToken);
        }

        // The road nodes, road segments and grade separated junctions of the contour, loaded together the first time
        // any of them is asked for.
        private async Task<RoadNetworkExtractDocuments<RoadNodeExtractItem, RoadSegmentExtractItem, GradeSeparatedJunctionExtractItem>> GetExtractDocuments(Geometry geometry)
        {
            if (_documentsCache.TryGetValue(geometry, out var documents))
            {
                return documents;
            }

            documents = await _roadNetworkRepository.LoadForExtract<RoadNodeExtractItem, RoadSegmentExtractItem, GradeSeparatedJunctionExtractItem>(_session, geometry);
            _documentsCache[geometry] = documents;
            return documents;
        }
    }
}
