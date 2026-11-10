namespace RoadRegistry.Extracts.ZipArchiveWriters;

using NetTopologySuite.Geometries;
using RoadRegistry.Editor.Schema.Organizations;
using RoadRegistry.Extracts.Projections;

public interface IZipArchiveDataSession
{
    // The road segments inside the contour a bijhouding extract hands out as bewerkbare data: those with
    // inwinningsstatus 'compleet' that share no road node with a road segment that is still 'nietGestart' or
    // 'locked', inside the contour or not. The other road segments inside the contour go into the integratiedata.
    Task<IReadOnlySet<RoadSegmentId>> GetEditableRoadSegmentIds(
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
