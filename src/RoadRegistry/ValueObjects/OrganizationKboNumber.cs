namespace RoadRegistry.BackOffice;

using System;
using System.Linq;

public readonly struct OrganizationKboNumber : IEquatable<OrganizationKboNumber>
{
    private readonly string _value;

    public const int Length = 10;

    public OrganizationKboNumber(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value), "The organization KBO-number must not be null or empty.");
        }

        if (!AcceptsValue(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The organization KBO-number must be {Length} characters and in the correct format.");
        }

        _value = value;
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length == Length && value.All(char.IsDigit);
    }

    public static OrganizationKboNumber? FromValue(string value)
    {
        return value is not null ? new OrganizationKboNumber(value) : null;
    }

    public bool Equals(OrganizationKboNumber other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is OrganizationKboNumber id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(OrganizationKboNumber instance)
    {
        return instance._value;
    }

    public static bool operator ==(OrganizationKboNumber left, OrganizationKboNumber right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OrganizationKboNumber left, OrganizationKboNumber right)
    {
        return !left.Equals(right);
    }
}
