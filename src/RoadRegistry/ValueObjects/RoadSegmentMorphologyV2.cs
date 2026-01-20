// ReSharper disable InconsistentNaming

namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentMorphologyV2 : IEquatable<RoadSegmentMorphologyV2>, IDutchToString
{
    public static readonly RoadSegmentMorphologyV2 Autosnelweg =
        new(
            nameof(Autosnelweg),
            new DutchTranslation(
                1,
                "autosnelweg",
                "Een autosnelweg heeft typisch twee gescheiden parallelle rijbanen met tegengestelde toegelaten rijrichtingen. Op een autosnelweg komen geen gelijkgrondse kruisingen voor. Kruisingen met andere wegen gebeuren steeds ofwel over bruggen of in tunnels."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Weg_met_gescheiden_rijbanen_die_geen_autosnelweg_is =
        new(
            nameof(Weg_met_gescheiden_rijbanen_die_geen_autosnelweg_is),
            new DutchTranslation(
                2,
                "weg_met_gescheiden_rijbanen_die_geen_autosnelweg_is",
                "Een weg met gescheiden rijbanen die geen autosnelweg is, wordt gekenmerkt door de aanwezigheid van minstens twee gescheiden rijbanen (onafhankelijk van het aantal rijstroken) die tegengestelde toegelaten rijrichtingen hebben."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Weg_bestaande_uit_1_rijbaan
        =
        new(
            nameof(Weg_bestaande_uit_1_rijbaan),
            new DutchTranslation(
                3,
                "Weg bestaande uit één rijbaan",
                "Een weg bestaande uit één rijbaan stelt de centrale as voor van een rijbaan waarop geen enkele andere morfologie van toepassing is."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Parallelweg =
        new(
            nameof(Parallelweg),
            new DutchTranslation(
                4,
                "Parallelweg",
                "Een parallelweg is een op- of afrit waarvan de begin- en eindpositie verbonden is met dezelfde weg. Een parallelweg heeft een rechtstreekse verbinding met deze weg, of een verbinding via op- of afritten die geen parallelweg zijn. Parallelwegen zijn uitsluitend bedoeld voor gemotoriseerd verkeer."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Op_of_afrit
        =
        new(
            nameof(Op_of_afrit),
            new DutchTranslation(
                5,
                "Op- of afrit",
                "Een op- of afrit vormt een verbinding tussen twee wegen die elkaar kruisen."
            )
        );

    public static readonly RoadSegmentMorphologyV2 In_of_uitrit =
        new(
            nameof(In_of_uitrit),
            new DutchTranslation(
                6,
                "In- of uitrit",
                "Een in- of uitrit is een weg die speciaal ontworpen is om een plaats te bereiken of te verlaten. In- of uitritten kunnen bijvoorbeeld opgenomen worden om een luchthaven, station, ziekenhuis, school, brandweerkazerne, politie, openbare dienst, parking, winkelcomplex, bedrijventerrein, hotel of restaurant te bereiken."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Rotonde =
        new(
            nameof(Rotonde),
            new DutchTranslation(
                7,
                "Rotonde",
                "Een rotonde is een weg waarop het verkeer in één richting verloopt rond een aangelegd middeneiland."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Aardeweg =
        new(
            nameof(Aardeweg),
            new DutchTranslation(
                8,
                "Aardeweg",
                "Een aardeweg is een openbare weg die breder is dan een pad en die niet voor het voertuigenverkeer in het algemeen is ingericht."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Pad =
        new(
            nameof(Pad),
            new DutchTranslation(
                9,
                "Pad",
                "Een pad is een smalle openbare weg die alleen het verkeer toelaat van voetgangers en van voertuigen die geen bredere dan de voor voetgangers vereiste ruimte nodig hebben."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Fietspad =
        new(
            nameof(Fietspad),
            new DutchTranslation(
                10,
                "Fietspad",
                "Een fietspad is het deel van de openbare weg gesignaleerd door het verkeersbord D7, D9, D11 of R12, of door twee evenwijdige witte onderbroken strepen op de weg. Fietspaden maken géén deel uit van de rijbaan."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Bedding_of_baan_voor_openbaar_vervoer =
        new(
            nameof(Bedding_of_baan_voor_openbaar_vervoer),
            new DutchTranslation(
                11,
                "Bedding of baan voor openbaar vervoer",
                "Een bedding of baan voor openbaar vervoer is een trambedding of busbaan. Een trambedding is een weg die speciaal ontworpen is voor het tramverkeer. Een busbaan wordt afgebakend door het verkeersbord F17 en één of twee brede witte doorlopende of onderbroken strepen, of dambordmarkeringen bestaande uit witte vierkanten. Andere voertuigen zijn slechts bij uitzondering toegelaten op beddingen of banen voor openbaar vervoer."
            )
        );

    public static readonly RoadSegmentMorphologyV2 Veer =
        new(
            nameof(Veer),
            new DutchTranslation(
                12,
                "veer",
                "Een veer is bedoeld voor het transport van passagiers, voertuigen of vracht over water en verbindt vaak twee of meerdere landwegen."
            )
        );


    public static readonly RoadSegmentMorphologyV2[] All =
    {
        Autosnelweg,
        Weg_met_gescheiden_rijbanen_die_geen_autosnelweg_is,
        Weg_bestaande_uit_1_rijbaan,
        Parallelweg,
        Op_of_afrit,
        In_of_uitrit,
        Rotonde,
        Aardeweg,
        Pad,
        Fietspad,
        Bedding_of_baan_voor_openbaar_vervoer,
        Veer
    };

    public sealed record Edit
    {
        public static readonly ImmutableArray<RoadSegmentMorphologyV2> Editable = [..All];
    }

    public static readonly IReadOnlyDictionary<int, RoadSegmentMorphologyV2> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentMorphologyV2> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentMorphologyV2(string value, DutchTranslation dutchTranslation)
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

    public bool Equals(RoadSegmentMorphologyV2? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentMorphologyV2 type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentMorphologyV2 left, RoadSegmentMorphologyV2 right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentMorphologyV2 left, RoadSegmentMorphologyV2 right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentMorphologyV2? instance)
    {
        return instance?.ToString();
    }

    public static implicit operator int(RoadSegmentMorphologyV2 instance)
    {
        return instance.Translation.Identifier;
    }

    public static RoadSegmentMorphologyV2 Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment morphology.");
        return parsed;
    }

    public static RoadSegmentMorphologyV2 ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment morphology.");
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

    public static bool TryParse(string value, out RoadSegmentMorphologyV2? parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentMorphologyV2? parsed)
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
