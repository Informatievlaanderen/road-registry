namespace RoadRegistry.BackOffice;

using System;
using Framework;

public readonly struct OrganizationId : IEquatable<OrganizationId>
{
    public const int MaxLength = 18;
    public static readonly OrganizationId Other = new("-7");
    public static readonly OrganizationId Unknown = new("-8");

    private readonly string _value;

    public OrganizationId(string value)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value), "The organization identifier must not be null or empty.");

        if (value.Length > MaxLength)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"The organization identifier must be {MaxLength} characters or less.");

        _value = value;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length <= MaxLength;
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
