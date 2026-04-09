namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost
{
    using Editor.Schema;
    using Editor.Schema.GradeSeparatedJunctions;
    using Editor.Schema.Organizations;
    using Editor.Schema.RoadNodes;
    using Editor.Schema.RoadSegments;
    using Extensions;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Extensions;
    using RoadRegistry.Extracts.Schema;
    using RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;

    public interface IZipArchiveDataProvider
    {
        Task<bool> HasInwinningRoadSegment(IPolygonal contour, CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadNodeRecord>> GetRoadNodes(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentRecord>> GetRoadSegments(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<GradeSeparatedJunctionRecord>> GetGradeSeparatedJunctions(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentEuropeanRoadAttributeRecord>> GetRoadSegmentEuropeanRoadAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentNationalRoadAttributeRecord>> GetRoadSegmentNationalRoadAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentNumberedRoadAttributeRecord>> GetRoadSegmentNumberedRoadAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentLaneAttributeRecord>> GetRoadSegmentLaneAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentSurfaceAttributeRecord>> GetRoadSegmentSurfaceAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentWidthAttributeRecord>> GetRoadSegmentWidthAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<int> GetOrganizationsCount(CancellationToken cancellationToken);
        Task<IReadOnlyList<OrganizationRecordV2>> GetOrganizations(CancellationToken cancellationToken);
    }

    public class ZipArchiveDataProvider : IZipArchiveDataProvider
    {
        private readonly EditorContext _editorContext;
        private readonly ExtractsDbContext _extractsDbContext;
        private readonly Dictionary<IPolygonal, bool> _hasInwinningRoadSegmentCache = new();
        private readonly Dictionary<IPolygonal, IReadOnlyList<RoadSegmentRecord>> _getRoadSegmentCache = new();

        public ZipArchiveDataProvider(EditorContext editorContext, ExtractsDbContext extractsDbContext)
        {
            _editorContext = editorContext.ThrowIfNull();
            _extractsDbContext = extractsDbContext.ThrowIfNull();
        }

        public async Task<bool> HasInwinningRoadSegment(IPolygonal contour, CancellationToken cancellationToken)
        {
            if (_hasInwinningRoadSegmentCache.TryGetValue(contour, out var hasInwinning))
            {
                return hasInwinning;
            }

            var segments = await GetRoadSegments(
                contour, cancellationToken);

            hasInwinning = await _extractsDbContext.HasInwinningRoadSegments(segments.Select(x => new RoadSegmentId(x.Id)), cancellationToken);
            _hasInwinningRoadSegmentCache[contour] = hasInwinning;

            return hasInwinning;
        }

        public async Task<IReadOnlyList<RoadNodeRecord>> GetRoadNodes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.RoadNodes
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentRecord>> GetRoadSegments(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            if (_getRoadSegmentCache.TryGetValue(contour, out var roadSegments))
            {
                return roadSegments;
            }

            roadSegments = await _editorContext.RoadSegments
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
            _getRoadSegmentCache[contour] = roadSegments;
            return roadSegments;
        }

        public async Task<IReadOnlyList<GradeSeparatedJunctionRecord>> GetGradeSeparatedJunctions(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.GradeSeparatedJunctions
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentEuropeanRoadAttributeRecord>> GetRoadSegmentEuropeanRoadAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.RoadSegmentEuropeanRoadAttributes
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentNationalRoadAttributeRecord>> GetRoadSegmentNationalRoadAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.RoadSegmentNationalRoadAttributes
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentNumberedRoadAttributeRecord>> GetRoadSegmentNumberedRoadAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.RoadSegmentNumberedRoadAttributes
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentLaneAttributeRecord>> GetRoadSegmentLaneAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.RoadSegmentLaneAttributes
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentSurfaceAttributeRecord>> GetRoadSegmentSurfaceAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.RoadSegmentSurfaceAttributes
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentWidthAttributeRecord>> GetRoadSegmentWidthAttributes(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.RoadSegmentWidthAttributes
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<int> GetOrganizationsCount(CancellationToken cancellationToken)
        {
            return await _editorContext.OrganizationsV2
                .Where(x => x.IsMaintainer)
                .CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<OrganizationRecordV2>> GetOrganizations(CancellationToken cancellationToken)
        {
            return await _editorContext.OrganizationsV2
                .Where(x => x.IsMaintainer)
                .OrderBy(x => x.Code)
                .ToListAsync(cancellationToken);
        }
    }
}
