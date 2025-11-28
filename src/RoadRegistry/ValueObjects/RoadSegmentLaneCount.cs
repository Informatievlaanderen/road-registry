namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using RoadRegistry.Extensions;

public readonly struct RoadSegmentLaneCount : IEquatable<RoadSegmentLaneCount>, IDutchToString
{
    private const int MinimumValue = 0;
    private const int MaximumValue = 10;
    private const int UnknownValue = -8;
    private const int NotApplicableValue = -9;
    public static readonly RoadSegmentLaneCount Minimum = new(MinimumValue);
    public static readonly RoadSegmentLaneCount Maximum = new(MaximumValue);
    public static readonly RoadSegmentLaneCount Unknown = new(UnknownValue);
    public static readonly RoadSegmentLaneCount NotApplicable = new(NotApplicableValue);
    private readonly int _value;

    private static readonly RoadSegmentLaneCount[] All =
        Array.Empty<RoadSegmentLaneCount>()
            .Concat(new[] { NotApplicable, Unknown })
            .Concat(Enumerable.Range(MinimumValue, MaximumValue - MinimumValue + 1).Select(value => new RoadSegmentLaneCount(value)))
            .ToArray();

    private static readonly IDictionary<string, RoadSegmentLaneCount> DutchNameMapping = new Dictionary<string, RoadSegmentLaneCount>()
    {
        { "niet gekend", Unknown },
        { "niet van toepassing", NotApplicable }
    };

    public RoadSegmentLaneCount(int value)
    {
        if (!Accepts(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The road segment lane count must be greater than or equal to {MinimumValue} and less than or equal to {MaximumValue}.");
        }

        _value = value;
    }

    public static bool Accepts(int value)
    {
        return value == UnknownValue || value == NotApplicableValue || (MinimumValue <= value && value <= MaximumValue);
    }

    [Pure]
    public int ToInt32()
    {
        return _value;
    }

    public bool Equals(RoadSegmentLaneCount other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentLaneCount revision && Equals(revision);
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

    public static bool operator ==(RoadSegmentLaneCount left, RoadSegmentLaneCount right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentLaneCount left, RoadSegmentLaneCount right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadSegmentLaneCount instance)
    {
        return instance._value;
    }

    public static bool CanParseUsingDutchName(string value) => TryParseUsingDutchName(value, out _);

    public static RoadSegmentLaneCount ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment lane count.");
        return parsed;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentLaneCount parsed)
    {
        if (DutchNameMapping.TryGetValue(value, out parsed))
        {
            return true;
        }

        parsed = Array.Find(All, candidate => candidate.ToString() == value);
        return parsed.ToString() == value;
    }
}
