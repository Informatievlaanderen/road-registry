namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using RoadRegistry.Extensions;

public readonly struct RoadSegmentNumberedRoadOrdinal : IEquatable<RoadSegmentNumberedRoadOrdinal>, IDutchToString
{
    private const int UnknownValue = -8;
    public static readonly RoadSegmentNumberedRoadOrdinal Unknown = new(UnknownValue);

    private readonly int _value;

    private static readonly IDictionary<string, RoadSegmentNumberedRoadOrdinal> DutchNameMapping = new Dictionary<string, RoadSegmentNumberedRoadOrdinal>()
    {
        { "niet gekend", Unknown }
    };

    public RoadSegmentNumberedRoadOrdinal(int value)
    {
        if (value < 0 && value != UnknownValue) throw new ArgumentOutOfRangeException(nameof(value), value, $"The road segment numbered road ordinal must be greater than or equal to zero, or the value '{UnknownValue}' (Not Known)");

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value >= 0 || value == UnknownValue;
    }

    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(RoadSegmentNumberedRoadOrdinal other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentNumberedRoadOrdinal revision && Equals(revision);
    }

    public override int GetHashCode()
    {
        return _value;
    }

    public override string ToString()
    {
        return _value.ToString();
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

    public static bool operator ==(RoadSegmentNumberedRoadOrdinal left, RoadSegmentNumberedRoadOrdinal right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentNumberedRoadOrdinal left, RoadSegmentNumberedRoadOrdinal right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadSegmentNumberedRoadOrdinal instance)
    {
        return instance._value;
    }

    public static bool CanParseUsingDutchName(string value) => TryParseUsingDutchName(value, out _);

    public static RoadSegmentNumberedRoadOrdinal ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment numbered road ordinal.");
        return parsed;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentNumberedRoadOrdinal parsed)
    {
        if (DutchNameMapping.TryGetValue(value, out parsed))
        {
            return true;
        }

        if (int.TryParse(value, out var intValue) && Accepts(intValue))
        {
            parsed = new RoadSegmentNumberedRoadOrdinal(intValue);
            return true;
        }

        return false;
    }
}
