namespace RoadRegistry.RoadSegment.ValueObjects;//TODO-pr fix namespace

using System;
using System.Diagnostics.Contracts;
using BackOffice;

public readonly struct AttributeHash : IEquatable<AttributeHash>
{
    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
    public RoadSegmentMorphology Morphology { get; }
    public RoadSegmentStatus Status { get; }
    public StreetNameLocalId? LeftStreetNameId { get; }
    public StreetNameLocalId? RightStreetNameId { get; }
    public OrganizationId MaintenanceAuthorityId { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }

    public AttributeHash(
        RoadSegmentAccessRestriction accessRestriction,
        RoadSegmentCategory category,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        StreetNameLocalId? leftStreetNameId,
        StreetNameLocalId? rightStreetNameId,
        OrganizationId maintenanceAuthorityId,
        RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        AccessRestriction = accessRestriction;
        Category = category;
        Morphology = morphology;
        Status = status;
        LeftStreetNameId = leftStreetNameId;
        RightStreetNameId = rightStreetNameId;
        MaintenanceAuthorityId = maintenanceAuthorityId;
        GeometryDrawMethod = geometryDrawMethod;
    }

    [Pure]
    public AttributeHash With(RoadSegmentAccessRestriction value)
    {
        return new AttributeHash(value, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, MaintenanceAuthorityId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(RoadSegmentCategory value)
    {
        return new AttributeHash(AccessRestriction, value, Morphology, Status, LeftStreetNameId, RightStreetNameId, MaintenanceAuthorityId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(RoadSegmentMorphology value)
    {
        return new AttributeHash(AccessRestriction, Category, value, Status, LeftStreetNameId, RightStreetNameId, MaintenanceAuthorityId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(RoadSegmentStatus value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, value, LeftStreetNameId, RightStreetNameId, MaintenanceAuthorityId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(OrganizationId value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, value, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash With(RoadSegmentGeometryDrawMethod value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, RightStreetNameId, MaintenanceAuthorityId, value);
    }

    [Pure]
    public AttributeHash WithLeftSide(StreetNameLocalId? value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, value, RightStreetNameId, MaintenanceAuthorityId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash WithRightSide(StreetNameLocalId? value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId, value, MaintenanceAuthorityId, GeometryDrawMethod);
    }

    [Pure]
    public AttributeHash Without(StreetNameLocalId value)
    {
        return new AttributeHash(AccessRestriction, Category, Morphology, Status, LeftStreetNameId == value ? null : LeftStreetNameId, RightStreetNameId == value ? null : RightStreetNameId, MaintenanceAuthorityId, GeometryDrawMethod);
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
               && MaintenanceAuthorityId.Equals(other.MaintenanceAuthorityId)
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
            MaintenanceAuthorityId,
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
