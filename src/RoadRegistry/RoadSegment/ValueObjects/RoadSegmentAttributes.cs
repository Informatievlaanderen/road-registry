namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public sealed record RoadSegmentAttributes : IEquatable<RoadSegmentAttributes>
{
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction> AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategory> Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphology> Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2> Status { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType> SurfaceType { get; init; }
    public ImmutableList<EuropeanRoadNumber> EuropeanRoadNumbers { get; init; }
    public ImmutableList<NationalRoadNumber> NationalRoadNumbers { get; init; }

    public RoadSegmentAttributes()
    {
    }

    [JsonConstructor]
    protected RoadSegmentAttributes(
        string geometryDrawMethod,
        RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction> accessRestriction,
        RoadSegmentDynamicAttributeValues<RoadSegmentCategory> category,
        RoadSegmentDynamicAttributeValues<RoadSegmentMorphology> morphology,
        RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2> status,
        RoadSegmentDynamicAttributeValues<StreetNameLocalId> streetNameId,
        RoadSegmentDynamicAttributeValues<OrganizationId> maintenanceAuthorityId,
        RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType> surfaceType,
        ICollection<EuropeanRoadNumber> europeanRoadNumbers,
        ICollection<NationalRoadNumber> nationalRoadNumbers
    )
    {
        GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(geometryDrawMethod);
        AccessRestriction = accessRestriction;
        Category = category;
        Morphology = morphology;
        Status = status;
        StreetNameId = streetNameId;
        MaintenanceAuthorityId = maintenanceAuthorityId;
        SurfaceType = surfaceType;
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
