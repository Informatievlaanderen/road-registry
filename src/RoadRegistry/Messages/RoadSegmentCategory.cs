namespace RoadRegistry.Messages
{
    using System;
    using System.Collections.Generic;

    public enum RoadSegmentCategory
    {
        Unknown,
        NotApplicable,
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
    }

    public class RoadSegmentCategoryTranslator : EnumTranslator<RoadSegmentCategory>
    {
        protected override IDictionary<RoadSegmentCategory, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<RoadSegmentCategory, string> DutchDescriptions => _dutchDescriptions;

        public new string TranslateToIdentifier(RoadSegmentCategory category)
        {
            return Codes.ContainsKey(category) ? Codes[category] : throw new NotImplementedException($"Identifier not set for {category}");
        }

        private static readonly IDictionary<RoadSegmentCategory, string> Codes =
            new Dictionary<RoadSegmentCategory, string>
            {
                { RoadSegmentCategory.Unknown, "-8" },
                { RoadSegmentCategory.NotApplicable, "-9" },
                { RoadSegmentCategory.MainRoad, "H" },
                { RoadSegmentCategory.LocalRoad, "L" },
                { RoadSegmentCategory.LocalRoadType1, "L1" },
                { RoadSegmentCategory.LocalRoadType2, "L2" },
                { RoadSegmentCategory.LocalRoadType3, "L3" },
                { RoadSegmentCategory.PrimaryRoadI, "PI" },
                { RoadSegmentCategory.PrimaryRoadII, "PII" },
                { RoadSegmentCategory.PrimaryRoadIIType1, "PII-1" },
                { RoadSegmentCategory.PrimaryRoadIIType2, "PII-2" },
                { RoadSegmentCategory.PrimaryRoadIIType3, "PII-3" },
                { RoadSegmentCategory.PrimaryRoadIIType4, "PII-4" },
                { RoadSegmentCategory.SecondaryRoad, "S" },
                { RoadSegmentCategory.SecondaryRoadType1, "S1" },
                { RoadSegmentCategory.SecondaryRoadType2, "S2" },
                { RoadSegmentCategory.SecondaryRoadType3, "S3" },
                { RoadSegmentCategory.SecondaryRoadType4, "S4" },
            };

        private static readonly IDictionary<RoadSegmentCategory, string> _dutchTranslations =
            new Dictionary<RoadSegmentCategory, string>
            {
                { RoadSegmentCategory.Unknown, "niet gekend" },
                { RoadSegmentCategory.NotApplicable, "niet van toepassing" },
                { RoadSegmentCategory.MainRoad, "hoofdweg" },
                { RoadSegmentCategory.LocalRoad, "lokale weg" },
                { RoadSegmentCategory.LocalRoadType1, "lokale weg type 1" },
                { RoadSegmentCategory.LocalRoadType2, "lokale weg type 2" },
                { RoadSegmentCategory.LocalRoadType3, "lokale weg type 3" },
                { RoadSegmentCategory.PrimaryRoadI, "primaire weg I" },
                { RoadSegmentCategory.PrimaryRoadII, "primaire weg II" },
                { RoadSegmentCategory.PrimaryRoadIIType1, "primaire weg II type 1" },
                { RoadSegmentCategory.PrimaryRoadIIType2, "primaire weg II type 2" },
                { RoadSegmentCategory.PrimaryRoadIIType3, "primaire weg II type 3" },
                { RoadSegmentCategory.PrimaryRoadIIType4, "primaire weg II type 4" },
                { RoadSegmentCategory.SecondaryRoad, "secundaire weg" },
                { RoadSegmentCategory.SecondaryRoadType1, "secundaire weg type 1" },
                { RoadSegmentCategory.SecondaryRoadType2, "secundaire weg type 2" },
                { RoadSegmentCategory.SecondaryRoadType3, "secundaire weg type 3" },
                { RoadSegmentCategory.SecondaryRoadType4, "secundaire weg type 4" },
            };

        private static readonly IDictionary<RoadSegmentCategory, string> _dutchDescriptions =
            new Dictionary<RoadSegmentCategory, string>{
                { RoadSegmentCategory.Unknown, "Geen informatie beschikbaar" },
                { RoadSegmentCategory.NotApplicable, "Niet van toepassing" },
                { RoadSegmentCategory.MainRoad, "Wegen die de verbindingsfunctie verzorgen voor de grootstedelijke- en regionaalstedelijke gebieden met elkaar, met het Brussels Hoofdstedelijk Gewest en met de groot- en regionaalstedelijke gebieden in Wallonië en de buurlanden." },
                { RoadSegmentCategory.LocalRoad, "Lokale wegen zijn wegen waar het toegang geven de belangrijkste functie is en zijn aldus niet van gewestelijk belang." },
                { RoadSegmentCategory.LocalRoadType1, "Lokale verbindingsweg" },
                { RoadSegmentCategory.LocalRoadType2, "Lokale gebiedsontsluitingsweg" },
                { RoadSegmentCategory.LocalRoadType3, "Lokale erftoegangsweg" },
                { RoadSegmentCategory.PrimaryRoadI, "Wegen die noodzakelijk zijn om het net van hoofdwegen te complementeren, maar die geen functie hebben als doorgaande, internationale verbinding." },
                { RoadSegmentCategory.PrimaryRoadII, "Wegen die een verzamelfunctie hebben voor gebieden en/of concentraties van activiteiten van gewestelijk belang." },
                { RoadSegmentCategory.PrimaryRoadIIType1, "De weg verzorgt binnen een grootstedelijk gebied of een poort de verbindings- en verzamelfunctie voor het geheel van het stedelijk gebied of de poort." },
                { RoadSegmentCategory.PrimaryRoadIIType2, "De weg verzorgt een verzamelfunctie binnen een regionaalstedelijk of kleinstedelijk gebied. De weg kan onderdeel zijn van een stedelijke ring." },
                { RoadSegmentCategory.PrimaryRoadIIType3, "De weg verzorgt de verzamelfunctie voor een kleinstedelijk of regionaalstedelijk gebied, of toeristisch-recreatief knooppunt van Vlaams niveau." },
                { RoadSegmentCategory.PrimaryRoadIIType4, "De aansluiting (= op- en afrittencomplex) verzorgt een verzamelfunctie voor een kleinstedelijk gebied, overig economisch knooppunt of voor een stedelijk of economisch netwerk op internationaal en Vlaams niveau." },
                { RoadSegmentCategory.SecondaryRoad, "Wegen die een belangrijke rol spelen in het ontsluiten van gebieden naar de primaire wegen en naar de hoofdwegen en die tevens op lokaal niveau van belang zijn voor de bereikbaarheid van de diverse activiteiten langsheen deze wegen." },
                { RoadSegmentCategory.SecondaryRoadType1, "De weg verzorgt een verbindende functie en verkleint een maas, maar functioneert niet als verbinding op Vlaams niveau, en wordt bijgevolg niet aangeduid als primaire weg I." },
                { RoadSegmentCategory.SecondaryRoadType2, "De weg verzorgt een verzamelfunctie voor het kleinstedelijk gebied naar het hoofdwegennet, maar kan niet als primaire weg II worden geselecteerd." },
                { RoadSegmentCategory.SecondaryRoadType3, "De weg verzorgt een verzamelfunctie voor een gebied dat niet geselecteerd is als stedelijk gebied, poort of toeristisch-recreatief knooppunt op Vlaams niveau en kan bijgevolg niet als primaire weg II geselecteerd worden." },
                { RoadSegmentCategory.SecondaryRoadType4, "De weg had oorspronkelijk een verbindende functie op Vlaams niveau als \"steenweg\". Deze functie wordt door een autosnelweg (hoofdweg) overgenomen. Momenteel heeft de weg een verbindings- en verzamelfunctie op (boven-)lokaal niveau." },
            };
    }
}
