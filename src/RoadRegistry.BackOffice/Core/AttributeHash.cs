namespace RoadRegistry.BackOffice.Core;

using System;
using System.Diagnostics.Contracts;

public readonly struct AttributeHash : IEquatable<AttributeHash>
{
    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
    public RoadSegmentMorphology Morphology { get; }
    public RoadSegmentStatus Status { get; }
    public CrabStreetNameId? LeftStreetNameId { get; }
    public CrabStreetNameId? RightStreetNameId { get; }
    public OrganizationId OrganizationId { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }

    public AttributeHash(
        RoadSegmentAccessRestriction accessRestriction,
        RoadSegmentCategory category,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        CrabStreetNameId? leftStreetNameId,
        CrabStreetNameId? rightStreetNameId,
        OrganizationId organizationId,
        RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        AccessRestriction = accessRestriction;
        Category = category;
        Morphology = morphology;
        Status = status;
        LeftStreetNameId = leftStreetNameId;
        RightStreetNameId = rightStreetNameId;
        OrganizationId = organizationId;
        GeometryDrawMethod = geometryDrawMethod;
    }

    [Pure]
    public AttributeHash With(RoadSegmentAccessRestriction value)
    {
        return new AttributeHash(value, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(RoadSegmentCategory value)
    {
        return new AttributeHash(AccessRestriction, value, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(RoadSegmentMorphology value)
    {
        return new AttributeHash(AccessRestriction, Category, value, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(RoadSegmentStatus value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, value, LeftStreetNameId, RightStreetNameId, OrganizationId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(OrganizationId value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, value, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(RoadSegmentGeometryDrawMethod value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, OrganizationId, value);
    }

    [Pure]
    public AttributeHash WithLeftSide(CrabStreetNameId? value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, value, RightStreetNameId, OrganizationId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash WithRightSide(CrabStreetNameId? value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, value, OrganizationId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash Without(CrabStreetNameId value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId == value ? null : LeftStreetNameId, RightStreetNameId == value ? null : RightStreetNameId, OrganizationId, GeometryDrawMethod);
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
               ;
    }

    public override bool Equals(object obj)
    {
        return obj is AttributeHash other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            AccessRestriction,
            Category,
            Morphology,
            Status,
            HashCode.Combine(LeftStreetNameId, 'L'),
            HashCode.Combine(RightStreetNameId, 'R'),
            OrganizationId,
            GeometryDrawMethod
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
