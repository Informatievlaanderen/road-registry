namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Immutable;
using System.Linq;
using RoadRegistry.BackOffice;

public sealed record RoadSegmentAttributes : IEquatable<RoadSegmentAttributes>
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

    public bool Equals(RoadSegmentAttributes? other)
    {
        if (other is null)
        {
            return false;
        }

        return GeometryDrawMethod.Equals(other.GeometryDrawMethod)
               && AccessRestriction.Equals(other.AccessRestriction)
               && Category.Equals(other.Category)
               && Morphology.Equals(other.Morphology)
               && Status.Equals(other.Status)
               && StreetNameId.Equals(other.StreetNameId)
               && MaintenanceAuthorityId.Equals(other.MaintenanceAuthorityId)
               && SurfaceType.Equals(other.SurfaceType)
               && EuropeanRoadNumbers.SequenceEqual(other.EuropeanRoadNumbers)
               && NationalRoadNumbers.SequenceEqual(other.NationalRoadNumbers)
            ;
    }
}
