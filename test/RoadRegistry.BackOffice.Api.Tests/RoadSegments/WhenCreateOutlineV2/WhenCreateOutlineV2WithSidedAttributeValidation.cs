namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.ValueObjects.ProblemCodes;

public class WhenCreateOutlineV2WithSidedAttributeValidation
{
    private readonly CreateOutlinedRoadSegmentV2ParametersValidator _validator = new(new FakeOrganizationCache());

    [Theory]
    [InlineData("links", "LinkerstraatnaamNietCorrect", "De linkerstraatnaam 'jibberish' is geen geldige waarde.")]
    [InlineData("rechts", "RechterstraatnaamNietCorrect", "De rechterstraatnaam 'jibberish' is geen geldige waarde.")]
    [InlineData("beide", "StraatnaamNietCorrect", "De straatnaam 'jibberish' is geen geldige waarde.")]
    public async Task WhenTheStreetNameIdentificatorIsInvalid_ThenTheErrorNamesTheSideItWasGivenFor(string kant, string expectedErrorCode, string expectedErrorMessage)
    {
        // Only the identificator is wrong: the other side (if any) is valid, so both sides are covered.
        var straatnaam = new List<IngeschetstWegsegmentStraatnaamAttribuutWaarde>
        {
            new() { Kant = kant, Identificator = "jibberish" }
        };
        if (kant != "beide")
        {
            straatnaam.Add(new() { Kant = kant == "links" ? "rechts" : "links", Identificator = "https://data.vlaanderen.be/id/straatnaam/71671" });
        }
        var parameters = CreateOutlineV2Parameters.Valid() with { Straatnaam = straatnaam.ToArray() };

        var translated = await ValidateAndTranslate(parameters);

        var error = Assert.Single(translated);
        Assert.Equal(expectedErrorCode, error.ErrorCode);
        Assert.Equal(expectedErrorMessage, error.ErrorMessage);
    }

    [Theory]
    [InlineData("niet van toepassing")]
    [InlineData("Niet van toepassing")]
    [InlineData("NIET VAN TOEPASSING")]
    public async Task WhenTheStreetNameIsNotApplicable_ThenItIsValid(string identificator)
    {
        var parameters = CreateOutlineV2Parameters.Valid() with
        {
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "links", Identificator = "https://data.vlaanderen.be/id/straatnaam/71671" },
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "rechts", Identificator = identificator }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorCode)));
    }

    [Theory]
    [InlineData("links")]
    [InlineData("rechts")]
    public async Task WhenAStreetNameIsGivenForOneSideOnly_ThenNotOnBothSidesError(string kant)
    {
        var parameters = CreateOutlineV2Parameters.Valid() with
        {
            Straatnaam = [new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = kant, Identificator = "https://data.vlaanderen.be/id/straatnaam/71671" }]
        };

        var translated = await ValidateAndTranslate(parameters);

        var error = Assert.Single(translated);
        Assert.Equal("StraatnaamNietLangsBeideKanten", error.ErrorCode);
        Assert.Equal("Er werd geen straatnaam opgegeven langs beide kanten van de weg, over de ganse lengte van het wegsegment.", error.ErrorMessage);
    }

    [Theory]
    [InlineData("links")]
    [InlineData("rechts")]
    public async Task WhenAMaintenanceAuthorityIsGivenForOneSideOnly_ThenNotOnBothSidesError(string kant)
    {
        var parameters = CreateOutlineV2Parameters.Valid() with
        {
            Wegbeheerder = [new IngeschetstWegsegmentWegbeheerderAttribuutWaarde { Kant = kant, Wegbeheerder = "AGIV" }]
        };

        var translated = await ValidateAndTranslate(parameters);

        var error = Assert.Single(translated);
        Assert.Equal("WegbeheerderNietLangsBeideKanten", error.ErrorCode);
        Assert.Equal("Er werd geen wegbeheerder opgegeven langs beide kanten van de weg, over de ganse lengte van het wegsegment.", error.ErrorMessage);
    }

    [Fact]
    public async Task WhenTheSidesAreGivenSeparately_ThenBothSidesAreCovered()
    {
        var parameters = CreateOutlineV2Parameters.Valid() with
        {
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "links", Identificator = "https://data.vlaanderen.be/id/straatnaam/71671" },
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "rechts", VanPositie = 0, TotPositie = 4, Identificator = "https://data.vlaanderen.be/id/straatnaam/65412" },
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "rechts", VanPositie = 4, Identificator = "niet van toepassing" }
            ],
            Wegbeheerder =
            [
                new IngeschetstWegsegmentWegbeheerderAttribuutWaarde { Kant = "links", Wegbeheerder = "AGIV" },
                new IngeschetstWegsegmentWegbeheerderAttribuutWaarde { Kant = "rechts", Wegbeheerder = "AWV" }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorCode)));
    }

    [Fact]
    public async Task WhenOneSideIsOnlyPartlyCovered_ThenTheCoverageErrorIsReported()
    {
        var parameters = CreateOutlineV2Parameters.Valid() with
        {
            Straatnaam =
            [
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "links", Identificator = "https://data.vlaanderen.be/id/straatnaam/71671" },
                new IngeschetstWegsegmentStraatnaamAttribuutWaarde { Kant = "rechts", VanPositie = 0, TotPositie = 4, Identificator = "https://data.vlaanderen.be/id/straatnaam/65412" }
            ]
        };

        var result = await _validator.ValidateAsync(parameters);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ProblemCode.RoadSegment.StreetName.DynamicAttributeProblemCodes.ToPositionNotEqualToLength.ToString(), error.ErrorCode);
    }

    private async Task<List<FluentValidation.Results.ValidationFailure>> ValidateAndTranslate(CreateOutlinedRoadSegmentV2Parameters parameters)
    {
        var result = await _validator.ValidateAsync(parameters);
        return result.Errors.TranslateToDutch(WellKnownProblemTranslators.Default).ToList();
    }
}
