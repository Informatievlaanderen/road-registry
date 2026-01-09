namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentStatus : IEquatable<RoadSegmentStatus>, IDutchToString
{
    public static readonly RoadSegmentStatus PermitRequested =
        new(
            "PermitRequested",
            new DutchTranslation(
                1,
                "vergunning aangevraagd",
                "Geometrie komt voor op officieel document in behandeling."
            )
        );

    public static readonly RoadSegmentStatus PermitGranted =
        new(
            "BuildingPermitGranted",
            new DutchTranslation(
                2,
                "vergunning verleend",
                "Geometrie komt voor op goedgekeurd, niet vervallen bouwdossier."
            )
        );

    public static readonly RoadSegmentStatus UnderConstruction =
        new(
            nameof(UnderConstruction),
            new DutchTranslation(
                3,
                "in aanbouw",
                "Aanvang der werken is gemeld."
            )
        );

    public static readonly RoadSegmentStatus InUse =
        new(
            nameof(InUse),
            new DutchTranslation(
                4,
                "in gebruik",
                "Werken zijn opgeleverd."
            )
        );

    public static readonly RoadSegmentStatus OutOfUse =
        new(
            nameof(OutOfUse),
            new DutchTranslation(
                5,
                "buiten gebruik",
                "Fysieke weg is buiten gebruik gesteld maar niet gesloopt."
            )
        );

    public static readonly RoadSegmentStatus Retired =
        new(
            nameof(Retired),
            new DutchTranslation(
                6,
                "TODO-pr implement new values",
                "TODO-pr implement new values"
            )
        );

    public static readonly RoadSegmentStatus Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                -8,
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentStatus[] All =
    {
        Unknown, PermitRequested, PermitGranted, UnderConstruction, InUse, OutOfUse
    };

    public sealed record Edit
    {
        public static readonly ImmutableArray<RoadSegmentStatus> Editable = [..All.Where(x => x != Unknown)];
    }

    public static readonly IReadOnlyDictionary<int, RoadSegmentStatus> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentStatus> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentStatus(string value, DutchTranslation dutchTranslation)
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

    public bool Equals(RoadSegmentStatus? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentStatus type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentStatus left, RoadSegmentStatus right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentStatus left, RoadSegmentStatus right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentStatus? instance)
    {
        return instance?.ToString();
    }

    public static implicit operator int(RoadSegmentStatus instance)
    {
        return instance.Translation.Identifier;
    }

    public static RoadSegmentStatus Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment status.");
        return parsed;
    }

    public static RoadSegmentStatus ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment status.");
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

    public static bool TryParse(string value, out RoadSegmentStatus parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentStatus parsed)
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
