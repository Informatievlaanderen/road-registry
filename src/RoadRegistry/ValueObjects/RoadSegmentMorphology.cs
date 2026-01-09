// ReSharper disable InconsistentNaming

namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentMorphology : IEquatable<RoadSegmentMorphology>, IDutchToString
{
    public static readonly RoadSegmentMorphology Entry_or_exit_of_a_car_park =
        new(
            nameof(Entry_or_exit_of_a_car_park),
            new DutchTranslation(
                111,
                "in- of uitrit van een parking",
                "Een \"in- of uitrit van een parking\" is een weg die speciaal ontworpen is om een parkeerterrein of parkeergarage te bereiken of te verlaten."
            )
        );

    public static readonly RoadSegmentMorphology Entry_or_exit_of_a_service =
        new(
            nameof(Entry_or_exit_of_a_service),
            new DutchTranslation(
                112,
                "in- of uitrit van een dienst",
                "Een \"in- of uitrit van een dienst\" is een weg die speciaal ontworpen is om een dienst (voorbeeld: luchthaven, station, ziekenhuis, brandweerkazerne, politie, openbare dienst, hotel, restaurant) te bereiken of te verlaten."
            )
        );

    public static readonly RoadSegmentMorphology Entry_or_exit_ramp_belonging_to_a_grade_separated_junction =
        new(
            nameof(Entry_or_exit_ramp_belonging_to_a_grade_separated_junction),
            new DutchTranslation(
                107,
                "op- of afrit, behorende tot een niet-gelijkgrondse verbinding",
                "Een \"op- of afrit, behorende tot een niet-gelijkgrondse kruising\" verzorgt de verbinding tussen twee wegen die zich niet-gelijkgronds kruisen. Alle op- en afritten van autosnelwegen en verkeerswisselaars worden eveneens tot deze klasse gerekend."
            )
        );

    public static readonly RoadSegmentMorphology Entry_or_exit_ramp_belonging_to_a_level_junction =
        new(
            nameof(Entry_or_exit_ramp_belonging_to_a_level_junction),
            new DutchTranslation(
                108,
                "op- of afrit, behorende tot een gelijkgrondse verbinding",
                "Een \"op- of afrit, behorende tot een gelijkgrondse kruising\" verzorgt de verbinding tussen twee wegen die geen autosnelweg zijn. Zonder de op- of afrit bestaat er nog steeds een topologische verbinding tussen de wegsegmenten waarbij de op- of afrit hoort"
            )
        );

    public static readonly RoadSegmentMorphology Ferry =
        new(
            nameof(Ferry),
            new DutchTranslation(
                130,
                "veer",
                "Een \"veer\" is bedoeld voor het transport van passagiers, voertuigen of vracht over het water en verbindt vaak twee of meerdere landwegen."
            )
        );

    public static readonly RoadSegmentMorphology FrontageRoad =
        new(
            nameof(FrontageRoad),
            new DutchTranslation(
                110,
                "ventweg",
                "Een \"ventweg\" loopt parallel aan een weg met een belangrijke verkeersfunctie die geen autosnelweg is. De weg biedt toegang tot minder belangrijke aanpalende wegen, bestemmingen of adressen en wordt van de hoofdweg gescheiden door kleine constructies."
            )
        );

    public static readonly RoadSegmentMorphology Motorway =
        new(
            nameof(Motorway),
            new DutchTranslation(
                101,
                "autosnelweg",
                "Een \"autosnelweg\" heeft typisch twee gescheiden parallelle rijbanen met tegengestelde toegelaten rijrichtingen. Op een autosnelweg komen geen gelijkgrondse kruisingen voor. Kruisingen met andere wegen gebeuren steeds ofwel over bruggen of in tunnels."
            )
        );

    public static readonly RoadSegmentMorphology ParallelRoad =
        new(
            nameof(ParallelRoad),
            new DutchTranslation(
                109,
                "parallelweg",
                "Een \"parallelweg\" is een op- of afrit waarvan de begin- en eindpositie verbonden is met dezelfde autosnelweg. Een \"parallelweg\" heeft een rechtstreekse verbinding of een verbinding via op- of afritten van een ander type met de bijhorende autosnelweg."
            )
        );

    public static readonly RoadSegmentMorphology PedestrainZone =
        new(
            nameof(PedestrainZone),
            new DutchTranslation(
                113,
                "voetgangerszone",
                "Gebied met een wegennet dat speciaal ontworpen is voor gebruik door voetgangers (meestal gesitueerd in stedelijke gebieden). In voetgangerszones is enkel voetgangersverkeer toegelaten (uitzondering: prioritaire voertuigen en leveringen)."
            )
        );

    public static readonly RoadSegmentMorphology PrimitiveRoad =
        new(
            nameof(PrimitiveRoad),
            new DutchTranslation(
                125,
                "aardeweg",
                "Een \"aardeweg\" is een weg zonder wegverharding die op zijn minst berijdbaar is voor bepaalde vierwielige motorvoertuigen (bv. terreinwagens, landbouwvoertuigen,…)."
            )
        );

    public static readonly RoadSegmentMorphology Road_consisting_of_one_roadway =
        new(
            nameof(Road_consisting_of_one_roadway),
            new DutchTranslation(
                103,
                "weg bestaande uit één rijbaan",
                "Wegsegmenten die behoren tot een weg waar het verkeer niet fysiek gescheiden wordt."
            )
        );

    public static readonly RoadSegmentMorphology Road_with_separate_lanes_that_is_not_a_motorway =
        new(
            nameof(Road_with_separate_lanes_that_is_not_a_motorway),
            new DutchTranslation(
                102,
                "weg met gescheiden rijbanen die geen autosnelweg is",
                "\"Een weg met gescheiden rijbanen die geen autosnelweg is\" wordt gekenmerkt door de aanwezigheid van minstens twee rijbanen (onafhankelijk van het aantal rijstroken) die fysiek gescheiden zijn en tegengestelde toegelaten rijrichtingen hebben."
            )
        );

    public static readonly RoadSegmentMorphology ServiceRoad =
        new(
            nameof(ServiceRoad),
            new DutchTranslation(
                120,
                "dienstweg",
                "Een dienstweg is uitsluitend bestemd voor bevoegde diensten (wegbeheerders, hulp- en spoeddiensten, …)."
            )
        );

    public static readonly RoadSegmentMorphology SpecialTrafficSituation =
        new(
            nameof(SpecialTrafficSituation),
            new DutchTranslation(
                105,
                "speciale verkeerssituatie",
                "Wegsegmenten die behoren tot een min of meer cirkelvormige constructie die geen rotonde is."
            )
        );

    public static readonly RoadSegmentMorphology TrafficCircle =
        new(
            nameof(TrafficCircle),
            new DutchTranslation(
                104,
                "rotonde",
                "Wegsegmenten die tot de rotonde behoren vormen een gesloten ringvormige structuur. Op deze wegsegmenten is enkel eenrichtingsverkeer toegelaten."
            )
        );

    public static readonly RoadSegmentMorphology TrafficSquare =
        new(
            nameof(TrafficSquare),
            new DutchTranslation(
                106,
                "verkeersplein",
                "Wegsegmenten die worden opgenomen in gebieden waar het verkeer ongestructureerd verloopt. Het gaat typisch om marktpleinen, parkeerterreinen of terreinen met een andere functie dan een zuivere verkeersfunctie."
            )
        );

    public static readonly RoadSegmentMorphology Tramway_not_accessible_to_other_vehicles =
        new(
            nameof(Tramway_not_accessible_to_other_vehicles),
            new DutchTranslation(
                116,
                "tramweg, niet toegankelijk voor andere voertuigen",
                "Een \"tramweg\" is een weg die speciaal ontworpen is voor het tramverkeer. De fysieke kenmerken van een \"tramweg\" laten de toegang van andere voertuigen niet toe."
            )
        );

    public static readonly RoadSegmentMorphology Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                -8,
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentMorphology Walking_or_cycling_path_not_accessible_to_other_vehicles =
        new(
            nameof(Walking_or_cycling_path_not_accessible_to_other_vehicles),
            new DutchTranslation(
                114,
                "wandel- of fietsweg, niet toegankelijk voor andere voertuigen",
                "Op een \"wandel- en/of fietsweg\" is de verkeerstoegang beperkt tot voetgangers en/of fietsers. De fysieke kenmerken van een \"wandel- en/of fietsweg\" laten de toegang van andere voertuigen niet toe (smaller dan 2.5m)."
            )
        );

    public static readonly RoadSegmentMorphology[] All =
    {
        Unknown,
        Motorway,
        Road_with_separate_lanes_that_is_not_a_motorway,
        Road_consisting_of_one_roadway,
        TrafficCircle,
        SpecialTrafficSituation,
        TrafficSquare,
        Entry_or_exit_ramp_belonging_to_a_grade_separated_junction,
        Entry_or_exit_ramp_belonging_to_a_level_junction,
        ParallelRoad,
        FrontageRoad,
        Entry_or_exit_of_a_car_park,
        Entry_or_exit_of_a_service,
        PedestrainZone,
        Walking_or_cycling_path_not_accessible_to_other_vehicles,
        Tramway_not_accessible_to_other_vehicles,
        ServiceRoad,
        PrimitiveRoad,
        Ferry
    };

    public sealed record Edit
    {
        public static readonly ImmutableArray<RoadSegmentMorphology> Editable = [..All.Where(x => x != Unknown)];
    }

    public static readonly IReadOnlyDictionary<int, RoadSegmentMorphology> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentMorphology> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentMorphology(string value, DutchTranslation dutchTranslation)
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

    public bool Equals(RoadSegmentMorphology? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentMorphology type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentMorphology left, RoadSegmentMorphology right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentMorphology left, RoadSegmentMorphology right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentMorphology? instance)
    {
        return instance?.ToString();
    }

    public static implicit operator int(RoadSegmentMorphology instance)
    {
        return instance.Translation.Identifier;
    }

    public static RoadSegmentMorphology Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment morphology.");
        return parsed;
    }

    public static RoadSegmentMorphology ParseUsingDutchName(string value)
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

    public static bool TryParse(string value, out RoadSegmentMorphology? parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentMorphology? parsed)
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
