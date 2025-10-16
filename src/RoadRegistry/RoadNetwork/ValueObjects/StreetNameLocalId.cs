namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Framework;

public readonly struct StreetNameLocalId : IEquatable<StreetNameLocalId>, IDutchToString
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
        return value is UnknownValue or NotApplicableValue or > 0;
    }

    public static StreetNameLocalId? FromValue(int? value)
    {
        return value.HasValue && value != 0
            ? new StreetNameLocalId(value.Value)
            : new StreetNameLocalId?();
    }

    private static readonly IDictionary<string, StreetNameLocalId> DutchNameMapping = new Dictionary<string, StreetNameLocalId>()
    {
        { "niet gekend", Unknown },
        { "niet van toepassing", NotApplicable }
    };

    [Pure]
    public int ToInt32()
    {
        return _value;
    }

    public static StreetNameLocalId ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed))
            throw new FormatException($"The value {value} is not a well known street name local id.");
        return parsed;
    }

    public static bool TryParseUsingDutchName(string value, out StreetNameLocalId parsed)
    {
        if (DutchNameMapping.TryGetValue(value, out parsed))
        {
            return true;
        }

        if (int.TryParse(value, out var valueAsInt) && Accepts(valueAsInt))
        {
            parsed = new StreetNameLocalId(valueAsInt);
            return true;
        }

        return false;
    }

    public string ToDutchString()
    {
        foreach (var item in DutchNameMapping)
        {
            if (item.Value == this)
            {
                return item.Key;
            }
        }

        return ToString();
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
