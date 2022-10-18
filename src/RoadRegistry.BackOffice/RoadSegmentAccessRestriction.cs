namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadSegmentAccessRestriction : IEquatable<RoadSegmentAccessRestriction>
{
    public static readonly RoadSegmentAccessRestriction PublicRoad =
        new(
            nameof(PublicRoad),
            new DutchTranslation(
                1,
                "openbare weg",
                "Weg is publiek toegankelijk."
            )
        );

    public static readonly RoadSegmentAccessRestriction PhysicallyImpossible =
        new(
            nameof(PhysicallyImpossible),
            new DutchTranslation(
                2,
                "onmogelijke toegang",
                "Weg is niet toegankelijk vanwege de aanwezigheid van hindernissen of obstakels."
            )
        );

    public static readonly RoadSegmentAccessRestriction LegallyForbidden =
        new(
            nameof(LegallyForbidden),
            new DutchTranslation(
                3,
                "verboden toegang",
                "Toegang tot de weg is bij wet verboden."
            )
        );

    public static readonly RoadSegmentAccessRestriction PrivateRoad =
        new(
            nameof(PrivateRoad),
            new DutchTranslation(
                4,
                "privaatweg",
                "Toegang tot de weg is beperkt aangezien deze een private eigenaar heeft."
            )
        );

    public static readonly RoadSegmentAccessRestriction Seasonal =
        new(
            nameof(Seasonal),
            new DutchTranslation(
                5,
                "seizoensgebonden toegang",
                "Weg is afhankelijk van het seizoen (on)toegankelijk."
            )
        );

    public static readonly RoadSegmentAccessRestriction Toll =
        new(
            nameof(Toll),
            new DutchTranslation(
                6,
                "tolweg",
                "Toegang tot de weg is onderhevig aan tolheffingen."
            )
        );

    public static readonly RoadSegmentAccessRestriction[] All =
    {
        PublicRoad,
        PhysicallyImpossible,
        LegallyForbidden,
        PrivateRoad,
        Seasonal,
        Toll
    };

    public static readonly IReadOnlyDictionary<int, RoadSegmentAccessRestriction> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    private readonly string _value;

    private RoadSegmentAccessRestriction(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public static bool CanParse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return Array.Find(All, candidate => candidate._value == value) != null;
    }

    public static bool TryParse(string value, out RoadSegmentAccessRestriction parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static RoadSegmentAccessRestriction Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known road segment access restriction.");
        return parsed;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentAccessRestriction type && Equals(type);
    }

    public bool Equals(RoadSegmentAccessRestriction other)
    {
        return other != null && other._value == _value;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator string(RoadSegmentAccessRestriction instance)
    {
        return instance.ToString();
    }

    public static bool operator ==(RoadSegmentAccessRestriction left, RoadSegmentAccessRestriction right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentAccessRestriction left, RoadSegmentAccessRestriction right)
    {
        return !Equals(left, right);
    }

    public class DutchTranslation
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
