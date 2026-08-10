namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenChangeRoadSegmentAttributesV2;

using System;
using System.Linq;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadSegments.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;

public class ChangeRoadSegmentAttributesV2Tests : V2ReadEndpointTestBase
{
    private const int MaximumRoadSegmentCount = 1000;

    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadSegmentsController _controller;

    public ChangeRoadSegmentAttributesV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentAttributesV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadSegmentsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private Task<IActionResult> Act(ChangeRoadSegmentAttributesV2Parameters parameters)
    {
        return _controller.ChangeRoadSegmentAttributesV2(parameters, CancellationToken.None);
    }

    private static ChangeRoadSegmentAttributeV2Parameters ChangeMorphologyOf(params int[] roadSegmentIds)
    {
        return new ChangeRoadSegmentAttributeV2Parameters
        {
            Wegsegmenten = roadSegmentIds,
            Morfologie =
            [
                new MorfologieParameters { Morfologie = RoadSegmentMorphologyV2.WegBestaandeUit1Rijbaan.ToDutchString() }
            ]
        };
    }

    private static int[] RoadSegmentIds(int count, int start = 1)
    {
        return Enumerable.Range(start, count).ToArray();
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<ChangeRoadSegmentAttributesV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private ChangeRoadSegmentAttributesV2SqsRequest _capturedSqsRequest;

    private void CaptureSqsRequest()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentAttributesV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (ChangeRoadSegmentAttributesV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());
    }

    private static async Task<ValidationFailure> ActAndExpectSingleFailure(Func<Task<IActionResult>> act)
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(act);
        return ex.Errors.Should().ContainSingle().Which;
    }

    private static ChangeRoadSegmentAttributesV2Parameters WithStreetName(params StraatnaamParameters[] straatnaam)
    {
        return [new ChangeRoadSegmentAttributeV2Parameters { Wegsegmenten = [1], Straatnaam = straatnaam }];
    }

    private static ChangeRoadSegmentAttributesV2Parameters WithMaintenanceAuthority(params WegbeheerderParameters[] wegbeheerder)
    {
        return [new ChangeRoadSegmentAttributeV2Parameters { Wegsegmenten = [1], Wegbeheerder = wegbeheerder }];
    }

    [Fact]
    public async Task GivenAnEmptyBody_ThenValidationException()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act([]));

        ex.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Ongeldige JSON.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenNoRoadSegments_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            new()
            {
                Wegsegmenten = null,
                Morfologie = [new MorfologieParameters { Morfologie = RoadSegmentMorphologyV2.WegBestaandeUit1Rijbaan.ToDutchString() }]
            }
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[0].wegsegmenten");
        failure.ErrorMessage.Should().Be("De parameter 'wegsegmenten' is verplicht.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnEmptyArrayOfRoadSegments_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            ChangeMorphologyOf()
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[0].wegsegmenten");
        failure.ErrorMessage.Should().Be("De parameter 'wegsegmenten' is verplicht.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenNoAttributeAtAll_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            new() { Wegsegmenten = [1] }
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[0]");
        failure.ErrorMessage.Should().Be("Minstens één attribuut is verplicht.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenTheSameAttributeTwiceForTheSameRoadSegment_ThenValidationException()
    {
        // The attribute is replaced as a whole, so two changes to the same attribute of the same segment in one
        // request would silently pick a winner.
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            ChangeMorphologyOf(1, 2),
            ChangeMorphologyOf(2, 3)
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[1]");
        failure.ErrorMessage.Should().Be("De parameter 'morfologie' werd meermaals meegegeven voor wegsegment 2.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenTwoDifferentAttributesForTheSameRoadSegment_ThenAccepted()
    {
        // Only the same attribute twice is a conflict; two different attributes for one segment are not.
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            ChangeMorphologyOf(1),
            new()
            {
                Wegsegmenten = [1],
                Wegverharding = [new WegverhardingParameters { Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString() }]
            }
        };

        var result = await Act(parameters);

        result.Should().BeOfType<AcceptedResult>();
    }

    [Fact]
    public async Task GivenSeveralInvalidChanges_ThenEveryFailureIsReported()
    {
        // Shape validation collects: the caller gets everything that is wrong with the request in one go.
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            new() { Wegsegmenten = [1] },
            new()
            {
                Wegsegmenten = [2],
                Morfologie = [new MorfologieParameters { Morfologie = "geen morfologie" }]
            },
            new()
            {
                Wegsegmenten = [3],
                Wegverharding = [new WegverhardingParameters { VanPositie = -1, Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString() }]
            }
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        ex.Errors.Select(x => x.PropertyName).Should().BeEquivalentTo(["[0]", "[1].morfologie[0]", "[2].wegverharding[0].vanPositie"]);
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenNoSide_ThenValidationException()
    {
        var parameters = WithStreetName(new StraatnaamParameters { Identificator = "79632" });

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].straatnaam[0]");
        failure.ErrorMessage.Should().Be("'kant' is verplicht binnen elk object in de array '[0].straatnaam[0]'.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnUnknownSide_ThenValidationException()
    {
        var parameters = WithStreetName(new StraatnaamParameters { Kant = "bovenkant", Identificator = "79632" });

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].straatnaam[0]");
        failure.ErrorMessage.Should().Be("De parameter 'kant' heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenNoStreetNameIdentifier_ThenValidationException()
    {
        var parameters = WithStreetName(new StraatnaamParameters { Kant = RoadSegmentAttributeSide.Beide.ToDutchString() });

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].straatnaam[0]");
        failure.ErrorMessage.Should().Be("'identificator' is verplicht binnen elk object in de array '[0].straatnaam'.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnUnparsableStreetNameIdentifier_ThenValidationException()
    {
        var parameters = WithStreetName(new StraatnaamParameters { Kant = RoadSegmentAttributeSide.Beide.ToDutchString(), Identificator = "straatnaam" });

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].straatnaam[0]");
        failure.ErrorMessage.Should().Be("De straatnaamidentificator straatnaam is ongeldig.");
        VerifyNothingWasQueued();
    }

    [Theory]
    // A bare number, the full PURI the registry publishes, and the PURI with a trailing slash all name the same street.
    [InlineData("79632")]
    [InlineData("https://data.vlaanderen.be/id/straatnaam/79632")]
    [InlineData("https://data.vlaanderen.be/id/straatnaam/79632/")]
    public async Task GivenAStreetNameIdentifierInAnyAcceptedShape_ThenTheLocalIdIsSentAlong(string identificator)
    {
        var parameters = WithStreetName(new StraatnaamParameters { Kant = RoadSegmentAttributeSide.Links.ToDutchString(), Identificator = identificator });
        CaptureSqsRequest();

        await Act(parameters);

        var streetName = _capturedSqsRequest.Groups.Should().ContainSingle().Which.StreetName.Should().ContainSingle().Which;
        streetName.Value.Should().Be(new StreetNameLocalId(79632));
        streetName.Side.Should().Be(RoadSegmentAttributeSide.Links);
    }

    [Fact]
    public async Task GivenAStreetNameThatIsNotApplicable_ThenTheNotApplicableLocalIdIsSentAlong()
    {
        var parameters = WithStreetName(new StraatnaamParameters { Kant = RoadSegmentAttributeSide.Beide.ToDutchString(), Identificator = "Niet van toepassing" });
        CaptureSqsRequest();

        await Act(parameters);

        _capturedSqsRequest.Groups.Should().ContainSingle()
            .Which.StreetName.Should().ContainSingle()
            .Which.Value.Should().Be(StreetNameLocalId.NotApplicable);
    }

    [Fact]
    public async Task GivenNoMaintenanceAuthority_ThenValidationException()
    {
        var parameters = WithMaintenanceAuthority(new WegbeheerderParameters { Kant = RoadSegmentAttributeSide.Beide.ToDutchString() });

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].wegbeheerder[0]");
        failure.ErrorMessage.Should().Be("'wegbeheerder' is verplicht binnen elk object in de array '[0].wegbeheerder'.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnUnusableMaintenanceAuthorityCode_ThenValidationException()
    {
        var parameters = WithMaintenanceAuthority(new WegbeheerderParameters { Kant = RoadSegmentAttributeSide.Beide.ToDutchString(), Wegbeheerder = "AWV 114" });

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].wegbeheerder[0]");
        failure.ErrorMessage.Should().Be("De wegbeheerdercode AWV 114 is ongeldig.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAMaintenanceAuthorityPerSide_ThenBothAreSentAlong()
    {
        // Whether the organization exists is not decided here - only the shape is - so an unknown but well-formed code
        // passes the endpoint and is settled against the organization cache further down the line.
        var parameters = WithMaintenanceAuthority(
            new WegbeheerderParameters { Kant = RoadSegmentAttributeSide.Links.ToDutchString(), Wegbeheerder = "AWV114" },
            new WegbeheerderParameters { Kant = RoadSegmentAttributeSide.Rechts.ToDutchString(), Wegbeheerder = "AWV116" });
        CaptureSqsRequest();

        await Act(parameters);

        _capturedSqsRequest.Groups.Should().ContainSingle()
            .Which.MaintenanceAuthority.Should().BeEquivalentTo(new[]
            {
                new { Side = RoadSegmentAttributeSide.Links, Value = new OrganizationId("AWV114") },
                new { Side = RoadSegmentAttributeSide.Rechts, Value = new OrganizationId("AWV116") }
            });
    }

    [Fact]
    public async Task GivenNoDirection_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            new() { Wegsegmenten = [1], VerkeerstypeAuto = [new VerkeerstypeParameters()] }
        };

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].verkeerstypeAuto[0]");
        failure.ErrorMessage.Should().Be("'richting' is verplicht binnen elk object in de array '[0].verkeerstypeAuto'.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnUnknownDirection_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            new() { Wegsegmenten = [1], VerkeerstypeVoetganger = [new VerkeerstypeVoetgangerParameters { Richting = "achteruit en vooruit" }] }
        };

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].verkeerstypeVoetganger[0]");
        failure.ErrorMessage.Should().Be("De parameter 'richting' heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenNoAttributeValue_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            new() { Wegsegmenten = [1], Toegang = [new ToegangParameters()] }
        };

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].toegang[0]");
        failure.ErrorMessage.Should().Be("'toegang' is verplicht binnen elk object in de array '[0].toegang'.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnUnknownAttributeValue_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            new() { Wegsegmenten = [1], Wegcategorie = [new WegcategorieParameters { Wegcategorie = "geen wegcategorie" }] }
        };

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].wegcategorie[0]");
        failure.ErrorMessage.Should().Be("De parameter 'wegcategorie' heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenANegativeToPosition_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            new()
            {
                Wegsegmenten = [1],
                Wegverharding = [new WegverhardingParameters { TotPositie = -5, Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString() }]
            }
        };

        var failure = await ActAndExpectSingleFailure(() => Act(parameters));

        failure.PropertyName.Should().Be("[0].wegverharding[0].totPositie");
        failure.ErrorMessage.Should().Be("De totPositie -5 heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnAttributeWithoutPositions_ThenItIsSentAlongUnbounded()
    {
        // Omitted positions mean "the whole segment", which the endpoint leaves as null: the length a null resolves
        // against is the segment's own, which only the domain knows.
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            ChangeMorphologyOf(1)
        };
        CaptureSqsRequest();

        await Act(parameters);

        var group = _capturedSqsRequest.Groups.Should().ContainSingle().Which;
        group.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1)]);
        var morphology = group.Morphology.Should().ContainSingle().Which;
        morphology.FromPosition.Should().BeNull();
        morphology.ToPosition.Should().BeNull();
        morphology.Value.Should().Be(RoadSegmentMorphologyV2.WegBestaandeUit1Rijbaan);
    }

    [Fact]
    public async Task GivenTheMaximumAmountOfRoadSegments_ThenAccepted()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            ChangeMorphologyOf(RoadSegmentIds(MaximumRoadSegmentCount))
        };

        var result = await Act(parameters);

        result.Should().BeOfType<AcceptedResult>();
    }

    [Fact]
    public async Task GivenMoreThanTheMaximumAmountOfRoadSegments_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            ChangeMorphologyOf(RoadSegmentIds(MaximumRoadSegmentCount + 1))
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        ex.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be($"Er kunnen maximaal {MaximumRoadSegmentCount} wegsegmenten gewijzigd worden. Er werden er {MaximumRoadSegmentCount + 1} opgegeven.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenMoreThanTheMaximumAmountOfRoadSegmentsSpreadOverMultipleGroups_ThenValidationException()
    {
        // The limit is on the request as a whole, not on a single group.
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            ChangeMorphologyOf(RoadSegmentIds(600)),
            ChangeMorphologyOf(RoadSegmentIds(600, start: 601))
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        ex.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be($"Er kunnen maximaal {MaximumRoadSegmentCount} wegsegmenten gewijzigd worden. Er werden er 1200 opgegeven.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenMoreThanTheMaximumAmountOfRoadSegmentsButNotUnique_ThenAccepted()
    {
        // Only the unique road segments count: the same segment listed in two groups is one segment to change.
        var parameters = new ChangeRoadSegmentAttributesV2Parameters
        {
            ChangeMorphologyOf(RoadSegmentIds(MaximumRoadSegmentCount)),
            new ChangeRoadSegmentAttributeV2Parameters
            {
                Wegsegmenten = RoadSegmentIds(MaximumRoadSegmentCount),
                Wegverharding =
                [
                    new WegverhardingParameters { Wegverharding = RoadSegmentSurfaceTypeV2.Verhard.ToDutchString() }
                ]
            }
        };

        var result = await Act(parameters);

        result.Should().BeOfType<AcceptedResult>();
    }
}
