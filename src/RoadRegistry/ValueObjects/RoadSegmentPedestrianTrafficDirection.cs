namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentPedestrianTrafficDirection : IEquatable<RoadSegmentPedestrianTrafficDirection>, IDutchToString
{
    public static readonly RoadSegmentPedestrianTrafficDirection Both =
        new(
            nameof(Both),
            new DutchTranslation(
                "beide"
            )
        );
    public static readonly RoadSegmentPedestrianTrafficDirection None =
        new(
            nameof(None),
            new DutchTranslation(
                "geen"
            )
        );

    public static readonly RoadSegmentPedestrianTrafficDirection[] All =
    [
        Both, None
    ];

    public static readonly IReadOnlyDictionary<string, RoadSegmentPedestrianTrafficDirection> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentPedestrianTrafficDirection(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public static bool CanParse(string value)
    {
        return TryParse(value.ThrowIfNull(), out _);
    }

    public static bool CanParseUsingDutchName(string value)
    {
        return TryParseUsingDutchName(value, out _);
    }

    public bool Equals(RoadSegmentPedestrianTrafficDirection? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentPedestrianTrafficDirection type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentPedestrianTrafficDirection? left, RoadSegmentPedestrianTrafficDirection? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentPedestrianTrafficDirection? left, RoadSegmentPedestrianTrafficDirection? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentPedestrianTrafficDirection? instance)
    {
        return instance?.ToString();
    }

    public static RoadSegmentPedestrianTrafficDirection Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known traffic direction.");
        return parsed;
    }

    public static RoadSegmentPedestrianTrafficDirection ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known traffic direction.");
        return parsed;
    }

    public override string ToString()
    {
        return _value;
    }

    public string ToDutchString()
    {
        return Translation.Name;
    }

    public static bool TryParse(string value, out RoadSegmentPedestrianTrafficDirection? parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        value = value.Trim();

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentPedestrianTrafficDirection? parsed)
    {
        parsed = Array.Find(All, candidate => candidate.Translation.Name == value);
        return parsed != null;
    }

    public sealed class DutchTranslation
    {
        internal DutchTranslation(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
