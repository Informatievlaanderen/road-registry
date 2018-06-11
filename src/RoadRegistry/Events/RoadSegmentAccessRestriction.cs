namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum RoadSegmentAccessRestriction
    {
        PublicRoad = 1, // 1	openbare weg	Weg is publiek toegankelijk.
        PhysicallyImpossible = 2, // 2	onmogelijke toegang	Weg is niet toegankelijk vanwege de aanwezigheid van hindernissen of obstakels.
        LegallyForbidden = 3, // 3	verboden toegang	Toegang tot de weg is bij wet verboden.
        PrivateRoad = 4, // 4	privaatweg	Toegang tot de weg is beperkt aangezien deze een private eigenaar heeft.
        Seasonal = 5, // 5	seizoensgebonden toegang	Weg is afhankelijk van het seizoen (on)toegankelijk.
        Toll = 6, // 6	tolweg	Toegang tot de weg is onderhevig aan tolheffingen.
    }

    public class RoadSegmentAccessRestrictionTranslator : EnumTranslator<RoadSegmentAccessRestriction>
    {
        protected override IDictionary<RoadSegmentAccessRestriction, string> DutchTranslations => _dutchTranslations;
        private static readonly IDictionary<RoadSegmentAccessRestriction, string> _dutchTranslations =
            new Dictionary<RoadSegmentAccessRestriction, string>
            {
                { RoadSegmentAccessRestriction.PublicRoad, "openbare weg" },
                { RoadSegmentAccessRestriction.PhysicallyImpossible, "onmogelijke toegang" },
                { RoadSegmentAccessRestriction.LegallyForbidden, "verboden toegang" },
                { RoadSegmentAccessRestriction.PrivateRoad, "privaatweg" },
                { RoadSegmentAccessRestriction.Seasonal, "seizoensgebonden toegang" },
                { RoadSegmentAccessRestriction.Toll, "tolweg" },
            };
    }
}
