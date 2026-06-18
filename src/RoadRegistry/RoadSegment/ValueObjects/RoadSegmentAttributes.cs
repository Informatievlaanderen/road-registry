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
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> CarTrafficDirection { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> BikeTrafficDirection { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> PedestrianTrafficDirection { get; init; }
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
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> carTrafficDirection,
        RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> bikeTrafficDirection,
        RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> pedestrianTrafficDirection,
        ICollection<EuropeanRoadNumber> europeanRoadNumbers,
        ICollection<NationalRoadNumber> nationalRoadNumbers
    )
    {
        GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Parse(geometryDrawMethod);
        AccessRestriction = accessRestriction;
        Category = category;
        Morphology = morphology;
        StreetNameId = streetNameId;
        MaintenanceAuthorityId = maintenanceAuthorityId;
        SurfaceType = surfaceType;
        CarTrafficDirection = carTrafficDirection;
        BikeTrafficDirection = bikeTrafficDirection;
        PedestrianTrafficDirection = pedestrianTrafficDirection;
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
               && StreetNameId.Equals(other.StreetNameId)
               && MaintenanceAuthorityId.Equals(other.MaintenanceAuthorityId)
               && SurfaceType.Equals(other.SurfaceType)
               && CarTrafficDirection.Equals(other.CarTrafficDirection)
               && BikeTrafficDirection.Equals(other.BikeTrafficDirection)
               && PedestrianTrafficDirection.Equals(other.PedestrianTrafficDirection)
               && EuropeanRoadNumbers.SequenceEqual(other.EuropeanRoadNumbers)
               && NationalRoadNumbers.SequenceEqual(other.NationalRoadNumbers)
            ;
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(GeometryDrawMethod);
        hashCode.Add(AccessRestriction);
        hashCode.Add(Category);
        hashCode.Add(Morphology);
        hashCode.Add(StreetNameId);
        hashCode.Add(MaintenanceAuthorityId);
        hashCode.Add(SurfaceType);
        hashCode.Add(CarTrafficDirection);
        hashCode.Add(BikeTrafficDirection);
        hashCode.Add(PedestrianTrafficDirection);
        hashCode.Add(EuropeanRoadNumbers);
        hashCode.Add(NationalRoadNumbers);
        return hashCode.ToHashCode();
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
