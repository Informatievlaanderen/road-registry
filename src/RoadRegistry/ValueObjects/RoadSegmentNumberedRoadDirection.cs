namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentNumberedRoadDirection : IEquatable<RoadSegmentNumberedRoadDirection>, IDutchToString
{
    public static readonly RoadSegmentNumberedRoadDirection Backward =
        new(
            nameof(Backward),
            new DutchTranslation(
                2,
                "tegengesteld aan de digitalisatiezin",
                "Nummering weg slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment."
            )
        );

    public static readonly RoadSegmentNumberedRoadDirection Forward =
        new(
            nameof(Forward),
            new DutchTranslation(
                1,
                "gelijklopend met de digitalisatiezin",
                "Nummering weg slaat op de richting die de digitalisatiezin van het wegsegment volgt."
            )
        );

    public static readonly RoadSegmentNumberedRoadDirection Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                -8,
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentNumberedRoadDirection[] All =
    {
        Unknown, Forward, Backward
    };

    public static readonly IReadOnlyDictionary<int, RoadSegmentNumberedRoadDirection> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    private readonly string _value;

    private RoadSegmentNumberedRoadDirection(string value, DutchTranslation dutchTranslation)
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

    public bool Equals(RoadSegmentNumberedRoadDirection? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentNumberedRoadDirection type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentNumberedRoadDirection left, RoadSegmentNumberedRoadDirection right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentNumberedRoadDirection left, RoadSegmentNumberedRoadDirection right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string(RoadSegmentNumberedRoadDirection instance)
    {
        return instance.ToString();
    }

    public static RoadSegmentNumberedRoadDirection Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known numbered road segment direction.");
        return parsed;
    }

    public static RoadSegmentNumberedRoadDirection ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known numbered road segment direction.");
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

    public static bool TryParse(string value, out RoadSegmentNumberedRoadDirection parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentNumberedRoadDirection parsed)
    {
        parsed = Array.Find(All, candidate => candidate.Translation.Name == value);
        return parsed != null;
    }

    public class DutchTranslation
    {
        internal DutchTranslation(int identifier, string name, string description)
        {
            Identifier = identifier;
            Name = name;
            Description = description;
        }

        public string Description { get; }
        public int Identifier { get; }
        public string Name { get; }
    }
}
