namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost
{
    using System.Data;
    using Editor.Schema;
    using Editor.Schema.GradeSeparatedJunctions;
    using Editor.Schema.RoadNodes;
    using Editor.Schema.RoadSegments;
    using Extensions;
    using Extracts.Dbase.Organizations;
    using Extracts.Dbase.RoadSegments;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using NetTopologySuite.Geometries;

    public interface IZipArchiveDataProvider
    {
        Task<IDbContextTransaction> BeginTransaction(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadNodeRecord>> GetRoadNodes(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadNodeRecord>> GetRoadNodesInIntegrationBuffer(
            IEnumerable<Geometry> integrationBufferedSegmentsGeometries,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentRecord>> GetRoadSegments(
            IPolygonal contour,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<RoadSegmentRecord>> GetRoadSegmentsInIntegrationBuffer(
            IEnumerable<Geometry> integrationBufferedSegmentsGeometries,
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
        Task<IReadOnlyList<OrganizationRecord>> GetOrganizations(CancellationToken cancellationToken);
    }

    public class ZipArchiveDataProvider : IZipArchiveDataProvider
    {
        private readonly EditorContext _editorContext;

        // todo-Rik do we want inject the editor context or editor context factory?
        public ZipArchiveDataProvider(EditorContext editorContext)
        {
            _editorContext = editorContext;
        }

        public async Task<IDbContextTransaction> BeginTransaction(IsolationLevel isolationLevel, CancellationToken cancellationToken)
        {
            return await _editorContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
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

        public async Task<IReadOnlyList<RoadNodeRecord>> GetRoadNodesInIntegrationBuffer(
            IEnumerable<Geometry> integrationBufferedSegmentsGeometries,
            CancellationToken cancellationToken)
        {
            // todo-rik perform below in calling method?
            var integrationBufferedContourGeometry = WellKnownGeometryFactories.Default
                .BuildGeometry(integrationBufferedSegmentsGeometries)
                .ConvexHull();

            return await _editorContext.RoadNodes
                .InsideContour((IPolygonal)integrationBufferedContourGeometry)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentRecord>> GetRoadSegments(
            IPolygonal contour,
            CancellationToken cancellationToken)
        {
            return await _editorContext.RoadSegments
                .ToListWithPolygonials(contour,
                    (dbSet, polygon) => dbSet.InsideContour(polygon),
                    x => x.Id,
                    cancellationToken);
        }

        public async Task<IReadOnlyList<RoadSegmentRecord>> GetRoadSegmentsInIntegrationBuffer(
            IEnumerable<Geometry> integrationBufferedSegmentsGeometries,
            CancellationToken cancellationToken)
        {
            var integrationBufferedContourGeometry = WellKnownGeometryFactories.Default
                .BuildGeometry(integrationBufferedSegmentsGeometries)
                .ConvexHull();

            return await _editorContext.RoadSegments
                .InsideContour((IPolygonal)integrationBufferedContourGeometry)
                .ToListAsync(cancellationToken);
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
            return await _editorContext.Organizations.CountAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<OrganizationRecord>> GetOrganizations(CancellationToken cancellationToken)
        {
            return await _editorContext.Organizations
                .OrderBy(x => x.SortableCode)
                .ToListAsync(cancellationToken);
        }
    }
}
