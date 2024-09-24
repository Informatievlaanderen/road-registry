namespace RoadRegistry.BackOffice;

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using Framework;

public readonly struct StreetNameLocalId : IEquatable<StreetNameLocalId>
{
    private const int UnknownValue = -8;
    private const int NotApplicableValue = -9;
    public static readonly StreetNameLocalId Unknown = new(UnknownValue);
    public static readonly StreetNameLocalId NotApplicable = new(NotApplicableValue);
    private readonly int _value;

    public StreetNameLocalId(int value)
    {
        if (value != UnknownValue
            && value != NotApplicableValue
            && value < 0) // allow 0 for backwards compatibility
            throw new ArgumentOutOfRangeException(nameof(value), value, "The street name local identifier must be greater than or equal to zero.");

        _value = value;
    }

    public static bool IsEmpty(StreetNameLocalId? value)
    {
        return value == null || value == UnknownValue || value == NotApplicableValue || value <= 0;
    }

    public static bool Accepts(int value)
    {
        return value == UnknownValue || value == NotApplicableValue || value > 0;
    }

    public static StreetNameLocalId? FromValue(int? value)
    {
        return value.HasValue && value != 0
            ? new StreetNameLocalId(value.Value)
            : new StreetNameLocalId?();
    }

    [Pure]
    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(StreetNameLocalId other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is StreetNameLocalId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public static bool operator ==(StreetNameLocalId left, StreetNameLocalId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StreetNameLocalId left, StreetNameLocalId right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(StreetNameLocalId instance)
    {
        return instance._value;
    }

    public static StreamName ToStreamName(StreetNameLocalId streetNameId)
    {
        return new StreamName(streetNameId.ToString()).WithPrefix("streetname-");
    }
}
