namespace RoadRegistry.BackOffice.Api.Tests.V2.GradeSeparatedJunctions.WhenChangeGradeSeparatedJunctionAttributesV2;

using System;
using AutoFixture;
using BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.GradeSeparatedJunctions;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Xunit;

public class ChangeGradeSeparatedJunctionAttributesV2Tests : V2ReadEndpointTestBase
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly GradeSeparatedJunctionsController _controller;
    private ChangeGradeSeparatedJunctionAttributesV2SqsRequest? _capturedSqsRequest;

    public ChangeGradeSeparatedJunctionAttributesV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeGradeSeparatedJunctionAttributesV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (ChangeGradeSeparatedJunctionAttributesV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new GradeSeparatedJunctionsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private GradeSeparatedJunctionReadItem SeedGradeSeparatedJunction(bool isRemoved = false)
    {
        var wasAdded = Fixture.Create<GradeSeparatedJunctionWasAdded>();
        var readItem = new GradeSeparatedJunctionReadItem
        {
            GradeSeparatedJunctionId = wasAdded.GradeSeparatedJunctionId,
            LowerRoadSegmentId = wasAdded.LowerRoadSegmentId,
            UpperRoadSegmentId = wasAdded.UpperRoadSegmentId,
            Type = wasAdded.Type.ToString(),
            Origin = wasAdded.Provenance.ToEventTimestamp(),
            LastModified = wasAdded.Provenance.ToEventTimestamp(),
            IsV2 = true,
            IsRemoved = isRemoved
        };
        Seed(readItem);
        return readItem;
    }

    private static ChangeGradeSeparatedJunctionAttributesV2Parameters Parameters(
        string? lower = "1",
        string? upper = "2",
        string? type = "brug")
    {
        return new ChangeGradeSeparatedJunctionAttributesV2Parameters
        {
            OnderliggendWegsegment = lower,
            BovenliggendWegsegment = upper,
            OngelijkgrondseKruisingType = type
        };
    }

    private Task<IActionResult> Act(int id, ChangeGradeSeparatedJunctionAttributesV2Parameters parameters)
    {
        return _controller.ChangeGradeSeparatedJunctionAttributesV2(id, parameters, Store, CancellationToken.None);
    }

    private static async Task<ValidationFailure> ActAndExpectSingleFailure(Func<Task<IActionResult>> act)
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(act);
        return ex.Errors.Should().ContainSingle().Which;
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<ChangeGradeSeparatedJunctionAttributesV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenTheRequestIsValid_ThenItIsQueued()
    {
        var gradeSeparatedJunction = SeedGradeSeparatedJunction();

        var result = await Act(gradeSeparatedJunction.GradeSeparatedJunctionId.ToInt32(), Parameters(lower: "432756", upper: "987567", type: "tunnel"));

        result.Should().BeOfType<AcceptedResult>();

        _capturedSqsRequest.Should().NotBeNull();
        _capturedSqsRequest!.GradeSeparatedJunctionId.Should().Be(gradeSeparatedJunction.GradeSeparatedJunctionId);
        _capturedSqsRequest.LowerRoadSegmentId.Should().Be(new RoadSegmentId(432756));
        _capturedSqsRequest.UpperRoadSegmentId.Should().Be(new RoadSegmentId(987567));
        _capturedSqsRequest.Type.Should().Be(GradeSeparatedJunctionTypeV2.Tunnel);
    }

    // VAL-2
    [Fact]
    public async Task WhenTheGradeSeparatedJunctionDoesNotExist_ThenNotFound()
    {
        var result = await Act(Fixture.Create<int>(), Parameters());

        result.Should().BeOfType<NotFoundResult>();
        VerifyNothingWasQueued();
    }

    // VAL-3
    [Fact]
    public async Task WhenTheGradeSeparatedJunctionIsRemoved_ThenGone()
    {
        var gradeSeparatedJunction = SeedGradeSeparatedJunction(isRemoved: true);

        var result = await Act(gradeSeparatedJunction.GradeSeparatedJunctionId.ToInt32(), Parameters());

        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
        VerifyNothingWasQueued();
    }

    // VAL-4, VAL-6, VAL-9
    [Theory]
    [InlineData(null, "2", "brug", "De parameter 'onderliggendWegsegment' is verplicht.")]
    [InlineData("1", null, "brug", "De parameter 'bovenliggendWegsegment' is verplicht.")]
    [InlineData("1", "2", null, "De parameter 'ongelijkgrondseKruisingType' is verplicht.")]
    public async Task WhenAParameterIsMissing_ThenItIsRequired(string? lower, string? upper, string? type, string expectedMessage)
    {
        var gradeSeparatedJunction = SeedGradeSeparatedJunction();

        var failure = await ActAndExpectSingleFailure(() => Act(gradeSeparatedJunction.GradeSeparatedJunctionId.ToInt32(), Parameters(lower, upper, type)));

        failure.ErrorMessage.Should().Be(expectedMessage);
        VerifyNothingWasQueued();
    }

    // VAL-10: 'nietGekend' exists for what was imported without a known kind and is not something a caller may ask for.
    [Theory]
    [InlineData("viaduct")]
    [InlineData("nietGekend")]
    public async Task WhenTheTypeIsNotAllowed_ThenItHasAnInvalidValue(string type)
    {
        var gradeSeparatedJunction = SeedGradeSeparatedJunction();

        var failure = await ActAndExpectSingleFailure(() => Act(gradeSeparatedJunction.GradeSeparatedJunctionId.ToInt32(), Parameters(type: type)));

        failure.ErrorMessage.Should().Be("De parameter 'ongelijkgrondseKruisingType' heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task WhenARoadSegmentIsNotAnIdentifier_ThenItHasAnInvalidValue()
    {
        var gradeSeparatedJunction = SeedGradeSeparatedJunction();

        var failure = await ActAndExpectSingleFailure(() => Act(gradeSeparatedJunction.GradeSeparatedJunctionId.ToInt32(), Parameters(lower: "abc")));

        failure.ErrorMessage.Should().Be("De parameter 'onderliggendWegsegment' heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }
}
