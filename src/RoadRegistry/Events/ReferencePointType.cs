namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum ReferencePointType
    {
        Unknown = -8, // -8	niet gekend	Geen informatie beschikbaar
        KilometerMarker = 1, // 1	kilometerpaal	Paaltje met aanduiding van het aantal kilometer vanaf het begin van de weg en van de code van de weg.
        HectometerMarker = 2 // 2	hectometerpaal	Paaltje met aanduiding van het aantal kilometer resp. hectometer vanaf het begin van de weg.
    }

    public class ReferencePointTypeTranslator : EnumTranslator<ReferencePointType>
    {
        protected override IDictionary<ReferencePointType, string> DutchTranslations => _dutchTransLations;
        private static readonly IDictionary<ReferencePointType, string> _dutchTransLations =
            new Dictionary<ReferencePointType, string>
            {
                { ReferencePointType.Unknown, "niet gekend" },
                { ReferencePointType.KilometerMarker, "kilometerpaal" },
                { ReferencePointType.HectometerMarker, "hectometerpaal" },
            };
    }
}
