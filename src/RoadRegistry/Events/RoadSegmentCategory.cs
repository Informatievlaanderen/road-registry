namespace RoadRegistry.Events
{
    public enum RoadSegmentCategory
    {
        Unknown, // -8	niet gekend	Geen informatie beschikbaar
        NotApplicable, // -9	niet van toepassing	Niet van toepassing
        MainRoad, // H	hoofdweg	Wegen die de verbindingsfunctie verzorgen voor de grootstedelijke- en regionaalstedelijke gebieden met elkaar, met het Brussels Hoofdstedelijk Gewest en met de groot- en regionaalstedelijke gebieden in Wallonië en de buurlanden.
        LocalRoad, // L	lokale weg	Lokale wegen zijn wegen waar het toegang geven de belangrijkste functie is en zijn aldus niet van gewestelijk belang.
        LocalRoadType1, // L1	lokale weg type 1	Lokale verbindingsweg
        LocalRoadType2, // L2	lokale weg type 2	Lokale gebiedsontsluitingsweg
        LocalRoadType3, // L3	lokale weg type 3	Lokale erftoegangsweg
        PrimaryRoadI, // PI	primaire weg I	Wegen die noodzakelijk zijn om het net van hoofdwegen te complementeren, maar die geen functie hebben als doorgaande, internationale verbinding.
        PrimaryRoadII, // PII	primaire weg II	Wegen die een verzamelfunctie hebben voor gebieden en/of concentraties van activiteiten van gewestelijk belang.
        PrimaryRoadIIType1, // PII-1	primaire weg II type 1	De weg verzorgt binnen een grootstedelijk gebied of een poort de verbindings- en verzamelfunctie voor het geheel van het stedelijk gebied of de poort.
        PrimaryRoadIIType2, // PII-2	primaire weg II type 2	De weg verzorgt een verzamelfunctie binnen een regionaalstedelijk of kleinstedelijk gebied. De weg kan onderdeel zijn van een stedelijke ring.
        PrimaryRoadIIType3, // PII-3	primaire weg II type 3	De weg verzorgt de verzamelfunctie voor een kleinstedelijk of regionaalstedelijk gebied, of toeristisch-recreatief knooppunt van Vlaams niveau.
        PrimaryRoadIIType4, // PII-4	primaire weg II type 4	De aansluiting (= op- en afrittencomplex) verzorgt een verzamelfunctie voor een kleinstedelijk gebied, overig economisch knooppunt of voor een stedelijk of economisch netwerk op internationaal en Vlaams niveau.
        SecondaryRoad, // S	secundaire weg	Wegen die een belangrijke rol spelen in het ontsluiten van gebieden naar de primaire wegen en naar de hoofdwegen en die tevens op lokaal niveau van belang zijn voor de bereikbaarheid van de diverse activiteiten langsheen deze wegen.
        SecondaryRoadType1, // S1	secundaire weg type 1	De weg verzorgt een verbindende functie en verkleint een maas, maar functioneert niet als verbinding op Vlaams niveau, en wordt bijgevolg niet aangeduid als primaire weg I.
        SecondaryRoadType2, // S2	secundaire weg type 2	De weg verzorgt een verzamelfunctie voor het kleinstedelijk gebied naar het hoofdwegennet, maar kan niet als primaire weg II worden geselecteerd.
        SecondaryRoadType3, // S3	secundaire weg type 3	De weg verzorgt een verzamelfunctie voor een gebied dat niet geselecteerd is als stedelijk gebied, poort of toeristisch-recreatief knooppunt op Vlaams niveau en kan bijgevolg niet als primaire weg II geselecteerd worden.
        SecondaryRoadType4 // S4	secundaire weg type 4	De weg had oorspronkelijk een verbindende functie op Vlaams niveau als “steenweg”. Deze functie wordt door een autosnelweg (hoofdweg) overgenomen. Momenteel heeft de weg een verbindings- en verzamelfunctie op (boven-)lokaal niveau.
    }
}