namespace RoadRegistry.Messages
{
    using System.Collections.Generic;

    public enum RoadSegmentMorphology
    {
        Unknown = -8,
        Motorway = 101,
        Road_with_separate_lanes_that_is_not_a_motorway = 102,
        Road_consisting_of_one_roadway = 103,
        Traffic_circle = 104,
        Special_traffic_situation = 105,
        Traffic_square = 106,
        Entry_or_exit_ramp_belonging_to_a_grade_separated_junction = 107,
        Entry_or_exit_ramp_belonging_to_a_level_junction = 108,
        Parallel_road = 109,
        Frontage_road = 110,
        Entry_or_exit_of_a_car_park = 111,
        Entry_or_exit_of_a_service = 112,
        Pedestrain_zone = 113,
        Walking_or_cycling_path_not_accessible_to_other_vehicles = 114,
        Tramway_not_accessible_to_other_vehicles = 116,
        Service_road = 120,
        Primitive_road = 125,
        Ferry = 130
    }

    public class RoadSegmentMorphologyTranslator : EnumTranslator<RoadSegmentMorphology>
    {
        protected override IDictionary<RoadSegmentMorphology, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<RoadSegmentMorphology, string> DutchDescriptions => _dutchDescriptions;

        private static readonly IDictionary<RoadSegmentMorphology, string> _dutchTranslations =
            new Dictionary<RoadSegmentMorphology, string>
            {
                { RoadSegmentMorphology.Unknown, "niet gekend" },
                { RoadSegmentMorphology.Motorway, "autosnelweg" },
                { RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway, "weg met gescheiden rijbanen die geen autosnelweg is" },
                { RoadSegmentMorphology.Road_consisting_of_one_roadway, "weg bestaande uit één rijbaan" },
                { RoadSegmentMorphology.Traffic_circle, "rotonde" },
                { RoadSegmentMorphology.Special_traffic_situation, "speciale verkeerssituatie" },
                { RoadSegmentMorphology.Traffic_square, "verkeersplein" },
                { RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction, "op- of afrit, behorende tot een niet-gelijkgrondse verbinding" },
                { RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction, "op- of afrit, behorende tot een gelijkgrondse verbinding" },
                { RoadSegmentMorphology.Parallel_road, "parallelweg" },
                { RoadSegmentMorphology.Frontage_road, "ventweg" },
                { RoadSegmentMorphology.Entry_or_exit_of_a_car_park, "in- of uitrit van een parking" },
                { RoadSegmentMorphology.Entry_or_exit_of_a_service, "in- of uitrit van een dienst" },
                { RoadSegmentMorphology.Pedestrain_zone, "voetgangerszone" },
                { RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles, "wandel- of fietsweg, niet toegankelijk voor andere voertuigen" },
                { RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles, "tramweg, niet toegankelijk voor andere voertuigen" },
                { RoadSegmentMorphology.Service_road, "dienstweg" },
                { RoadSegmentMorphology.Primitive_road, "aardeweg" },
                { RoadSegmentMorphology.Ferry, "veer" },
            };

        private static readonly IDictionary<RoadSegmentMorphology, string> _dutchDescriptions =
            new Dictionary<RoadSegmentMorphology, string>
            {
                { RoadSegmentMorphology.Unknown, "Geen informatie beschikbaar" },
                { RoadSegmentMorphology.Motorway, "Een \"autosnelweg\" heeft typisch twee gescheiden parallelle rijbanen met tegengestelde toegelaten rijrichtingen. Op een autosnelweg komen geen gelijkgrondse kruisingen voor. Kruisingen met andere wegen gebeuren steeds ofwel over bruggen of in tunnels." },
                { RoadSegmentMorphology.Road_with_separate_lanes_that_is_not_a_motorway, "\"Een weg met gescheiden rijbanen die geen autosnelweg is\" wordt gekenmerkt door de aanwezigheid van minstens twee rijbanen (onafhankelijk van het aantal rijstroken) die fysiek gescheiden zijn en tegengestelde toegelaten rijrichtingen hebben." },
                { RoadSegmentMorphology.Road_consisting_of_one_roadway, "Wegsegmenten die behoren tot een weg waar het verkeer niet fysiek gescheiden wordt." },
                { RoadSegmentMorphology.Traffic_circle, "Wegsegmenten die tot de rotonde behoren vormen een gesloten ringvormige structuur. Op deze wegsegmenten is enkel eenrichtingsverkeer toegelaten." },
                { RoadSegmentMorphology.Special_traffic_situation, "Wegsegmenten die behoren tot een min of meer cirkelvormige constructie die geen rotonde is." },
                { RoadSegmentMorphology.Traffic_square, "Wegsegmenten die worden opgenomen in gebieden waar het verkeer ongestructureerd verloopt. Het gaat typisch om marktpleinen, parkeerterreinen of terreinen met een andere functie dan een zuivere verkeersfunctie." },
                { RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_grade_separated_junction, "Een \"op- of afrit, behorende tot een niet-gelijkgrondse kruising\" verzorgt de verbinding tussen twee wegen die zich niet-gelijkgronds kruisen. Alle op- en afritten van autosnelwegen en verkeerswisselaars worden eveneens tot deze klasse gerekend." },
                { RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_level_junction, "Een \"op- of afrit, behorende tot een gelijkgrondse kruising\" verzorgt de verbinding tussen twee wegen die geen autosnelweg zijn. Zonder de op- of afrit bestaat er nog steeds een topologische verbinding tussen de wegsegmenten waarbij de op- of afrit hoort" },
                { RoadSegmentMorphology.Parallel_road, "Een \"parallelweg\" is een op- of afrit waarvan de begin- en eindpositie verbonden is met dezelfde autosnelweg. Een \"parallelweg\" heeft een rechtstreekse verbinding of een verbinding via op- of afritten van een ander type met de bijhorende autosnelweg." },
                { RoadSegmentMorphology.Frontage_road, "Een \"ventweg\" loopt parallel aan een weg met een belangrijke verkeersfunctie die geen autosnelweg is. De weg biedt toegang tot minder belangrijke aanpalende wegen, bestemmingen of adressen en wordt van de hoofdweg gescheiden door kleine constructies." },
                { RoadSegmentMorphology.Entry_or_exit_of_a_car_park, "Een \"in- of uitrit van een parking\" is een weg die speciaal ontworpen is om een parkeerterrein of parkeergarage te bereiken of te verlaten." },
                { RoadSegmentMorphology.Entry_or_exit_of_a_service, "Een \"in- of uitrit van een dienst\" is een weg die speciaal ontworpen is om een dienst (voorbeeld: luchthaven, station, ziekenhuis, brandweerkazerne, politie, openbare dienst, hotel, restaurant) te bereiken of te verlaten." },
                { RoadSegmentMorphology.Pedestrain_zone, "Gebied met een wegennet dat speciaal ontworpen is voor gebruik door voetgangers (meestal gesitueerd in stedelijke gebieden). In voetgangerszones is enkel voetgangersverkeer toegelaten (uitzondering: prioritaire voertuigen en leveringen)." },
                { RoadSegmentMorphology.Walking_or_cycling_path_not_accessible_to_other_vehicles, "Op een \"wandel- en/of fietsweg\" is de verkeerstoegang beperkt tot voetgangers en/of fietsers. De fysieke kenmerken van een \"wandel- en/of fietsweg\" laten de toegang van andere voertuigen niet toe (smaller dan 2.5m)." },
                { RoadSegmentMorphology.Tramway_not_accessible_to_other_vehicles, "Een \"tramweg\" is een weg die speciaal ontworpen is voor het tramverkeer. De fysieke kenmerken van een \"tramweg\" laten de toegang van andere voertuigen niet toe." },
                { RoadSegmentMorphology.Service_road, "Een dienstweg is uitsluitend bestemd voor bevoegde diensten (wegbeheerders, hulp- en spoeddiensten, …)." },
                { RoadSegmentMorphology.Primitive_road, "Een \"aardeweg\" is een weg zonder wegverharding die op zijn minst berijdbaar is voor bepaalde vierwielige motorvoertuigen (bv. terreinwagens, landbouwvoertuigen,…)." },
                { RoadSegmentMorphology.Ferry, "Een \"veer\" is bedoeld voor het transport van passagiers, voertuigen of vracht over het water en verbindt vaak twee of meerdere landwegen." },
            };
    }
}
