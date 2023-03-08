namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Swashbuckle.AspNetCore.Filters;

public class ChangeRoadSegmentAttributesParametersExamples : IExamplesProvider<ChangeRoadSegmentAttributesParameters>
{
    public ChangeRoadSegmentAttributesParameters GetExamples()
    {
        return new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = "wegbeheerder",
                Attribuutwaarde = "AWV112",
                Wegsegmenten = new[] { 481110,481112 }
            },
            new()
            {
                Attribuut = "wegbeheerder",
                Attribuutwaarde = "AWV114",
                Wegsegmenten = new[] { 481111 }
            },
            new()
            {
                Attribuut = "wegsegmentstatus",
                Attribuutwaarde = "buiten gebruik",
                Wegsegmenten = new[] { 481111 }
            },
            new()
            {
                Attribuut = "morfologischeWegklasse",
                Attribuutwaarde = "aardeweg",
                Wegsegmenten = new[] { 481111 }
            }
        };
    }
}
