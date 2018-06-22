namespace RoadRegistry.Events
{
    using System.Collections.Generic;

    public enum GradeSeparatedJunctionType
    {
        Unknown = -8,   //-8    niet gekend Geen informatie beschikbaar
        Tunnel = 1,     // 1	tunnel	    Een tunnel is een doorgang voor een weg, spoorweg, aardeweg of pad die onder de grond, onder water of in een langwerpige, overdekte uitgraving is gelegen.
        Bridge = 2      // 2	brug	    Een brug is een doorgang voor een weg, spoorweg, aardeweg of pad die boven de grond of boven water gelegen is. Een brug kan vast of beweegbaar zijn.
    }

    public class GradeSeparatedJunctionTypeTranslator : EnumTranslator<GradeSeparatedJunctionType>
    {
        protected override IDictionary<GradeSeparatedJunctionType, string> DutchTranslations => _dutchTranslations;
        private static readonly IDictionary<GradeSeparatedJunctionType, string> _dutchTranslations =
            new Dictionary<GradeSeparatedJunctionType, string>
            {
                { GradeSeparatedJunctionType.Unknown, "niet gekend" },
                { GradeSeparatedJunctionType.Tunnel, "tunnel" },
                { GradeSeparatedJunctionType.Bridge, "brug" },
            };
    }
}
