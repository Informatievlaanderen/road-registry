namespace RoadRegistry.BackOffice;

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

public readonly struct CrabStreetNameId : IEquatable<CrabStreetNameId>
{
    private const int UnknownValue = -8;
    private const int NotApplicableValue = -9;
    public static readonly CrabStreetNameId Unknown = new(UnknownValue);
    public static readonly CrabStreetNameId NotApplicable = new(NotApplicableValue);
    private readonly int _value;

    public CrabStreetNameId(int value)
    {
        if (value != UnknownValue
            && value != NotApplicableValue
            && value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "The crab street name identifier must be greater than or equal to zero.");

        _value = value;
    }

    public static bool IsEmpty(CrabStreetNameId? value)
    {
        return value == null || value == UnknownValue || value == NotApplicableValue || value <= 0;
    }

    public static bool Accepts(int value)
    {
        return value == UnknownValue || value == NotApplicableValue || 0 <= value;
    }

    public static CrabStreetNameId? FromValue(int? value)
    {
        return value.HasValue
            ? new CrabStreetNameId(value.Value)
            : new CrabStreetNameId?();
    }

    [Pure]
    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(CrabStreetNameId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is CrabStreetNameId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public static bool operator ==(CrabStreetNameId left, CrabStreetNameId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CrabStreetNameId left, CrabStreetNameId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(CrabStreetNameId instance)
    {
        return instance._value;
    }
}
