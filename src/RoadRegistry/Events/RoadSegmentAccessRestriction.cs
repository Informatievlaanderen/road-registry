namespace RoadRegistry.Events
{
    public enum RoadSegmentAccessRestriction
    {
        PublicRoad, // 1	openbare weg	Weg is publiek toegankelijk.
        PhysicallyImpossible, // 2	onmogelijke toegang	Weg is niet toegankelijk vanwege de aanwezigheid van hindernissen of obstakels.
        LegallyForbidden, // 3	verboden toegang	Toegang tot de weg is bij wet verboden.
        PrivateRoad, // 4	privaatweg	Toegang tot de weg is beperkt aangezien deze een private eigenaar heeft.
        Seasonal, // 5	seizoensgebonden toegang	Weg is afhankelijk van het seizoen (on)toegankelijk.
        Toll, // 6	tolweg	Toegang tot de weg is onderhevig aan tolheffingen.
    }
}