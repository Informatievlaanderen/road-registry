namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

public readonly struct RoadSegmentWidth : IEquatable<RoadSegmentWidth>, IDutchToString
{
    private const int MinimumValue = 0;
    private const int MaximumValue = 50;
    private const int UnknownValue = -8;
    private const int NotApplicableValue = -9;
    public static readonly RoadSegmentWidth Unknown = new(UnknownValue);
    public static readonly RoadSegmentWidth NotApplicable = new(NotApplicableValue);
    public static readonly RoadSegmentWidth Minimum = new(MinimumValue);
    public static readonly RoadSegmentWidth Maximum = new(MaximumValue);
    private readonly int _value;

    private static readonly RoadSegmentWidth[] All =
            Array.Empty<RoadSegmentWidth>()
                .Concat(new[] { NotApplicable, Unknown })
                .Concat(Enumerable.Range(MinimumValue, MaximumValue - MinimumValue + 1).Select(value => new RoadSegmentWidth(value)))
                .ToArray();

    private static readonly IDictionary<string, RoadSegmentWidth> DutchNameMapping = new Dictionary<string, RoadSegmentWidth>()
    {
        { "niet gekend", Unknown },
        { "niet van toepassing", NotApplicable }
    };

    public RoadSegmentWidth(int value)
    {
        if (!Accepts(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The road segment width must be greater than or equal to {MinimumValue} and less than or equal to {MaximumValue}, or {UnknownValue} (unknown) or {NotApplicableValue} (not applicable).");
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

    public bool Equals(RoadSegmentWidth other)
    {
        return _value == other._value;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentWidth revision && Equals(revision);
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

    public static bool operator ==(RoadSegmentWidth left, RoadSegmentWidth right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoadSegmentWidth left, RoadSegmentWidth right)
    {
        return !left.Equals(right);
    }

    public static implicit operator int(RoadSegmentWidth instance)
    {
        return instance._value;
    }

    public static bool CanParseUsingDutchName(string value) => TryParseUsingDutchName(value, out _);

    public static RoadSegmentWidth ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment width.");
        return parsed;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentWidth parsed)
    {
        if (DutchNameMapping.TryGetValue(value, out parsed))
        {
            return true;
        }

        parsed = Array.Find(All, candidate => candidate.ToString() == value);
        return parsed.ToString() == value;
    }
}
