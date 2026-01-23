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
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2> Status { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType { get; init; }
    public RoadSegmentDynamicAttributeValues<VehicleAccess> CarAccess { get; init; }
    public RoadSegmentDynamicAttributeValues<VehicleAccess> BikeAccess { get; init; }
    public RoadSegmentDynamicAttributeValues<bool> PedestrianAccess { get; init; }
    public ImmutableList<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public ImmutableList<NationalRoadNumber> NationalRoadNumbers { get; init; }

    public RoadSegmentAttributes()
    {
    }

    [JsonConstructor]
    protected RoadSegmentAttributes(
        string geometryDrawMethod,
        RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> accessRestriction,
        RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> category,
        RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> morphology,
        RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2> status,
        RoadSegmentDynamicAttributeValues<StreetNameLocalId> streetNameId,
        RoadSegmentDynamicAttributeValues<OrganizationId> maintenanceAuthorityId,
        RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> surfaceType,
        RoadSegmentDynamicAttributeValues<VehicleAccess> carAccess,
        RoadSegmentDynamicAttributeValues<VehicleAccess> bikeAccess,
        RoadSegmentDynamicAttributeValues<bool> pedestrianAccess,
        ICollection<EuropeanRoadNumber> europeanRoadNumbers,
        ICollection<NationalRoadNumber> nationalRoadNumbers
    )
    {
        GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Parse(geometryDrawMethod);
        AccessRestriction = accessRestriction;
        Category = category;
        Morphology = morphology;
        Status = status;
        StreetNameId = streetNameId;
        MaintenanceAuthorityId = maintenanceAuthorityId;
        SurfaceType = surfaceType;
        CarAccess = carAccess;
        BikeAccess = bikeAccess;
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
               && EuropeanRoadNumbers.SequenceEqual(other.EuropeanRoadNumbers)
               && NationalRoadNumbers.SequenceEqual(other.NationalRoadNumbers)
            ;
    }
}

public enum VehicleAccess
{
    Forward,
    Backward,
    BiDirectional
}
