namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Swashbuckle.AspNetCore.Filters;

public class ChangeRoadSegmentAttributesParametersExamples : IExamplesProvider<ChangeRoadSegmentAttributesParameters>
{
    public ChangeRoadSegmentAttributesParameters GetExamples()
    {
        return new ChangeRoadSegmentAttributesParameters
        {
            Wegsegmentstatus = RoadSegmentStatus.InUse.Translation.Name,
            MorfologischeWegklasse = RoadSegmentMorphology.PrimitiveRoad.Translation.Name,
            Toegangsbeperking = RoadSegmentAccessRestriction.PublicRoad.Translation.Name,
            Wegbeheerder = "44021",
            Wegcategorie = RoadSegmentCategory.LocalRoad.Translation.Name,
        };
    }
}
