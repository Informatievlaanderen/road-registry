namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2
{
    using Editor.Schema;
    using Editor.Schema.Organizations;
    using GradeSeparatedJunction;
    using Marten;
    using NetTopologySuite.Geometries;
    using RoadNetwork;
    using RoadNode;
    using RoadRegistry.Infrastructure.MartenDb;
    using RoadSegment;

    public class ZipArchiveDataProvider : IZipArchiveDataProvider
    {
        private readonly IDocumentSession _session;
        private readonly IRoadNetworkRepository _roadNetworkRepository;
        private readonly EditorContext _editorContext;
        private readonly Dictionary<Geometry, RoadNetworkIds> _ids = [];

        public ZipArchiveDataProvider(IDocumentSession session, IRoadNetworkRepository roadNetworkRepository, EditorContext editorContext)
        {
            _session = session;
            _roadNetworkRepository = roadNetworkRepository;
            _editorContext = editorContext;
        }

        public async Task<IReadOnlyList<RoadNode>> GetRoadNodes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            var ids = await GetUnderlyingRoadNetworkIds((Geometry)contour);

            return await _session.LoadManyAsync(ids.RoadNodeIds);
        }

        public async Task<IReadOnlyList<RoadSegment>> GetRoadSegments(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            var ids = await GetUnderlyingRoadNetworkIds((Geometry)contour);

            return await _session.LoadManyAsync(ids.RoadSegmentIds);
        }
        public async Task<IReadOnlyList<GradeSeparatedJunction>> GetGradeSeparatedJunctions(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            var ids = await GetUnderlyingRoadNetworkIds((Geometry)contour);

            return await _session.LoadManyAsync(ids.GradeSeparatedJunctionIds);
        }

        public async Task<IReadOnlyList<OrganizationRecordV2>> GetOrganizations(CancellationToken cancellationToken)
        {
            return await _editorContext.OrganizationsV2
                .Where(x => x.IsMaintainer)
                .OrderBy(x => x.Code)
                .ToListAsync(cancellationToken);
        }

        private async Task<RoadNetworkIds> GetUnderlyingRoadNetworkIds(Geometry geometry)
        {
            if (_ids.TryGetValue(geometry, out var roadNetworkIds))
            {
                return roadNetworkIds;
            }

            var ids = await _roadNetworkRepository.GetUnderlyingIds(_session, geometry);
            _ids[geometry] = ids;
            return ids;
        }
    }
}
