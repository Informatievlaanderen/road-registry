namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract(Name = "WegsegmentAttribuutWijzigen", Namespace = "")]
public class ChangeRoadSegmentAttributesParameters : List<ChangeAttributeParameters>
{
}

public class ChangeRoadSegmentAttributesParametersExamples : IExamplesProvider<ChangeRoadSegmentAttributesParameters>
{
    public ChangeRoadSegmentAttributesParameters GetExamples()
    {
        return new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { 481110, 481111 },
                Wegbeheerder = "AWV112"
            },
            new()
            {
                Wegsegmenten = new[] { 481111 },
                Wegsegmentstatus = "buiten gebruik",
                MorfologischeWegklasse = "aardeweg",
                Wegbeheerder = "AWV114",
                EuropeseWegen = new[]{ "E40" },
                NationaleWegen = new[] { "N180" },
                GenummerdeWegen = new[]
                {
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "N0080001",
                        Richting = "gelijklopend met de digitalisatiezin",
                        Volgnummer = "2686"
                    }
                }
            }
        };
    }
}
