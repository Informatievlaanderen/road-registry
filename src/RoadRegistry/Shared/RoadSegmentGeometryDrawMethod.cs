namespace RoadRegistry.Shared
{
    using System.Collections.Generic;

    public enum RoadSegmentGeometryDrawMethod
    {
        Outlined = 1,
        Measured = 2,
        Measured_according_to_GRB_specifications = 3
    }

    public class RoadSegmentGeometryDrawMethodTranslator : EnumTranslator<RoadSegmentGeometryDrawMethod>
    {
        protected override IDictionary<RoadSegmentGeometryDrawMethod, string> DutchTranslations => _dutchTranslations;
        protected override IDictionary<RoadSegmentGeometryDrawMethod, string> DutchDescriptions => _dutchDescriptions;

        private static readonly IDictionary<RoadSegmentGeometryDrawMethod, string> _dutchTranslations =
            new Dictionary<RoadSegmentGeometryDrawMethod, string>
            {
                { RoadSegmentGeometryDrawMethod.Outlined, "ingeschetst" },
                { RoadSegmentGeometryDrawMethod.Measured, "ingemeten" },
                { RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications, "ingemeten volgens GRB-specificaties" },
            };

        private static readonly IDictionary<RoadSegmentGeometryDrawMethod, string> _dutchDescriptions =
            new Dictionary<RoadSegmentGeometryDrawMethod, string>
            {
                { RoadSegmentGeometryDrawMethod.Outlined, "Wegsegment waarvan de geometrie ingeschetst werd." },
                { RoadSegmentGeometryDrawMethod.Measured, "Wegsegment waarvan de geometrie ingemeten werd (bv. overgenomen uit as-built-plan of andere dataset)." },
                { RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications, "Wegsegment waarvan de geometrie werd ingemeten volgens GRB-specificaties." },
            };
    }
}
