namespace RoadRegistry.Extracts.ZipArchiveWriters;

using NetTopologySuite.Geometries;
using RoadRegistry.Editor.Schema.Organizations;
using RoadRegistry.Extracts.Projections;

public interface IZipArchiveDataSession
{
    // Whether every road segment inside the contour has inwinningsstatus 'compleet'. A bijhouding extract is about
    // data the inwinning has finished with, so as soon as one segment is still 'nietGestart' or 'locked' the basis-
    // and werkbestanden stay empty and everything goes into the integratiedata instead.
    Task<bool> EverythingInContourIsCompleet(
        IPolygonal contour,
        CancellationToken cancellationToken);

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
