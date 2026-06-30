namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.ValueObjects.ProblemCodes;

public class WhenCreateOutlineV2WithPositionalValidation
{
    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());

    [Fact]
    public async Task WhenLastTotPositieIsZero_ThenTreatedAsGeometryLength()
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Morfologie =
            [
                new WegsegmentMorfologieAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = 0,
                    Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString()
                }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        Assert.DoesNotContain(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.ToPositionNotEqualToLength.ToString());
        Assert.DoesNotContain(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.HasLengthOfZero.ToString());
    }

    [Theory]
    [InlineData(0.0, 0.5)]
    [InlineData(0.0, 0.999)]
    public async Task WhenTotPositieMinusVanPositieIsLessThanOne_ThenHasLengthOfZeroError(double van, double tot)
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Morfologie =
            [
                new WegsegmentMorfologieAttribuutWaarde
                {
                    VanPositie = van,
                    TotPositie = tot,
                    Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString()
                }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        Assert.Contains(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.HasLengthOfZero.ToString());
    }

    [Theory]
    [InlineData(0.0, 1.0)]
    [InlineData(0.0, 5.0)]
    [InlineData(0.0, 10.0)]
    public async Task WhenTotPositieMinusVanPositieIsAtLeastOne_ThenNoHasLengthOfZeroError(double van, double tot)
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Morfologie =
            [
                new WegsegmentMorfologieAttribuutWaarde
                {
                    VanPositie = van,
                    TotPositie = tot,
                    Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString()
                }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        Assert.DoesNotContain(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.HasLengthOfZero.ToString());
    }
}
