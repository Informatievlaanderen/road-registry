namespace RoadRegistry.BackOffice;

using System;
using System.Diagnostics.Contracts;
using System.Globalization;

public readonly struct CrabStreetnameId : IEquatable<CrabStreetnameId>
{
    private const int UnknownValue = -8;
    private const int NotApplicableValue = -9;
    public static readonly CrabStreetnameId Unknown = new(UnknownValue);
    public static readonly CrabStreetnameId NotApplicable = new(NotApplicableValue);
    private readonly int _value;

    public CrabStreetnameId(int value)
    {
        if (value != UnknownValue
            && value != NotApplicableValue
            && value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "The crab street name identifier must be greater than or equal to zero.");

        _value = value;
    }

    public static bool IsEmpty(CrabStreetnameId? value)
    {
        return value == null || value == UnknownValue || value == NotApplicableValue || value <= 0;
    }

    public static bool Accepts(int value)
    {
        return value == UnknownValue || value == NotApplicableValue || 0 <= value;
    }

    public static CrabStreetnameId? FromValue(int? value)
    {
        return value.HasValue
            ? new CrabStreetnameId(value.Value)
            : new CrabStreetnameId?();
    }

    [Pure]
    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(CrabStreetnameId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is CrabStreetnameId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public static bool operator ==(CrabStreetnameId left, CrabStreetnameId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CrabStreetnameId left, CrabStreetnameId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(CrabStreetnameId instance)
    {
        return instance._value;
    }
}
