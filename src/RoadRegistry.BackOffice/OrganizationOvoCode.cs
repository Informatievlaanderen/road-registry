namespace RoadRegistry.BackOffice;

using System;
using System.Linq;

public readonly struct OrganizationOvoCode : IEquatable<OrganizationOvoCode>
{
    public const int Length = 9;
    public const int MaxDigitsValue = 999999;
    public const int MinDigitsValue = 1;
    private readonly string _value;
    public const string Prefix = "OVO";

    public OrganizationOvoCode(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(nameof(value), "The organization OVO-code must not be null or empty.");
        }

        if (!AcceptsValue(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The organization OVO-code must be {Length} characters and in the correct format.");
        }

        _value = value;
    }

    public OrganizationOvoCode(int value)
    {
        if (value > MaxDigitsValue || value < MinDigitsValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"The organization OVO-code digits must be between {MinDigitsValue} and {MaxDigitsValue}.");
        }

        _value = $"{Prefix}{value:000000}";
    }

    public static bool AcceptsValue(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length == Length && value.StartsWith(Prefix) && value.Substring(Prefix.Length).All(char.IsDigit);
    }

    public static OrganizationOvoCode? FromValue(string value)
    {
        return value is not null ? new OrganizationOvoCode(value) : null;
    }

    public bool Equals(OrganizationOvoCode other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is OrganizationOvoCode id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(OrganizationOvoCode instance)
    {
        return instance._value;
    }

    public static bool operator ==(OrganizationOvoCode left, OrganizationOvoCode right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(OrganizationOvoCode left, OrganizationOvoCode right)
    {
        return !left.Equals(right);
    }
}
