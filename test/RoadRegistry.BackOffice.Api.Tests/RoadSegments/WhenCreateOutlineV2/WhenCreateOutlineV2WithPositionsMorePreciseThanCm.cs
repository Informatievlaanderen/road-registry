namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using System.Linq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.ValueObjects.ProblemCodes;

public class WhenCreateOutlineV2WithPositionsMorePreciseThanCm
{
    private static readonly string ExpectedErrorCode = ProblemCode.RoadSegment.PositionMorePreciseThanCm.ToString();

    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());

    [Theory]
    [InlineData(0.12)]
    [InlineData(123.45)]
    [InlineData(99999.99)]
    [InlineData(0.30000000000000004)]
    [InlineData(0.1200000002)]
    [InlineData(0.11999999731779099)]
    public void WhenValueIsACentimetreOrFloatingPointNoiseAroundIt_ThenItIsNotMorePrecise(double value)
    {
        Assert.False(value.IsMorePreciseThanCm());
    }

    [Theory]
    [InlineData(0.125)]
    [InlineData(0.004)]
    [InlineData(0.12001)]
    [InlineData(123.44999694824219)]
    public void WhenValueCarriesMoreThanACentimetre_ThenItIsMorePrecise(double value)
    {
        Assert.True(value.IsMorePreciseThanCm());
    }

    [Fact]
    public async Task WhenTotPositieHasThreeDecimals_ThenErrorTranslatesToDutchWithThePosition()
    {
        var result = await _validator.ValidateAsync(WithMorfologie(0, 9.125));

        Assert.Contains(result.Errors, e => e.ErrorCode == ExpectedErrorCode);

        var translated = result.Errors.TranslateToDutch(WellKnownProblemTranslators.Default).ToList();
        var error = Assert.Single(translated, e => e.ErrorCode == "PositieNauwkeurigerDanCentimeter");
        Assert.Contains("9.125", error.ErrorMessage);
    }

    [Fact]
    public async Task WhenTotPositieOnlyRoundsToZero_ThenItIsNotTreatedAsTheEndOfTheSegment()
    {
        var result = await _validator.ValidateAsync(WithMorfologie(0, 0.004));

        Assert.Contains(result.Errors, e => e.ErrorCode == ExpectedErrorCode);
    }

    [Fact]
    public async Task WhenPositionsOnlyCarryFloatingPointNoise_ThenTheyAreAccepted()
    {
        var result = await _validator.ValidateAsync(WithMorfologie(0.0000000002, 10.0000000002));

        Assert.DoesNotContain(result.Errors, e => e.ErrorCode == ExpectedErrorCode);
        Assert.DoesNotContain(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.Morphology.DynamicAttributeProblemCodes.ToPositionNotEqualToLength.ToString());
    }

    [Fact]
    public async Task WhenASidedAttributeHasAPositionOnBothSides_ThenItIsReportedOnce()
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Wegbeheerder =
            [
                new IngeschetstWegsegmentWegbeheerderAttribuutWaarde
                {
                    Kant = RoadSegmentAttributeSide.Links.ToDutchString(),
                    VanPositie = 0,
                    TotPositie = 9.125,
                    Wegbeheerder = "AWV114"
                },
                new IngeschetstWegsegmentWegbeheerderAttribuutWaarde
                {
                    Kant = RoadSegmentAttributeSide.Rechts.ToDutchString(),
                    VanPositie = 0,
                    TotPositie = 9.125,
                    Wegbeheerder = "AWV114"
                }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        Assert.Single(result.Errors, e => e.ErrorCode == ExpectedErrorCode);
    }

    private static CreateOutlinedRoadSegmentV2Parameters WithMorfologie(double vanPositie, double totPositie)
    {
        return new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Morfologie =
            [
                new MorfologieParameters
                {
                    VanPositie = vanPositie,
                    TotPositie = totPositie,
                    Morfologie = RoadSegmentMorphologyV2.Parallelweg.ToDutchString()
                }
            ]
        };
    }
}
