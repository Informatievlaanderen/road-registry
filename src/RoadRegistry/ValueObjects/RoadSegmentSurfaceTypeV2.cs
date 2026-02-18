namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentSurfaceTypeV2 : IEquatable<RoadSegmentSurfaceTypeV2>, IDutchToString
{
    public static readonly RoadSegmentSurfaceTypeV2 Verhard =
        new(
            nameof(Verhard),
            new DutchTranslation(
                1,
                "verhard",
                "De oppervlakte van de weg is afgedicht door geprefabriceerde elementen zoals tegels, keien of straatstenen, of door een gebonden vorm van granulaten die na het aanbrengen een solide geheel vormt."
            )
        );

    public static readonly RoadSegmentSurfaceTypeV2 Halfverhard =
        new(
            nameof(Halfverhard),
            new DutchTranslation(
                2,
                "halfverhard",
                "De ondergrond van de weg is opgebouwd uit (kleine) losse stukken basismateriaal (granulaat). Deze granulaten werden bij constructie eerst in losse vorm aangebracht en vervolgens al dan niet bewerkt, verstevigd of gebonden."
            )
        );

    public static readonly RoadSegmentSurfaceTypeV2 Onverhard =
        new(
            nameof(Onverhard),
            new DutchTranslation(
                3,
                "onverhard",
                "De weg is ontstaan door betreding van de ondergrond (door mens of dier) of door een mechanische handeling zoals maaien, herprofileren of beperkte bodembewerkingen. Er werden geen externe materialen toegepast bij de aanleg van de weg."
            )
        );

    public static readonly RoadSegmentSurfaceTypeV2[] All =
    {
        Verhard, Halfverhard, Onverhard
    };

    public static readonly IReadOnlyDictionary<int, RoadSegmentSurfaceTypeV2> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentSurfaceTypeV2> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentSurfaceTypeV2(string value, DutchTranslation dutchTranslation)
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

    public bool Equals(RoadSegmentSurfaceTypeV2? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentSurfaceTypeV2 type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentSurfaceTypeV2? left, RoadSegmentSurfaceTypeV2? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentSurfaceTypeV2? left, RoadSegmentSurfaceTypeV2? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentSurfaceTypeV2? instance)
    {
        return instance?.ToString();
    }

    public static implicit operator int(RoadSegmentSurfaceTypeV2 instance)
    {
        return instance.Translation.Identifier;
    }

    public static RoadSegmentSurfaceTypeV2 Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known type of road surface.");
        return parsed;
    }

    public static RoadSegmentSurfaceTypeV2 ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known type of road surface.");
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

    public static bool TryParse(string value, out RoadSegmentSurfaceTypeV2? parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentSurfaceTypeV2? parsed)
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

        public string Description { get; }
        public int Identifier { get; }
        public string Name { get; }
    }
}
