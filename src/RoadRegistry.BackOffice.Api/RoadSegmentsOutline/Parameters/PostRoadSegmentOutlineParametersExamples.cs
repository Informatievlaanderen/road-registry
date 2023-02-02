namespace RoadRegistry.BackOffice.Api.RoadSegmentsOutline.Parameters;

using Swashbuckle.AspNetCore.Filters;

public class PostRoadSegmentOutlineParametersExamples : IExamplesProvider<PostRoadSegmentOutlineParameters>
{
    public PostRoadSegmentOutlineParameters GetExamples()
    {
        return new PostRoadSegmentOutlineParameters
        {
            MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList>
</gml:LineString>",
            Wegsegmentstatus = RoadSegmentStatus.InUse.Translation.Name,
            MorfologischeWegklasse = RoadSegmentMorphology.PrimitiveRoad.Translation.Name,
            Toegangsbeperking = RoadSegmentAccessRestriction.PublicRoad.Translation.Name,
            Wegbeheerder = "44021",
            Wegverharding = RoadSegmentSurfaceType.SolidSurface.Translation.Name,
            Wegbreedte = 5,
            AantalRijstroken = new RoadSegmentLaneParameters
            {
                Aantal = 2,
                Richting = RoadSegmentLaneDirection.Forward.Translation.Name
            }
        };
    }
}
