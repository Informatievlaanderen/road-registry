namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum NumberedRoadSegmentDirection
    {
        Unknown = -8, //-8	niet gekend	                            Geen informatie beschikbaar
        Forward = 1,  // 1	gelijklopend met de digitalisatiezin	Nummering weg slaat op de richting die de digitalisatiezin van het wegsegment volgt.
        Backward = 2  // 2	tegengesteld aan de digitalisatiezin	Nummering weg slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment.
    }

    public class NumberedRoadSegmentDirectionTranslator : EnumTranslator<NumberedRoadSegmentDirection>
    {
        protected override IDictionary<NumberedRoadSegmentDirection, string> DutchTranslations => _dutchTranslations;
        private static readonly IDictionary<NumberedRoadSegmentDirection, string> _dutchTranslations =
            new Dictionary<NumberedRoadSegmentDirection, string>
            {
                { NumberedRoadSegmentDirection.Unknown, "niet gekend" },
                { NumberedRoadSegmentDirection.Forward, "gelijklopend met de digitalisatiezin" },
                { NumberedRoadSegmentDirection.Backward, "tegengesteld aan de digitalisatiezin" },
            };
    }
}
