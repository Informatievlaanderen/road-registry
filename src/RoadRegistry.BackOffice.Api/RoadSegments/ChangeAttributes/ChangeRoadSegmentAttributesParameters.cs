namespace RoadRegistry.BackOffice.Api.RoadSegments.ChangeAttributes;

using System.Collections.Generic;
using System.Runtime.Serialization;
using Swashbuckle.AspNetCore.Filters;

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
                Wegbeheerder = "AWV114"
            }
        };
    }
}
