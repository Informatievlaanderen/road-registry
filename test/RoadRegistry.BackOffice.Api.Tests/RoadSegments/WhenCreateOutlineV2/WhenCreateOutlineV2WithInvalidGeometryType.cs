namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Tests;
using RoadRegistry.ValueObjects.ProblemCodes;

public class WhenCreateOutlineV2WithInvalidGeometryType
{
    private const string GmlPoint = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:pos>217368.75 181577.016</gml:pos>
</gml:Point>";

    private const string GmlMultiLineString = @"<gml:MultiLineString srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:lineStringMember><gml:LineString><gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList></gml:LineString></gml:lineStringMember>
</gml:MultiLineString>";

    private const string GmlPolygon = @"<gml:Polygon srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:exterior><gml:LinearRing><gml:posList>0 0 10 0 10 10 0 10 0 0</gml:posList></gml:LinearRing></gml:exterior>
</gml:Polygon>";

    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());

    [Theory]
    [InlineData(GmlPoint)]
    [InlineData(GmlMultiLineString)]
    [InlineData(GmlPolygon)]
    public async Task WhenGeometryIsNotALineString_ThenGeometryNotValidError(string gml)
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters { WegsegmentGeometrie = gml };

        var result = await _validator.ValidateAsync(parameters);

        Assert.Contains(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Geometry.NotValid.ToString());
    }
}
