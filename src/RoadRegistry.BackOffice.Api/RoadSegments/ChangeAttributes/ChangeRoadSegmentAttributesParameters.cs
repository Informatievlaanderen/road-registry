namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using Infrastructure;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract(Name = "WegsegmentAttribuutWijzigen", Namespace = "")]
[CustomSwaggerSchemaId("WegsegmentAttribuutWijzigen")]
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
                Wegsegmenten = [481110, 481111],
                Wegbeheerder = "AWV112"
            },
            new()
            {
                Wegsegmenten = [481111],
                Wegsegmentstatus = RoadSegmentStatus.OutOfUse.ToDutchString(),
                MorfologischeWegklasse = RoadSegmentMorphology.PrimitiveRoad.ToDutchString(),
                Wegbeheerder = "AWV114",
                EuropeseWegen =
                [
                    new ChangeAttributeEuropeanRoad
                    {
                        EuNummer = "E40"
                    }
                ],
                NationaleWegen =
                [
                    new ChangeAttributeNationalRoad
                    {
                        Ident2 = "N180"
                    }
                ],
                GenummerdeWegen =
                [
                    new ChangeAttributeNumberedRoad
                    {
                        Ident8 = "N0080001",
                        Richting = RoadSegmentNumberedRoadDirection.Forward.ToDutchString(),
                        Volgnummer = new RoadSegmentNumberedRoadOrdinal(2686).ToDutchString()
                    }
                ],
                LinkerstraatnaamId = "https://data.vlaanderen.be/id/straatnaam/1",
                RechterstraatnaamId = StreetNameLocalId.NotApplicable.ToDutchString() // todo-rik valideren met Erik
            }
        };
    }
}
