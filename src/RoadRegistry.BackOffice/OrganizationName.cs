namespace RoadRegistry.BackOffice;
using System;
using Extensions;

public readonly struct OrganizationName : IEquatable<OrganizationName>
{
    public const int MaxLength = 64;
    private readonly string _value;

    public OrganizationName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value), "The organization name must not be null or empty.");
        }

        if (value.Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The organization name must be {MaxLength} characters or less.");
        }

        _value = value;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length <= MaxLength;
    }

    public bool Equals(OrganizationName other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is OrganizationName id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value;
    }

    public OrganizationName WithMaxLength(int length)
    {
        return new OrganizationName(_value.WithMaxLength(length));
    }

    public static OrganizationName WithoutExcessLength(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value), "The organization name must not be null or empty.");
        }

        return new OrganizationName(value.WithMaxLength(MaxLength));
    }

    public static implicit operator string(OrganizationName instance)
    {
        return instance._value;
    }

    public static bool operator ==(OrganizationName left, OrganizationName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OrganizationName left, OrganizationName right)
    {
        return !left.Equals(right);
    }
}
