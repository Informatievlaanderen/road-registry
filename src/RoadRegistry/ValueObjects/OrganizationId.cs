namespace RoadRegistry.ValueObjects;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.Extensions;

public readonly struct OrganizationId : IEquatable<OrganizationId>
{
    public const int MaxLength = 18;
    public static readonly OrganizationId Other = new("-7");
    public static readonly OrganizationId Unknown = new("-8");
    public static readonly OrganizationId DigitaalVlaanderen = new(Organisation.DigitaalVlaanderen.ToString());

    private readonly string _value;

    public OrganizationId(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value), "The organization identifier must not be null or empty.");
        }

        if (value.ContainsWhitespace())
        {
            throw new ArgumentException("The organization identifier must not contain whitespace.", nameof(value));
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The organization identifier must be {MaxLength} characters or less.");
        }

        _value = value;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.ContainsWhitespace() && value.Length <= MaxLength;
    }

    public static bool IsSystemValue(OrganizationId value)
    {
        return value == Other || value == Unknown;
    }

    public static OrganizationId? FromValue(string? value)
    {
        return value is not null
            ? new OrganizationId(value)
            : null;
    }

    public bool Equals(OrganizationId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is OrganizationId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(OrganizationId instance)
    {
        return instance.ToString();
    }

    public static bool operator ==(OrganizationId left, OrganizationId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OrganizationId left, OrganizationId right)
    {
        return !left.Equals(right);
    }

    public static StreamName ToStreamName(OrganizationId organizationId)
    {
        return new StreamName(organizationId.ToString()).WithPrefix("organization-");
    }
}
