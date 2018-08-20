namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum GradeSeparatedJunctionType
    {
        Unknown = -8,
        Tunnel = 1,
        Bridge = 2
    }

    public class GradeSeparatedJunctionTypeTranslator : EnumTranslator<GradeSeparatedJunctionType>
    {
        protected override IDictionary<GradeSeparatedJunctionType, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<GradeSeparatedJunctionType, string> DutchDescriptions => _dutchDescriptions;

        private static readonly IDictionary<GradeSeparatedJunctionType, string> _dutchTranslations =
            new Dictionary<GradeSeparatedJunctionType, string>
            {
                { GradeSeparatedJunctionType.Unknown, "niet gekend" },
                { GradeSeparatedJunctionType.Tunnel, "tunnel" },
                { GradeSeparatedJunctionType.Bridge, "brug" },
            };

        private static readonly IDictionary<GradeSeparatedJunctionType, string> _dutchDescriptions =
            new Dictionary<GradeSeparatedJunctionType, string>
            {
                { GradeSeparatedJunctionType.Unknown, "Geen informatie beschikbaar" },
                { GradeSeparatedJunctionType.Tunnel, "Een tunnel is een doorgang voor een weg, spoorweg, aardeweg of pad die onder de grond, onder water of in een langwerpige, overdekte uitgraving is gelegen." },
                { GradeSeparatedJunctionType.Bridge, "Een brug is een doorgang voor een weg, spoorweg, aardeweg of pad die boven de grond of boven water gelegen is. Een brug kan vast of beweegbaar zijn." },
            };
    }
}
