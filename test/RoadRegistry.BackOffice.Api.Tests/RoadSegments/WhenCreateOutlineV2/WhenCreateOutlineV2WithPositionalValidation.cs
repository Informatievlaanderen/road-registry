namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using System.Linq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
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

    [Fact]
    public async Task WhenTotPositieIsBeyondGeometryLength_ThenErrorTranslatesToDutchWithPositions()
    {
        // The test geometry is 10m long, so an attribute running to 20 trips ToPositionNotEqualToLength.
        var parameters = new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Morfologie =
            [
                new WegsegmentMorfologieAttribuutWaarde
                {
                    VanPositie = 0,
                    TotPositie = 20,
                    Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString()
                }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        var expectedErrorCode = ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.ToPositionNotEqualToLength.ToString();
        Assert.Contains(result.Errors, e => e.ErrorCode == expectedErrorCode);

        var translated = result.Errors.TranslateToDutch(WellKnownProblemTranslators.Default).ToList();
        var error = Assert.Single(translated, e => e.ErrorCode == expectedErrorCode);
        Assert.Contains("20", error.ErrorMessage);
        Assert.Contains("10", error.ErrorMessage);
    }

    [Theory]
    [InlineData(2.0, 10.0)]
    [InlineData(0.0, 5.0, 6.0, 10.0)]
    [InlineData(0.0, 0.5)]
    public async Task WhenPositionsAreInvalid_ThenErrorsTranslateToDutchWithoutThrowing(params double[] vanTotPairs)
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Morfologie = vanTotPairs.Chunk(2)
                .Select(pair => new WegsegmentMorfologieAttribuutWaarde
                {
                    VanPositie = pair[0],
                    TotPositie = pair[1],
                    Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString()
                })
                .ToArray()
        };

        var result = await _validator.ValidateAsync(parameters);

        Assert.False(result.IsValid);
        var translated = result.Errors.TranslateToDutch(WellKnownProblemTranslators.Default).ToList();
        Assert.NotEmpty(translated);
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
