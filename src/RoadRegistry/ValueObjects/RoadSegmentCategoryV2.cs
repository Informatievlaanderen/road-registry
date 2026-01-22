// ReSharper disable InconsistentNaming

namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentCategoryV2 : IEquatable<RoadSegmentCategoryV2>, IDutchToString
{
    //TODO-pr rename props to NL
    public static readonly RoadSegmentCategoryV2 EuropeseHoofdweg =
        new(
            nameof(EuropeseHoofdweg),
            new DutchTranslation(
                "EHW",
                "europese hoofdweg",
                "Europese hoofdwegen"
            )
        );

    public static readonly RoadSegmentCategoryV2 VlaamseHoofdweg =
        new(
            nameof(VlaamseHoofdweg),
            new DutchTranslation(
                "VHW",
                "vlaamse hoofdweg",
                "Vlaamse hoofdwegen"
            )
        );

    public static readonly RoadSegmentCategoryV2 RegionaleWeg =
        new(
            nameof(RegionaleWeg),
            new DutchTranslation(
                "RW",
                "regionale weg",
                "Regionale wegen"
            )
        );

    public static readonly RoadSegmentCategoryV2 InterlokaleWeg =
        new(
            nameof(InterlokaleWeg),
            new DutchTranslation(
                "IW",
                "interlokale weg",
                "Interlokale wegen"
            )
        );

    public static readonly RoadSegmentCategoryV2 LokaleOntsluitingsweg =
        new(
            nameof(LokaleOntsluitingsweg),
            new DutchTranslation(
                "OW",
                "lokale ontsluitingsweg",
                "Lokale ontsluitingsweg"
            )
        );

    public static readonly RoadSegmentCategoryV2 LokaleErftoegangsweg =
        new(
            nameof(LokaleErftoegangsweg),
            new DutchTranslation(
                "EW",
                "lokale erftoegangsweg",
                "Lokale erftoegangsweg"
            )
        );

    public static readonly RoadSegmentCategoryV2 NietGekend =
        new(
            nameof(NietGekend),
            new DutchTranslation(
                "-8",
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentCategoryV2 NietVanToepassing =
        new(
            nameof(NietVanToepassing),
            new DutchTranslation(
                "-9",
                "niet van toepassing",
                "Niet van toepassing"
            )
        );

    public static readonly RoadSegmentCategoryV2[] All =
    [
        NietGekend,
        NietVanToepassing,
        EuropeseHoofdweg,
        VlaamseHoofdweg,
        RegionaleWeg,
        InterlokaleWeg,
        LokaleOntsluitingsweg,
        LokaleErftoegangsweg
    ];

    public sealed record Edit
    {
        public static readonly ImmutableArray<RoadSegmentCategoryV2> Editable = [..All];
    }

    public static readonly IReadOnlyDictionary<string, RoadSegmentCategoryV2> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier, StringComparer.InvariantCultureIgnoreCase);

    private readonly string _value;

    private RoadSegmentCategoryV2(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentCategoryV2 type && Equals(type);
    }

    public bool Equals(RoadSegmentCategoryV2 other)
    {
        return other != null && other._value == _value;
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

    public static bool operator ==(RoadSegmentCategoryV2 left, RoadSegmentCategoryV2 right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentCategoryV2 left, RoadSegmentCategoryV2 right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentCategoryV2? instance)
    {
        return instance?.ToString();
    }

    public static RoadSegmentCategoryV2 Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known road segment category.");

        return parsed;
    }
    public static RoadSegmentCategoryV2 ParseUsingDutchName(string value)
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

    public static bool TryParse(string value, out RoadSegmentCategoryV2 parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentCategoryV2 parsed)
    {
        parsed = Array.Find(All, candidate => candidate.Translation.Name == value);
        return parsed != null;
    }

    public sealed class DutchTranslation
    {
        internal DutchTranslation(string identifier, string name, string description)
        {
            Identifier = identifier;
            Name = name;
            Description = description;
        }

        public string Description { get; }
        public string Identifier { get; }
        public string Name { get; }
    }
}
