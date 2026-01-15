namespace RoadRegistry.Extracts.ZipArchiveWriters;

using NetTopologySuite.Geometries;
using RoadRegistry.Editor.Schema.Organizations;
using RoadRegistry.Extracts.Projections;

public interface IZipArchiveDataProvider
{
    Task<IReadOnlyList<RoadNodeExtractItem>> GetRoadNodes(
        IPolygonal contour,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RoadSegmentExtractItem>> GetRoadSegments(
        IPolygonal contour,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<GradeSeparatedJunctionExtractItem>> GetGradeSeparatedJunctions(
        IPolygonal contour,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizationRecordV2>> GetOrganizations(CancellationToken cancellationToken);
}
