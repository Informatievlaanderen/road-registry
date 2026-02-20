namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public sealed record RoadSegmentAttributes : IEquatable<RoadSegmentAttributes>
{
    public RoadSegmentGeometryDrawMethodV2 GeometryDrawMethod { get; init; }
    public RoadSegmentStatusV2 Status { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType { get; init; }
    public RoadSegmentDynamicAttributeValues<bool> CarAccessForward { get; init; }
    public RoadSegmentDynamicAttributeValues<bool> CarAccessBackward { get; init; }
    public RoadSegmentDynamicAttributeValues<bool> BikeAccessForward { get; init; }
    public RoadSegmentDynamicAttributeValues<bool> BikeAccessBackward { get; init; }
    public RoadSegmentDynamicAttributeValues<bool> PedestrianAccess { get; init; }
    public ImmutableList<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public ImmutableList<NationalRoadNumber> NationalRoadNumbers { get; init; }

    public RoadSegmentAttributes()
    {
    }

    [JsonConstructor]
    protected RoadSegmentAttributes(
        string geometryDrawMethod,
        string status,
        RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> accessRestriction,
        RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> category,
        RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> morphology,
        RoadSegmentDynamicAttributeValues<StreetNameLocalId> streetNameId,
        RoadSegmentDynamicAttributeValues<OrganizationId> maintenanceAuthorityId,
        RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> surfaceType,
        RoadSegmentDynamicAttributeValues<bool> carAccessForward,
        RoadSegmentDynamicAttributeValues<bool> carAccessBackward,
        RoadSegmentDynamicAttributeValues<bool> bikeAccessForward,
        RoadSegmentDynamicAttributeValues<bool> bikeAccessBackward,
        RoadSegmentDynamicAttributeValues<bool> pedestrianAccess,
        ICollection<EuropeanRoadNumber> europeanRoadNumbers,
        ICollection<NationalRoadNumber> nationalRoadNumbers
    )
    {
        GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Parse(geometryDrawMethod);
        Status = RoadSegmentStatusV2.Parse(status);
        AccessRestriction = accessRestriction;
        Category = category;
        Morphology = morphology;
        StreetNameId = streetNameId;
        MaintenanceAuthorityId = maintenanceAuthorityId;
        SurfaceType = surfaceType;
        CarAccessForward = carAccessForward;
        CarAccessBackward = carAccessBackward;
        BikeAccessForward = bikeAccessForward;
        BikeAccessBackward = bikeAccessBackward;
        PedestrianAccess = pedestrianAccess;
        EuropeanRoadNumbers = europeanRoadNumbers.ToImmutableList();
        NationalRoadNumbers = nationalRoadNumbers.ToImmutableList();
    }

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
               && CarAccessForward.Equals(other.CarAccessForward)
               && CarAccessBackward.Equals(other.CarAccessBackward)
               && BikeAccessForward.Equals(other.BikeAccessForward)
               && BikeAccessBackward.Equals(other.BikeAccessBackward)
               && PedestrianAccess.Equals(other.PedestrianAccess)
               && EuropeanRoadNumbers.SequenceEqual(other.EuropeanRoadNumbers)
               && NationalRoadNumbers.SequenceEqual(other.NationalRoadNumbers)
            ;
    }

    public bool EqualsOnlyNonDynamicAttributes(RoadSegmentAttributes? other)
    {
        if (other is null)
        {
            return false;
        }

        return GeometryDrawMethod.Equals(other.GeometryDrawMethod)
               && Status.Equals(other.Status)
               && EuropeanRoadNumbers.SequenceEqual(other.EuropeanRoadNumbers)
               && NationalRoadNumbers.SequenceEqual(other.NationalRoadNumbers)
            ;
    }
}
