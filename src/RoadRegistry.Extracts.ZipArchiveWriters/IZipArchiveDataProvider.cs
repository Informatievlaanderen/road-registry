namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2;

using Editor.Schema.Organizations;
using GradeSeparatedJunction;
using NetTopologySuite.Geometries;
using RoadNode;
using RoadRegistry.Extracts.Projections;
using RoadSegment;

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

    Task<IReadOnlyList<OrganizationRecordV2>> GetOrganizations(CancellationToken cancellationToken); //TODO-pr moet mee aangepast worden eens de orgs ook gemigreerd zijn
}
