namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum ReferencePointType
    {
        Unknown = -8,
        KilometerMarker = 1,
        HectometerMarker = 2
    }

    public class ReferencePointTypeTranslator : EnumTranslator<ReferencePointType>
    {
        protected override IDictionary<ReferencePointType, string> DutchTranslations => _dutchTransLations;
        protected override IDictionary<ReferencePointType, string> DutchDescriptions => _dutchDescriptions;

        private static readonly IDictionary<ReferencePointType, string> _dutchTransLations =
            new Dictionary<ReferencePointType, string>
            {
                { ReferencePointType.Unknown, "niet gekend" },
                { ReferencePointType.KilometerMarker, "kilometerpaal" },
                { ReferencePointType.HectometerMarker, "hectometerpaal" },
            };

        private static readonly IDictionary<ReferencePointType, string> _dutchDescriptions =
            new Dictionary<ReferencePointType, string>
            {
                { ReferencePointType.Unknown, "Geen informatie beschikbaar." },
                { ReferencePointType.KilometerMarker, "Paaltje met aanduiding van het aantal kilometer vanaf het begin van de weg en van de code van de weg." },
                { ReferencePointType.HectometerMarker, "Paaltje met aanduiding van het aantal kilometer resp. hectometer vanaf het begin van de weg." },
            };
    }
}
