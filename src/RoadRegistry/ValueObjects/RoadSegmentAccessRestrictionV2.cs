namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentAccessRestrictionV2 : IEquatable<RoadSegmentAccessRestrictionV2>, IDutchToString
{
    public static readonly RoadSegmentAccessRestrictionV2 OpenbareWeg =
        new(
            nameof(OpenbareWeg),
            new DutchTranslation(
                1,
                "openbare weg",
                "Elke weg die een bestemming tot openbaar gebruik heeft gekregen."
            )
        );

    public static readonly RoadSegmentAccessRestrictionV2 PrivateWeg =
        new(
            nameof(PrivateWeg),
            new DutchTranslation(
                2,
                "private weg",
                "Toegang tot de weg is beperkt aangezien deze een privatief karakter heeft."
            )
        );

    public static readonly RoadSegmentAccessRestrictionV2[] All =
    {
        OpenbareWeg,
        PrivateWeg
    };

    public static readonly IReadOnlyDictionary<int, RoadSegmentAccessRestrictionV2> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentAccessRestrictionV2> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentAccessRestrictionV2(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentAccessRestrictionV2 type && Equals(type);
    }

    public bool Equals(RoadSegmentAccessRestrictionV2? other)
    {
        return other is not null && other._value == _value;
    }

    public static bool CanParse(string value)
    {
        return TryParse(value.ThrowIfNull(), out _);
    }

    public static bool CanParseUsingDutchName(string value)
    {
        return TryParseUsingDutchName(value, out _);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentAccessRestrictionV2? left, RoadSegmentAccessRestrictionV2? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentAccessRestrictionV2? left, RoadSegmentAccessRestrictionV2? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentAccessRestrictionV2? instance)
    {
        return instance?.ToString();
    }

    public static implicit operator int(RoadSegmentAccessRestrictionV2 instance)
    {
        return instance.Translation.Identifier;
    }

    public static RoadSegmentAccessRestrictionV2 Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment access restriction.");
        return parsed;
    }

    public static RoadSegmentAccessRestrictionV2 ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment access restriction.");
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

    public static bool TryParse(string value, out RoadSegmentAccessRestrictionV2 parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentAccessRestrictionV2 parsed)
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
