using System.Collections.Generic;

namespace RoadRegistry.Events
{
    public enum RoadSegmentGeometryDrawMethod
    {
        Outlined = 1, // 1	ingeschetst	Wegsegment waarvan de geometrie ingeschetst werd.
        Measured = 2, // 2	ingemeten	Wegsegment waarvan de geometrie ingemeten werd (bv. overgenomen uit as-built-plan of andere dataset).
        Measured_according_to_GRB_specifications = 3 // 3	ingemeten volgens GRB-specificaties	Wegsegment waarvan de geometrie werd ingemeten volgens GRB-specificaties.
    }

    public class RoadSegmentGeometryDrawMethodTranslator : EnumTranslator<RoadSegmentGeometryDrawMethod>
    {
        protected override IDictionary<RoadSegmentGeometryDrawMethod, string> DutchTranslations => _dutchTranslation;
        private static readonly IDictionary<RoadSegmentGeometryDrawMethod, string> _dutchTranslation =
            new Dictionary<RoadSegmentGeometryDrawMethod, string>
            {
                { RoadSegmentGeometryDrawMethod.Outlined, "ingeschetst" },
                { RoadSegmentGeometryDrawMethod.Measured, "ingemeten" },
                { RoadSegmentGeometryDrawMethod.Measured_according_to_GRB_specifications, "ingemeten volgens GRB-specificaties" },
            };
    }
}
