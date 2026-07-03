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
    using RoadRegistry.ScopedRoadNetwork;
    using RoadRegistry.ValueObjects;

    public class ZipArchiveDataSession : IZipArchiveDataSession
    {
        private readonly IDocumentSession _session;
        private readonly IRoadNetworkRepository _roadNetworkRepository;
        private readonly EditorContext _editorContext;

        private readonly Dictionary<Geometry, RoadNetworkIds> _idsCache = [];
        private readonly Dictionary<IPolygonal, IReadOnlyList<RoadNodeExtractItem>> _roadNodesCache = [];
        private readonly Dictionary<IPolygonal, IReadOnlyList<RoadSegmentExtractItem>> _roadSegmentsCache = [];

        public ZipArchiveDataSession(IDocumentSession session, IRoadNetworkRepository roadNetworkRepository, EditorContext editorContext)
        {
            _session = session;
            _roadNetworkRepository = roadNetworkRepository;
            _editorContext = editorContext;
        }

        public async Task<IReadOnlyList<RoadNodeExtractItem>> GetRoadNodes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            if (_roadNodesCache.TryGetValue(contour, out var roadNodes))
            {
                return roadNodes.ToList();
            }

            var ids = await GetUnderlyingRoadNetworkIds((Geometry)contour);

            var result = await _session.LoadManyAsync<RoadNodeExtractItem>(cancellationToken, ids.RoadNodeIds.Select(x => x.ToInt32()).ToArray());
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

            var ids = await GetUnderlyingRoadNetworkIds((Geometry)contour);

            var result = await _session.LoadManyAsync<RoadSegmentExtractItem>(cancellationToken, ids.RoadSegmentIds.Select(x => x.ToInt32()).ToArray());
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
            var ids = await GetUnderlyingRoadNetworkIds((Geometry)contour);

            return await _session.LoadManyAsync<GradeSeparatedJunctionExtractItem>(cancellationToken, ids.GradeSeparatedJunctionIds.Select(x => x.ToInt32()).ToArray());
        }

        public async Task<IReadOnlyList<OrganizationRecordV2>> GetOrganizations(CancellationToken cancellationToken)
        {
            return await EntityFrameworkQueryableExtensions.ToListAsync(_editorContext.OrganizationsV2
                    .Where(x => x.IsMaintainer)
                    .OrderBy(x => x.Code), cancellationToken);
        }

        private async Task<RoadNetworkIds> GetUnderlyingRoadNetworkIds(Geometry geometry)
        {
            if (_idsCache.TryGetValue(geometry, out var roadNetworkIds))
            {
                return roadNetworkIds;
            }

            var ids = await _roadNetworkRepository.GetUnderlyingIdsForExtract(_session, geometry);
            _idsCache[geometry] = ids;
            return ids;
        }
    }
}
