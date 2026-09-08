namespace RoadRegistry.BackOffice.Api.Tests.V2.GradeSeparatedJunctions.WhenChangeToGradeJunctionV2;

using AutoFixture;
using BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.GradeSeparatedJunctions;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Xunit;

public class ChangeToGradeJunctionV2Tests : V2ReadEndpointTestBase
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly GradeSeparatedJunctionsController _controller;
    private ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest? _capturedSqsRequest;

    public ChangeToGradeJunctionV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest)r)
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

    private Task<IActionResult> Act(int id)
    {
        return _controller.ChangeGradeSeparatedJunctionToGradeJunctionV2(id, Store, CancellationToken.None);
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenTheRequestIsValid_ThenItIsQueued()
    {
        var gradeSeparatedJunction = SeedGradeSeparatedJunction();

        var result = await Act(gradeSeparatedJunction.GradeSeparatedJunctionId.ToInt32());

        result.Should().BeOfType<AcceptedResult>();
        _capturedSqsRequest.Should().NotBeNull();
        _capturedSqsRequest!.GradeSeparatedJunctionId.Should().Be(gradeSeparatedJunction.GradeSeparatedJunctionId);
    }

    // VAL-2
    [Fact]
    public async Task WhenTheGradeSeparatedJunctionDoesNotExist_ThenNotFound()
    {
        var result = await Act(Fixture.Create<int>());

        result.Should().BeOfType<NotFoundResult>();
        VerifyNothingWasQueued();
    }

    // VAL-3
    [Fact]
    public async Task WhenTheGradeSeparatedJunctionIsRemoved_ThenGone()
    {
        var gradeSeparatedJunction = SeedGradeSeparatedJunction(isRemoved: true);

        var result = await Act(gradeSeparatedJunction.GradeSeparatedJunctionId.ToInt32());

        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
        VerifyNothingWasQueued();
    }
}
