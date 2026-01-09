namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2
{
    using Editor.Schema;
    using Editor.Schema.Organizations;
    using Marten;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Extracts.Projections;
    using ScopedRoadNetwork;

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

        public async Task<IReadOnlyList<RoadNodeExtractItem>> GetRoadNodes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            var ids = await GetUnderlyingRoadNetworkIds((Geometry)contour);

            return await _session.LoadManyAsync<RoadNodeExtractItem>(cancellationToken, ids.RoadNodeIds.Select(x => x.ToInt32()).ToArray());
        }

        public async Task<IReadOnlyList<RoadSegmentExtractItem>> GetRoadSegments(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            var ids = await GetUnderlyingRoadNetworkIds((Geometry)contour);

            return await _session.LoadManyAsync<RoadSegmentExtractItem>(cancellationToken, ids.RoadSegmentIds.Select(x => x.ToInt32()).ToArray());
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
