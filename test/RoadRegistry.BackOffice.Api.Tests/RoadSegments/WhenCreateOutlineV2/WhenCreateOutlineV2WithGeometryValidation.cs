namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using System.Linq;
using FluentAssertions;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.ValueObjects.ProblemCodes;

// The geometry a caller supplies has to be stated in the coordinate system the register works in, at the precision it
// works in. Anything else would be stored as something other than what was sent.
public class WhenCreateOutlineV2WithGeometryValidation
{
    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());

    private static CreateOutlinedRoadSegmentV2Parameters ParametersWith(string gml)
    {
        return new CreateOutlinedRoadSegmentV2Parameters { WegsegmentGeometrie = gml };
    }

    [Theory]
    // Anything that is not Lambert 2008, and a geometry that does not say what it is in - which parses as Lambert 72,
    // the reader's default, so it is refused for the same reason rather than passing unnoticed.
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenAGeometryThatIsNotLambert08_ThenSridError(bool withoutSrsName)
    {
        var gml = withoutSrsName
            ? GeometryTranslatorTestCases.GmlLineStringWithoutSrsName
            : GeometryTranslatorTestCases.ValidGmlLineStringLambert72;

        var result = await _validator.ValidateAsync(ParametersWith(gml));

        result.Errors.Select(x => x.ErrorCode).Should().Contain(ProblemCode.RoadSegment.Geometry.SridNotLambert08.ToString());
    }

    [Fact]
    public async Task GivenAGeometryMorePreciseThanCm_ThenPrecisionError()
    {
        var result = await _validator.ValidateAsync(ParametersWith(GeometryTranslatorTestCases.GmlLineStringLambert08MorePreciseThanCm));

        result.Errors.Select(x => x.ErrorCode).Should().Contain(ProblemCode.RoadSegment.Geometry.HasCoordinatesMorePreciseThanCm.ToString());
    }

    [Fact]
    public async Task GivenALambert08GeometryAtCmPrecision_ThenNeitherError()
    {
        // Two decimals is the precision the register works in, so it must not be mistaken for too much precision.
        var result = await _validator.ValidateAsync(ParametersWith(GeometryTranslatorTestCases.ValidGmlLineStringLambert08));

        result.Errors.Select(x => x.ErrorCode).Should().NotContain(
        [
            ProblemCode.RoadSegment.Geometry.SridNotLambert08.ToString(),
            ProblemCode.RoadSegment.Geometry.HasCoordinatesMorePreciseThanCm.ToString()
        ]);
    }
}
