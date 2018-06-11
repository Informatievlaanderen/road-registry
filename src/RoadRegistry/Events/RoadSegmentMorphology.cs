namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum RoadSegmentMorphology
    {
        Unknown = -8, // -8	niet gekend	Geen informatie beschikbaar
        Motorway = 101, // 101	autosnelweg	Een "autosnelweg" heeft typisch twee gescheiden parallelle rijbanen met tegengestelde toegelaten rijrichtingen. Op een autosnelweg komen geen gelijkgrondse kruisingen voor. Kruisingen met andere wegen gebeuren steeds ofwel over bruggen of in tunnels.
        Road_with_separate_lanes_that_is_not_a_motorway = 102, // 102	weg met gescheiden rijbanen die geen autosnelweg is	"Een weg met gescheiden rijbanen die geen autosnelweg is" wordt gekenmerkt door de aanwezigheid van minstens twee rijbanen (onafhankelijk van het aantal rijstroken) die fysiek gescheiden zijn en tegengestelde toegelaten rijrichtingen hebben.
        Road_consisting_of_one_roadway = 103, // 103	weg bestaande uit één rijbaan	Wegsegmenten die behoren tot een weg waar het verkeer niet fysiek gescheiden wordt.
        Traffic_circle = 104, // 104	rotonde	Wegsegmenten die tot de rotonde behoren vormen een gesloten ringvormige structuur. Op deze wegsegmenten is enkel eenrichtingsverkeer toegelaten.
        Special_traffic_situation = 105, // 105	speciale verkeerssituatie	Wegsegmenten die behoren tot een min of meer cirkelvormige constructie die geen rotonde is.
        Traffic_square = 106, // 106	verkeersplein	Wegsegmenten die worden opgenomen in gebieden waar het verkeer ongestructureerd verloopt. Het gaat typisch om marktpleinen, parkeerterreinen of terreinen met een andere functie dan een zuivere verkeersfunctie.
        Entry_or_exit_ramp_belonging_to_a_grade_separated_junction = 107, // 107	op- of afrit, behorende tot een niet-gelijkgrondse verbinding	Een "op- of afrit, behorende tot een niet-gelijkgrondse kruising" verzorgt de verbinding tussen twee wegen die zich niet-gelijkgronds kruisen. Alle op- en afritten van autosnelwegen en verkeerswisselaars worden eveneens tot deze klasse gerekend.
        Entry_or_exit_ramp_belonging_to_a_at_grade_junction = 108, // 108	op- of afrit, behorende tot een gelijkgrondse verbinding	Een "op- of afrit, behorende tot een gelijkgrondse kruising" verzorgt de verbinding tussen twee wegen die geen autosnelweg zijn. Zonder de op- of afrit bestaat er nog steeds een topologische verbinding tussen de wegsegmenten waarbij de op- of afrit hoort.
        Parallel_road = 109, // 109	parallelweg	Een "parallelweg" is een op- of afrit waarvan de begin- en eindpositie verbonden is met dezelfde autosnelweg. Een ‘parallelweg’ heeft een rechtstreekse verbinding of een verbinding via op- of afritten van een ander type met de bijhorende autosnelweg.
        Frontage_road = 110, // 110	ventweg	Een "ventweg" loopt parallel aan een weg met een belangrijke verkeersfunctie die geen autosnelweg is. De weg biedt toegang tot minder belangrijke aanpalende wegen, bestemmingen of adressen en wordt van de hoofdweg gescheiden door kleine constructies.
        Entry_or_exit_of_a_car_park = 111, // 111	in- of uitrit van een parking	Een "in- of uitrit van een parking" is een weg die speciaal ontworpen is om een parkeerterrein of parkeergarage te bereiken of te verlaten.
        Entry_or_exit_of_a_service = 112, // 112	in- of uitrit van een dienst	Een "in- of uitrit van een dienst" is een weg die speciaal ontworpen is om een dienst (voorbeeld: luchthaven, station, ziekenhuis, brandweerkazerne, politie, openbare dienst, hotel, restaurant) te bereiken of te verlaten.
        Pedestrain_zone = 113, // 113	voetgangerszone	Gebied met een wegennet dat speciaal ontworpen is voor gebruik door voetgangers (meestal gesitueerd in stedelijke gebieden). In voetgangerszones is enkel voetgangersverkeer toegelaten (uitzondering: prioritaire voertuigen en leveringen).
        Walking_or_cycling_path_not_accessible_to_other_vehicles = 114, // 114	wandel- of fietsweg, niet toegankelijk voor andere voertuigen	Op een "wandel- en/of fietsweg" is de verkeerstoegang beperkt tot voetgangers en/of fietsers. De fysieke kenmerken van een "wandel- en/of fietsweg" laten de toegang van andere voertuigen niet toe (smaller dan 2.5m).
        Tramway_not_accessible_to_other_vehicles = 116, // 116	tramweg, niet toegankelijk voor andere voertuigen	Een "tramweg" is een weg die speciaal ontworpen is voor het tramverkeer. De fysieke kenmerken van een "tramweg" laten de toegang van andere voertuigen niet toe.
        Service_road = 120, // 120	dienstweg	Een dienstweg is uitsluitend bestemd voor bevoegde diensten (wegbeheerders, hulp- en spoeddiensten, …).
        Primitive_road = 125, // 125	aardeweg	Een "aardeweg" is een weg zonder wegverharding die op zijn minst berijdbaar is voor bepaalde vierwielige motorvoertuigen (bv. terreinwagens, landbouwvoertuigen,…)
        Ferry = 130 // 130	veer	Een "veer" is bedoeld voor het transport van passagiers, voertuigen of vracht over het water en verbindt vaak twee of meerdere landwegen.
    }

    public class RoadSegmentMorphologyTranslator : EnumTranslator<RoadSegmentMorphology>
    {
        protected override IDictionary<RoadSegmentMorphology, string> DutchTranslations => _dutchTranslations;
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
                { RoadSegmentMorphology.Entry_or_exit_ramp_belonging_to_a_at_grade_junction, "op- of afrit, behorende tot een gelijkgrondse verbinding" },
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
    }
}
