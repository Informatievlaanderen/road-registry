namespace RoadRegistry.BackOffice.Api.RoadSegments.Parameters;

using Swashbuckle.AspNetCore.Filters;

public class PostChangeOutlineGeometryParametersExamples : IExamplesProvider<PostChangeOutlineGeometryParameters>
{
    public PostChangeOutlineGeometryParameters GetExamples()
    {
        return new PostChangeOutlineGeometryParameters
        {
            MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList>
</gml:LineString>"
        };
    }
}
