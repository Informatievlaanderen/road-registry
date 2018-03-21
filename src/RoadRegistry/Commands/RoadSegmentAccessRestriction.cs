namespace RoadRegistry.Commands
{
    public enum RoadSegmentAccessRestriction
    {
        PublicRoad, // 1	openbare weg	Weg is publiek toegankelijk.
        AccessImpossible, // 2	onmogelijke toegang	Weg is niet toegankelijk vanwege de aanwezigheid van hindernissen of obstakels.
        AccessForbidden, // 3	verboden toegang	Toegang tot de weg is bij wet verboden.
        PrivateRoad, // 4	privaatweg	Toegang tot de weg is beperkt aangezien deze een private eigenaar heeft.
        SeasonalAccess, // 5	seizoensgebonden toegang	Weg is afhankelijk van het seizoen (on)toegankelijk.
        TollRoad, // 6	tolweg	Toegang tot de weg is onderhevig aan tolheffingen.
    }
}