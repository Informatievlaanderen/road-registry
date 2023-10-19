namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

public readonly struct AttributeHash : IEquatable<AttributeHash>
{
    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
    public RoadSegmentMorphology Morphology { get; }
    public RoadSegmentStatus Status { get; }
    public CrabStreetnameId? LeftStreetNameId { get; }
    public CrabStreetnameId? RightStreetNameId { get; }
    public OrganizationId OrganizationId { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public IReadOnlyCollection<BackOffice.RoadSegmentLaneAttribute> Lanes { get; }
    public IReadOnlyCollection<BackOffice.RoadSegmentSurfaceAttribute> Surfaces { get; }
    public IReadOnlyCollection<BackOffice.RoadSegmentWidthAttribute> Widths { get; }

    public AttributeHash(
        RoadSegmentAccessRestriction accessRestriction,
        RoadSegmentCategory category,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        CrabStreetnameId? leftStreetNameId,
        CrabStreetnameId? rightStreetNameId,
        OrganizationId organizationId,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        IReadOnlyCollection<BackOffice.RoadSegmentLaneAttribute> lanes,
        IReadOnlyCollection<BackOffice.RoadSegmentSurfaceAttribute> surfaces,
        IReadOnlyCollection<BackOffice.RoadSegmentWidthAttribute> widths)
    {
        AccessRestriction = accessRestriction;
        Category = category;
        Morphology = morphology;
        Status = status;
        LeftStreetNameId = leftStreetNameId;
        RightStreetNameId = rightStreetNameId;
        OrganizationId = organizationId;
        GeometryDrawMethod = geometryDrawMethod;
        Lanes = lanes;
        Surfaces = surfaces;
        Widths = widths;
    }

    [Pure]
    public AttributeHash With(RoadSegmentAccessRestriction value)
    {
        return new AttributeHash(value, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod, Lanes, Surfaces, Widths);
    }

    [Pure]
    public AttributeHash With(RoadSegmentCategory value)
    {
        return new AttributeHash(AccessRestriction, value, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod, Lanes, Surfaces, Widths);
    }

    [Pure]
    public AttributeHash With(RoadSegmentMorphology value)
    {
        return new AttributeHash(AccessRestriction, Category, value, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod, Lanes, Surfaces, Widths);
    }

    [Pure]
    public AttributeHash With(RoadSegmentStatus value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, value, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod, Lanes, Surfaces, Widths);
    }

    [Pure]
    public AttributeHash With(OrganizationId value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, value, GeometryDrawMethod, Lanes, Surfaces, Widths);
    }

    [Pure]
    public AttributeHash With(RoadSegmentGeometryDrawMethod value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, value, Lanes, Surfaces, Widths);
    }
    
    [Pure]
    public AttributeHash With(IReadOnlyCollection<BackOffice.RoadSegmentLaneAttribute> lanes)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod, lanes, Surfaces, Widths);
    }

    [Pure]
    public AttributeHash With(IReadOnlyCollection<BackOffice.RoadSegmentSurfaceAttribute> surfaces)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod, Lanes, surfaces, Widths);
    }

    [Pure]
    public AttributeHash With(IReadOnlyCollection<BackOffice.RoadSegmentWidthAttribute> widths)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod, Lanes, Surfaces, widths);
    }

    [Pure]
    public AttributeHash WithLeftSide(CrabStreetnameId? value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, value, RightStreetNameId, OrganizationId, GeometryDrawMethod, Lanes, Surfaces, Widths);
    }

    [Pure]
    public AttributeHash WithRightSide(CrabStreetnameId? value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, value, OrganizationId, GeometryDrawMethod, Lanes, Surfaces, Widths);
    }

    [Pure]
    public bool Equals(AttributeHash other)
    {
        return Equals(AccessRestriction, other.AccessRestriction)
               && Equals(Category, other.Category)
               && Equals(Morphology, other.Morphology)
               && Equals(Status, other.Status)
               && LeftStreetNameId.Equals(other.LeftStreetNameId)
               && RightStreetNameId.Equals(other.RightStreetNameId)
               && OrganizationId.Equals(other.OrganizationId)
               && GeometryDrawMethod.Equals(other.GeometryDrawMethod)
               && Lanes.Equals(other.Lanes)
               && Surfaces.Equals(other.Surfaces)
               && Widths.Equals(other.Widths)
               ;
    }

    public override bool Equals(object obj)
    {
        return obj is AttributeHash other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCode.Combine(
                AccessRestriction,
                Category,
                Morphology,
                Status,
                HashCode.Combine(LeftStreetNameId, 'L'),
                HashCode.Combine(RightStreetNameId, 'R'),
                OrganizationId,
                GeometryDrawMethod
            ),
            HashCode.Combine(
                Lanes,
                Surfaces,
                Widths
            )
        );
    }

    public override string ToString()
    {
        return GetHashCode().ToString();
    }

    public static bool operator ==(AttributeHash left, AttributeHash right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AttributeHash left, AttributeHash right)
    {
        return !left.Equals(right);
    }
}
