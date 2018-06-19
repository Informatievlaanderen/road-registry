namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum LaneDirection
    {
        Unknown = -8, // -8	niet gekend	Geen informatie beschikbaar
        Forward = 1, // 1	gelijklopend met de digitalisatiezin	Aantal rijstroken slaat op de richting die de digitalisatiezin van het wegsegment volgt.
        Backward = 2, // 2	tegengesteld aan de digitalisatiezin	Aantal rijstroken slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment.
        Independent = 3 // 3	onafhankelijk van de digitalisatiezin	Aantal rijstroken slaat op het totaal in beide richtingen, onafhankelijk van de digitalisatiezin van het wegsegment.
    }

    public class LaneDirectionTranslator : EnumTranslator<LaneDirection>
    {
        protected override IDictionary<LaneDirection, string> DutchTranslations => _dutchTranslations;
        private static readonly IDictionary<LaneDirection, string> _dutchTranslations =
            new Dictionary<LaneDirection, string>
            {
                { LaneDirection.Unknown, "niet gekend" },
                { LaneDirection.Forward, "gelijklopend met de digitalisatiezin" },
                { LaneDirection.Backward, "tegengesteld aan de digitalisatiezin" },
                { LaneDirection.Independent, "onafhankelijk van de digitalisatiezin" },
            };
    }

}
