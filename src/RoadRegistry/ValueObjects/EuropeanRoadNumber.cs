namespace RoadRegistry.ValueObjects;

using System;

public sealed class EuropeanRoadNumber : IEquatable<EuropeanRoadNumber>, IDutchToString, IComparable<EuropeanRoadNumber>
{
    public static readonly EuropeanRoadNumber E17 = new(nameof(E17));
    public static readonly EuropeanRoadNumber E19 = new(nameof(E19));
    public static readonly EuropeanRoadNumber E25 = new(nameof(E25));
    public static readonly EuropeanRoadNumber E313 = new(nameof(E313));
    public static readonly EuropeanRoadNumber E314 = new(nameof(E314));
    public static readonly EuropeanRoadNumber E34 = new(nameof(E34));
    public static readonly EuropeanRoadNumber E40 = new(nameof(E40));
    public static readonly EuropeanRoadNumber E403 = new(nameof(E403));
    public static readonly EuropeanRoadNumber E411 = new(nameof(E411));
    public static readonly EuropeanRoadNumber E429 = new(nameof(E429));

    public static readonly EuropeanRoadNumber[] All =
    {
        E17, E19, E25, E313, E314, E34,
        E40, E403, E411, E429
    };

    private readonly string _value;

    private EuropeanRoadNumber(string value)
    {
        _value = value;
    }

    public static bool CanParse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return Array.Find(All, candidate => candidate._value == value) != null;
    }

    public bool Equals(EuropeanRoadNumber? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is EuropeanRoadNumber type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static EuropeanRoadNumber Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known european road number.");
        return parsed;
    }

    public override string ToString()
    {
        return _value;
    }

    public string ToDutchString()
    {
        return _value;
    }

    public static implicit operator string?(EuropeanRoadNumber? instance)
    {
        return instance?.ToString();
    }

    public int CompareTo(EuropeanRoadNumber other)
    {
        return string.CompareOrdinal(_value, other._value);
    }

    public static bool TryParse(string value, out EuropeanRoadNumber parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool operator ==(EuropeanRoadNumber left, EuropeanRoadNumber right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EuropeanRoadNumber left, EuropeanRoadNumber right)
    {
        return !Equals(left, right);
    }

    public static bool operator <(EuropeanRoadNumber left, EuropeanRoadNumber right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(EuropeanRoadNumber left, EuropeanRoadNumber right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(EuropeanRoadNumber left, EuropeanRoadNumber right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(EuropeanRoadNumber left, EuropeanRoadNumber right)
    {
        return left.CompareTo(right) >= 0;
    }
}
