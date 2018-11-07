namespace RoadRegistry.Messages
{
    using System.Collections.Generic;

    public enum NumberedRoadSegmentDirection
    {
        Unknown = -8,
        Forward = 1,
        Backward = 2
    }

    public class NumberedRoadSegmentDirectionTranslator : EnumTranslator<NumberedRoadSegmentDirection>
    {
        protected override IDictionary<NumberedRoadSegmentDirection, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<NumberedRoadSegmentDirection, string> DutchDescriptions => _dutchDescriptions;

        private static readonly IDictionary<NumberedRoadSegmentDirection, string> _dutchTranslations =
            new Dictionary<NumberedRoadSegmentDirection, string>
            {
                { NumberedRoadSegmentDirection.Unknown, "niet gekend" },
                { NumberedRoadSegmentDirection.Forward, "gelijklopend met de digitalisatiezin" },
                { NumberedRoadSegmentDirection.Backward, "tegengesteld aan de digitalisatiezin" },
            };

        private static readonly IDictionary<NumberedRoadSegmentDirection, string> _dutchDescriptions =
            new Dictionary<NumberedRoadSegmentDirection, string>
            {
                { NumberedRoadSegmentDirection.Unknown, "Geen informatie beschikbaar" },
                { NumberedRoadSegmentDirection.Forward, "Nummering weg slaat op de richting die de digitalisatiezin van het wegsegment volgt." },
                { NumberedRoadSegmentDirection.Backward, "Nummering weg slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment." },
            };
    }
}
