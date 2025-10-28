namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Immutable;
using RoadRegistry.BackOffice;

public sealed class RoadSegmentAttributes
{
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction> AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategory> Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphology> Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentStatus> Status { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType> SurfaceType { get; init; }
    public ImmutableList<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public ImmutableList<NationalRoadNumber> NationalRoadNumbers { get; init; }

    public bool Equals(RoadSegmentAttributes other)
    {
        //TODO-pr implement equality check, taking dynamic attributes into account
        throw new NotImplementedException();
    }
}
