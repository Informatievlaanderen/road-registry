namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentAttributeSide : IEquatable<RoadSegmentAttributeSide>, IDutchToString
{
    public static readonly RoadSegmentAttributeSide Links =
        new(nameof(Links), new DutchTranslation(1, "links", "Het attribuut is van toepassing langs de linkerkant van het wegsegment (t.o.v. de digitalisatiezin)."));

    public static readonly RoadSegmentAttributeSide Rechts =
        new(nameof(Rechts), new DutchTranslation(2, "rechts", "Het attribuut is van toepassing langs de rechterkant van het wegsegment (t.o.v. de digitalisatiezin)."));

    public static readonly RoadSegmentAttributeSide Beide =
        new(nameof(Beide), new DutchTranslation(3, "beide", "Het attribuut is van toepassing langs beide kanten van het wegsegment (t.o.v. de digitalisatiezin)."));

    public static readonly RoadSegmentAttributeSide[] All =
    [
        Beide, Links, Rechts
    ];

    public static readonly IReadOnlyDictionary<int, RoadSegmentAttributeSide> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentAttributeSide> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentAttributeSide(string value, DutchTranslation dutchTranslation)
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

    public bool Equals(RoadSegmentAttributeSide? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentAttributeSide type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentAttributeSide? left, RoadSegmentAttributeSide? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentAttributeSide? left, RoadSegmentAttributeSide? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentAttributeSide? instance)
    {
        return instance?.ToString();
    }

    public static RoadSegmentAttributeSide Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment attribute side.");
        return parsed;
    }

    public static RoadSegmentAttributeSide ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment attribute side.");
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

    public static bool TryParse(string value, out RoadSegmentAttributeSide? parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        value = value.Trim();

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentAttributeSide? parsed)
    {
        parsed = Array.Find(All, candidate => candidate.Translation.Name == value);
        return parsed != null;
    }

    public sealed class DutchTranslation
    {
        internal DutchTranslation(int identifier, string name, string description)
        {
            Identifier = identifier;
            Name = name;
            Description = description;
        }

        public int Identifier { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
