namespace RoadRegistry.Shared
{
    using System.Collections.Generic;

    public enum RoadSegmentAccessRestriction
    {
        PublicRoad = 1,
        PhysicallyImpossible = 2,
        LegallyForbidden = 3,
        PrivateRoad = 4,
        Seasonal = 5,
        Toll = 6,
    }

    public class RoadSegmentAccessRestrictionTranslator : EnumTranslator<RoadSegmentAccessRestriction>
    {
        protected override IDictionary<RoadSegmentAccessRestriction, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<RoadSegmentAccessRestriction, string> DutchDescriptions => _dutchDescriptions;

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

        private static readonly IDictionary<RoadSegmentAccessRestriction, string> _dutchDescriptions =
            new Dictionary<RoadSegmentAccessRestriction, string>
            {
                { RoadSegmentAccessRestriction.PublicRoad, "Weg is publiek toegankelijk." },
                { RoadSegmentAccessRestriction.PhysicallyImpossible, "Weg is niet toegankelijk vanwege de aanwezigheid van hindernissen of obstakels." },
                { RoadSegmentAccessRestriction.LegallyForbidden, "Toegang tot de weg is bij wet verboden." },
                { RoadSegmentAccessRestriction.PrivateRoad, "Toegang tot de weg is beperkt aangezien deze een private eigenaar heeft." },
                { RoadSegmentAccessRestriction.Seasonal, "Weg is afhankelijk van het seizoen (on)toegankelijk." },
                { RoadSegmentAccessRestriction.Toll, "Toegang tot de weg is onderhevig aan tolheffingen." },
            };
    }
}
