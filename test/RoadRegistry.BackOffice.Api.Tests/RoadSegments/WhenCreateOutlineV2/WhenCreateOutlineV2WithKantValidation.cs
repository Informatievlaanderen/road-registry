namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using System.Linq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.ValueObjects.ProblemCodes;

public class WhenCreateOutlineV2WithKantValidation
{
    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("onbekend")]
    public async Task WhenKantIsMissingOrUnknown_ThenAttributeSideNotValidError(string? kant)
    {
        // An unusable kant must be refused by the validator: the controller parses it outright, so letting it
        // through would turn the request into a 500.
        var parameters = new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = kant!,
                    VanPositie = 0,
                    TotPositie = 10,
                    Identificator = "https://data.vlaanderen.be/id/straatnaam/71671"
                }
            ],
            Wegbeheerder =
            [
                new IngeschetstWegsegmentWegbeheerderAttribuutWaarde
                {
                    Kant = kant!,
                    VanPositie = 0,
                    TotPositie = 10,
                    Wegbeheerder = "AGIV"
                }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        var expectedErrorCode = ProblemCode.RoadSegment.AttributeSide.NotValid.ToString();
        Assert.Equal(2, result.Errors.Count(e => e.ErrorCode == expectedErrorCode));

        var translated = result.Errors.TranslateToDutch(WellKnownProblemTranslators.Default).ToList();
        var kantErrors = translated.Where(e => e.ErrorCode == "KantNietCorrect").ToList();
        Assert.Equal(2, kantErrors.Count);
        Assert.All(kantErrors, e => Assert.StartsWith("Kant is foutief.", e.ErrorMessage));
    }

    [Theory]
    [InlineData("links")]
    [InlineData("rechts")]
    [InlineData("beide")]
    public async Task WhenKantIsKnown_ThenNoAttributeSideError(string kant)
    {
        var parameters = new CreateOutlinedRoadSegmentV2Parameters
        {
            WegsegmentGeometrie = GeometryTranslatorTestCases.ValidGmlLineStringLambert08,
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde
                {
                    Kant = kant,
                    VanPositie = 0,
                    TotPositie = 10,
                    Identificator = "https://data.vlaanderen.be/id/straatnaam/71671"
                }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        Assert.DoesNotContain(result.Errors,
            e => e.ErrorCode == ProblemCode.RoadSegment.AttributeSide.NotValid.ToString());
    }
}
