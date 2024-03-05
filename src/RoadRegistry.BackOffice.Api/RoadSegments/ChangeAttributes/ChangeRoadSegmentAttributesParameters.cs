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
                Wegsegmentstatus = RoadSegmentStatus.OutOfUse.ToDutchString(),
                MorfologischeWegklasse = RoadSegmentMorphology.PrimitiveRoad.ToDutchString(),
                Wegbeheerder = "AWV114",
                EuropeseWegen = new[]
                {
                    new ChangeAttributeEuropeanRoad
                    {
                        EuNummer = "E40"
                    }
                },
                NationaleWegen = new[]
                {
                    new ChangeAttributeNationalRoad
                    {
                        Ident2 = "N180"
                    }
                },
                GenummerdeWegen = new[]
                {
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "N0080001",
                        Richting = RoadSegmentNumberedRoadDirection.Forward.ToDutchString(),
                        Volgnummer = new RoadSegmentNumberedRoadOrdinal(2686).ToDutchString()
                    }
                }
            }
        };
    }
}
