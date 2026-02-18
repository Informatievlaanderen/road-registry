namespace RoadRegistry.RoadSegment.Changes;

using System;
using System.Collections.Generic;
using System.Linq;
using ScopedRoadNetwork;
using ValueObjects;

public sealed record AddRoadSegmentChange : IRoadNetworkChange, IEquatable<AddRoadSegmentChange>
{
    public required RoadSegmentId TemporaryId { get; init; }
    public RoadSegmentId? OriginalId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadSegmentGeometryDrawMethodV2 GeometryDrawMethod { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> Category { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2> Status { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType { get; init; }
    public required RoadSegmentDynamicAttributeValues<VehicleAccess> CarAccess { get; init; }
    public required RoadSegmentDynamicAttributeValues<VehicleAccess> BikeAccess { get; init; }
    public required RoadSegmentDynamicAttributeValues<bool> PedestrianAccess { get; init; }
    public IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; } = [];
    public IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; init; } = [];

    public bool Equals(AddRoadSegmentChange? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return TemporaryId.Equals(other.TemporaryId)
               && OriginalId.Equals(other.OriginalId)
               && Geometry.Equals(other.Geometry)
               && GeometryDrawMethod.Equals(other.GeometryDrawMethod)
               && AccessRestriction.Equals(other.AccessRestriction)
               && Category.Equals(other.Category)
               && Morphology.Equals(other.Morphology)
               && Status.Equals(other.Status)
               && StreetNameId.Equals(other.StreetNameId)
               && MaintenanceAuthorityId.Equals(other.MaintenanceAuthorityId)
               && SurfaceType.Equals(other.SurfaceType)
               && CarAccess.Equals(other.CarAccess)
               && BikeAccess.Equals(other.BikeAccess)
               && PedestrianAccess.Equals(other.PedestrianAccess)
               && EuropeanRoadNumbers.SequenceEqual(other.EuropeanRoadNumbers)
               && NationalRoadNumbers.SequenceEqual(other.NationalRoadNumbers);
    }
}
