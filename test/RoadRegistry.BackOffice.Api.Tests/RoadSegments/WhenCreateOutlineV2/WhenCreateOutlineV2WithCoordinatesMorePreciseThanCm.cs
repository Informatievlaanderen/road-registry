namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Tests;
using RoadRegistry.ValueObjects.ProblemCodes;

public class WhenCreateOutlineV2WithCoordinatesMorePreciseThanCm
{
    // 3 decimal places on Y → more precise than cm
    private const string GmlWithSubCmCoordinates = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.016 217378.75 181577.016</gml:posList>
</gml:LineString>";

    // 2 decimal places maximum → exactly cm-precise
    private const string GmlWithCmPreciseCoordinates = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.02 217378.75 181577.02</gml:posList>
</gml:LineString>";

    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());

    [Fact]
    public async Task WhenACoordinateHasMoreThanTwoDecimalPlaces_ThenError()
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters { WegsegmentGeometrie = GmlWithSubCmCoordinates };

        var result = await _validator.ValidateAsync(parameters);

        Assert.Contains(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Geometry.HasCoordinatesMorePreciseThanCm.ToString());
    }

    [Fact]
    public async Task WhenAllCoordinatesHaveAtMostTwoDecimalPlaces_ThenNoError()
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters { WegsegmentGeometrie = GmlWithCmPreciseCoordinates };

        var result = await _validator.ValidateAsync(parameters);

        Assert.DoesNotContain(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Geometry.HasCoordinatesMorePreciseThanCm.ToString());
    }

    [Theory]
    [InlineData("217368.75 181577.016 217378.75 181577.02")]
    [InlineData("217368.751 181577.02 217378.75 181577.02")]
    [InlineData("217368.75 181577.02 217378.75 181577.019")]
    public async Task WhenAnyCoordinateHasMoreThanTwoDecimalPlaces_ThenError(string posList)
    {
        var gml = $@"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>{posList}</gml:posList>
</gml:LineString>";

        var parameters = new CreateOutlinedRoadSegmentV2Parameters { WegsegmentGeometrie = gml };

        var result = await _validator.ValidateAsync(parameters);

        Assert.Contains(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Geometry.HasCoordinatesMorePreciseThanCm.ToString());
    }
}
