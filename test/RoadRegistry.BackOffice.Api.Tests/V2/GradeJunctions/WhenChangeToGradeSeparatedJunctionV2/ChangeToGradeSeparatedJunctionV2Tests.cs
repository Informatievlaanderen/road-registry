namespace RoadRegistry.BackOffice.Api.Tests.V2.GradeJunctions.WhenChangeToGradeSeparatedJunctionV2;

using System;
using AutoFixture;
using BackOffice.Handlers.Sqs.GradeJunctions.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.GradeJunctions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Xunit;

public class ChangeToGradeSeparatedJunctionV2Tests : V2ReadEndpointTestBase
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly GradeJunctionsController _controller;
    private ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest? _capturedSqsRequest;

    public ChangeToGradeSeparatedJunctionV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new GradeJunctionsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private GradeJunctionReadItem SeedGradeJunction(bool isRemoved = false)
    {
        var gradeJunctionWasAdded = Fixture.Create<GradeJunctionWasAdded>();
        var readItem = new GradeJunctionReadItem
        {
            GradeJunctionId = gradeJunctionWasAdded.GradeJunctionId,
            RoadSegmentId1 = gradeJunctionWasAdded.RoadSegmentId1,
            RoadSegmentId2 = gradeJunctionWasAdded.RoadSegmentId2,
            Origin = gradeJunctionWasAdded.Provenance.ToEventTimestamp(),
            LastModified = gradeJunctionWasAdded.Provenance.ToEventTimestamp(),
            IsV2 = true,
            IsRemoved = isRemoved
        };
        Seed(readItem);
        return readItem;
    }

    private static ChangeToGradeSeparatedJunctionV2Parameters Parameters(
        string? lower = "1",
        string? upper = "2",
        string? type = "brug")
    {
        return new ChangeToGradeSeparatedJunctionV2Parameters
        {
            OnderliggendWegsegment = lower,
            BovenliggendWegsegment = upper,
            OngelijkgrondseKruisingType = type
        };
    }

    private Task<IActionResult> Act(int id, ChangeToGradeSeparatedJunctionV2Parameters parameters)
    {
        return _controller.ChangeGradeJunctionToGradeSeparatedJunctionV2(id, parameters, Store, CancellationToken.None);
    }

    private static async Task<ValidationFailure> ActAndExpectSingleFailure(Func<Task<IActionResult>> act)
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(act);
        return ex.Errors.Should().ContainSingle().Which;
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenTheRequestIsValid_ThenItIsQueued()
    {
        var gradeJunction = SeedGradeJunction();

        var result = await Act(gradeJunction.GradeJunctionId.ToInt32(), Parameters(lower: "432756", upper: "987567", type: "tunnel"));

        result.Should().BeOfType<AcceptedResult>();

        _capturedSqsRequest.Should().NotBeNull();
        _capturedSqsRequest!.GradeJunctionId.Should().Be(gradeJunction.GradeJunctionId);
        _capturedSqsRequest.LowerRoadSegmentId.Should().Be(new RoadSegmentId(432756));
        _capturedSqsRequest.UpperRoadSegmentId.Should().Be(new RoadSegmentId(987567));
        _capturedSqsRequest.Type.Should().Be(GradeSeparatedJunctionTypeV2.Tunnel);
    }

    // VAL-2
    [Fact]
    public async Task WhenTheGradeJunctionDoesNotExist_ThenNotFound()
    {
        var result = await Act(Fixture.Create<int>(), Parameters());

        result.Should().BeOfType<NotFoundResult>();
        VerifyNothingWasQueued();
    }

    // VAL-3
    [Fact]
    public async Task WhenTheGradeJunctionIsRemoved_ThenGone()
    {
        var gradeJunction = SeedGradeJunction(isRemoved: true);

        var result = await Act(gradeJunction.GradeJunctionId.ToInt32(), Parameters());

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
        var gradeJunction = SeedGradeJunction();

        var failure = await ActAndExpectSingleFailure(() => Act(gradeJunction.GradeJunctionId.ToInt32(), Parameters(lower, upper, type)));

        failure.ErrorMessage.Should().Be(expectedMessage);
        VerifyNothingWasQueued();
    }

    // VAL-10: 'nietGekend' exists for what was imported without a known kind and is not something a caller may ask for.
    [Theory]
    [InlineData("viaduct")]
    [InlineData("nietGekend")]
    public async Task WhenTheTypeIsNotAllowed_ThenItHasAnInvalidValue(string type)
    {
        var gradeJunction = SeedGradeJunction();

        var failure = await ActAndExpectSingleFailure(() => Act(gradeJunction.GradeJunctionId.ToInt32(), Parameters(type: type)));

        failure.ErrorMessage.Should().Be("De parameter 'ongelijkgrondseKruisingType' heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task WhenARoadSegmentIsNotAnIdentifier_ThenItHasAnInvalidValue()
    {
        var gradeJunction = SeedGradeJunction();

        var failure = await ActAndExpectSingleFailure(() => Act(gradeJunction.GradeJunctionId.ToInt32(), Parameters(lower: "abc")));

        failure.ErrorMessage.Should().Be("De parameter 'onderliggendWegsegment' heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }
}
