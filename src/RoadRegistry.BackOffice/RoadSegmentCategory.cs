// ReSharper disable InconsistentNaming

namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed class RoadSegmentCategory : IEquatable<RoadSegmentCategory>, IDutchToString
{
    public static readonly RoadSegmentCategory LocalRoad =
        new(
            nameof(LocalRoad),
            new DutchTranslation(
                "L",
                "lokale weg",
                "Lokale wegen zijn wegen waar het toegang geven de belangrijkste functie is en zijn aldus niet van gewestelijk belang."
            )
        );

    public static readonly RoadSegmentCategory LocalRoadType1 =
        new(
            nameof(LocalRoadType1),
            new DutchTranslation(
                "L1",
                "lokale weg type 1",
                "Lokale verbindingsweg"
            )
        );

    public static readonly RoadSegmentCategory LocalRoadType2 =
        new(
            nameof(LocalRoadType2),
            new DutchTranslation(
                "L2",
                "lokale weg type 2",
                "Lokale gebiedsontsluitingsweg"
            )
        );

    public static readonly RoadSegmentCategory LocalRoadType3 =
        new(
            nameof(LocalRoadType3),
            new DutchTranslation(
                "L3",
                "lokale weg type 3",
                "Lokale erftoegangsweg"
            )
        );

    public static readonly RoadSegmentCategory MainRoad =
        new(
            nameof(MainRoad),
            new DutchTranslation(
                "H",
                "hoofdweg",
                "Wegen die de verbindingsfunctie verzorgen voor de grootstedelijke- en regionaalstedelijke gebieden met elkaar, met het Brussels Hoofdstedelijk Gewest en met de groot- en regionaalstedelijke gebieden in Wallonië en de buurlanden."
            )
        );

    public static readonly RoadSegmentCategory PrimaryRoadI =
        new(
            nameof(PrimaryRoadI),
            new DutchTranslation(
                "PI",
                "primaire weg I",
                "Wegen die noodzakelijk zijn om het net van hoofdwegen te complementeren, maar die geen functie hebben als doorgaande, internationale verbinding."
            )
        );

    public static readonly RoadSegmentCategory PrimaryRoadII =
        new(
            nameof(PrimaryRoadII),
            new DutchTranslation(
                "PII",
                "primaire weg II",
                "Wegen die een verzamelfunctie hebben voor gebieden en/of concentraties van activiteiten van gewestelijk belang."
            )
        );

    public static readonly RoadSegmentCategory PrimaryRoadIIType1 =
        new(
            nameof(PrimaryRoadIIType1),
            new DutchTranslation(
                "PII-1",
                "primaire weg II type 1",
                "De weg verzorgt binnen een grootstedelijk gebied of een poort de verbindings- en verzamelfunctie voor het geheel van het stedelijk gebied of de poort."
            )
        );

    public static readonly RoadSegmentCategory PrimaryRoadIIType2 =
        new(
            nameof(PrimaryRoadIIType2),
            new DutchTranslation(
                "PII-2",
                "primaire weg II type 2",
                "De weg verzorgt een verzamelfunctie binnen een regionaalstedelijk of kleinstedelijk gebied. De weg kan onderdeel zijn van een stedelijke ring."
            )
        );

    public static readonly RoadSegmentCategory PrimaryRoadIIType3 =
        new(
            nameof(PrimaryRoadIIType3),
            new DutchTranslation(
                "PII-3",
                "primaire weg II type 3",
                "De weg verzorgt de verzamelfunctie voor een kleinstedelijk of regionaalstedelijk gebied, of toeristisch-recreatief knooppunt van Vlaams niveau."
            )
        );

    public static readonly RoadSegmentCategory PrimaryRoadIIType4 =
        new(
            nameof(PrimaryRoadIIType4),
            new DutchTranslation(
                "PII-4",
                "primaire weg II type 4",
                "De aansluiting (= op- en afrittencomplex) verzorgt een verzamelfunctie voor een kleinstedelijk gebied, overig economisch knooppunt of voor een stedelijk of economisch netwerk op internationaal en Vlaams niveau."
            )
        );

    public static readonly RoadSegmentCategory SecondaryRoad =
        new(
            nameof(SecondaryRoad),
            new DutchTranslation(
                "S",
                "secundaire weg",
                "Wegen die een belangrijke rol spelen in het ontsluiten van gebieden naar de primaire wegen en naar de hoofdwegen en die tevens op lokaal niveau van belang zijn voor de bereikbaarheid van de diverse activiteiten langsheen deze wegen."
            )
        );

    public static readonly RoadSegmentCategory SecondaryRoadType1 =
        new(
            nameof(SecondaryRoadType1),
            new DutchTranslation(
                "S1",
                "secundaire weg type 1",
                "De weg verzorgt een verbindende functie en verkleint een maas, maar functioneert niet als verbinding op Vlaams niveau, en wordt bijgevolg niet aangeduid als primaire weg I."
            )
        );

    public static readonly RoadSegmentCategory SecondaryRoadType2 =
        new(
            nameof(SecondaryRoadType2),
            new DutchTranslation(
                "S2",
                "secundaire weg type 2",
                "De weg verzorgt een verzamelfunctie voor het kleinstedelijk gebied naar het hoofdwegennet, maar kan niet als primaire weg II worden geselecteerd."
            )
        );

    public static readonly RoadSegmentCategory SecondaryRoadType3 =
        new(
            nameof(SecondaryRoadType3),
            new DutchTranslation(
                "S3",
                "secundaire weg type 3",
                "De weg verzorgt een verzamelfunctie voor een gebied dat niet geselecteerd is als stedelijk gebied, poort of toeristisch-recreatief knooppunt op Vlaams niveau en kan bijgevolg niet als primaire weg II geselecteerd worden."
            )
        );

    public static readonly RoadSegmentCategory SecondaryRoadType4 =
        new(
            nameof(SecondaryRoadType4),
            new DutchTranslation(
                "S4",
                "secundaire weg type 4",
                "De weg had oorspronkelijk een verbindende functie op Vlaams niveau als \"steenweg\". Deze functie wordt door een autosnelweg (hoofdweg) overgenomen. Momenteel heeft de weg een verbindings- en verzamelfunctie op (boven-)lokaal niveau."
            )
        );

    public static readonly RoadSegmentCategory EuropeanMainRoad =
        new(
            nameof(EuropeanMainRoad),
            new DutchTranslation(
                "EHW",
                "europese hoofdweg",
                "Europese hoofdwegen"
            )
        );

    public static readonly RoadSegmentCategory FlemishMainRoad =
        new(
            nameof(FlemishMainRoad),
            new DutchTranslation(
                "VHW",
                "vlaamse hoofdweg",
                "Vlaamse hoofdwegen"
            )
        );

    public static readonly RoadSegmentCategory RegionalRoad =
        new(
            nameof(RegionalRoad),
            new DutchTranslation(
                "RW",
                "regionale weg",
                "Regionale wegen"
            )
        );

    public static readonly RoadSegmentCategory InterLocalRoad =
        new(
            nameof(InterLocalRoad),
            new DutchTranslation(
                "IW",
                "interlokale weg",
                "Interlokale wegen"
            )
        );

    public static readonly RoadSegmentCategory LocalAccessRoad =
        new(
            nameof(LocalAccessRoad),
            new DutchTranslation(
                "OW",
                "lokale onstsluitingsweg",
                "Lokale onstsluitingsweg"
            )
        );

    public static readonly RoadSegmentCategory LocalHeritageAccessRoad =
        new(
            nameof(LocalHeritageAccessRoad),
            new DutchTranslation(
                "EW",
                "lokale erftoegangsweg",
                "Lokale erftoegangsweg"
            )
        );

    public static readonly RoadSegmentCategory Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                "-8",
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentCategory NotApplicable =
        new(
            nameof(NotApplicable),
            new DutchTranslation(
                "-9",
                "niet van toepassing",
                "Niet van toepassing"
            )
        );

    public static readonly RoadSegmentCategory[] All =
    [
        Unknown,
        NotApplicable,

        // obsolete
        MainRoad,
        LocalRoad,
        LocalRoadType1,
        LocalRoadType2,
        LocalRoadType3,
        PrimaryRoadI,
        PrimaryRoadII,
        PrimaryRoadIIType1,
        PrimaryRoadIIType2,
        PrimaryRoadIIType3,
        PrimaryRoadIIType4,
        SecondaryRoad,
        SecondaryRoadType1,
        SecondaryRoadType2,
        SecondaryRoadType3,
        SecondaryRoadType4,

        // upgraded
        EuropeanMainRoad,
        FlemishMainRoad,
        RegionalRoad,
        InterLocalRoad,
        LocalAccessRoad,
        LocalHeritageAccessRoad
    ];

    public static readonly RoadSegmentCategory[] Obsolete =
    [
        MainRoad,
        LocalRoad,
        LocalRoadType1,
        LocalRoadType2,
        LocalRoadType3,
        PrimaryRoadI,
        PrimaryRoadII,
        PrimaryRoadIIType1,
        PrimaryRoadIIType2,
        PrimaryRoadIIType3,
        PrimaryRoadIIType4,
        SecondaryRoad,
        SecondaryRoadType1,
        SecondaryRoadType2,
        SecondaryRoadType3,
        SecondaryRoadType4
    ];
    private static readonly RoadSegmentCategory[] _upgraded =
    [
        EuropeanMainRoad,
        FlemishMainRoad,
        RegionalRoad,
        InterLocalRoad,
        LocalAccessRoad,
        LocalHeritageAccessRoad
    ];

    public sealed record Edit
    {
        public static readonly ImmutableArray<RoadSegmentCategory> Editable = [..All.Except(Obsolete)];
    }

    public static bool IsUpgraded(RoadSegmentCategory category)
    {
        return _upgraded.Contains(category);
    }

    public static readonly IReadOnlyDictionary<string, RoadSegmentCategory> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier, StringComparer.InvariantCultureIgnoreCase);

    private readonly string _value;

    private RoadSegmentCategory(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentCategory type && Equals(type);
    }

    public bool Equals(RoadSegmentCategory other)
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

    public static bool operator ==(RoadSegmentCategory left, RoadSegmentCategory right)
    {
        return Equals(left, right);
    }

    public static implicit operator string(RoadSegmentCategory instance)
    {
        return instance?.ToString();
    }

    public static bool operator !=(RoadSegmentCategory left, RoadSegmentCategory right)
    {
        return !Equals(left, right);
    }

    public static RoadSegmentCategory Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known road segment category.");

        return parsed;
    }
    public static RoadSegmentCategory ParseUsingDutchName(string value)
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

    public static bool TryParse(string value, out RoadSegmentCategory parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentCategory parsed)
    {
        parsed = Array.Find(All, candidate => candidate.Translation.Name == value);
        return parsed != null;
    }

    public class DutchTranslation
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
