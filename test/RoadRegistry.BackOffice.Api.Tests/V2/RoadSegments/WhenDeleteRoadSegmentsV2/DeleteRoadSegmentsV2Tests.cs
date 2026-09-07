namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenDeleteRoadSegmentsV2;

using System.Linq;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadNetwork;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.RoadSegment.ValueObjects;

public class DeleteRoadSegmentsV2Tests : V2ReadEndpointTestBase
{
    private const int MaximumRoadSegmentCount = 1000;

    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadSegmentsController _controller;

    public DeleteRoadSegmentsV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<RemoveRoadSegmentsSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadSegmentsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private Task<IActionResult> Act(DeleteRoadSegmentsV2Parameters parameters)
    {
        return _controller.DeleteRoadSegmentsV2(parameters, new DeleteRoadSegmentsV2ParametersValidator(), CancellationToken.None);
    }

    [Fact]
    public async Task WhenRoadSegmentsAreGiven_ThenTheyAreQueuedForRemoval()
    {
        var result = await Act(new DeleteRoadSegmentsV2Parameters { Wegsegmenten = [1, 2, 3] });

        result.Should().BeOfType<AcceptedResult>();

        _mediator.Verify(x => x.Send(
            It.Is<RemoveRoadSegmentsSqsRequest>(request => request.RoadSegmentIds.Select(id => id.ToInt32()).SequenceEqual(new[] { 1, 2, 3 })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Naming the same segment twice asks for one removal, not two.
    [Fact]
    public async Task WhenTheSameRoadSegmentIsGivenTwice_ThenItIsQueuedOnce()
    {
        await Act(new DeleteRoadSegmentsV2Parameters { Wegsegmenten = [1, 1, 2] });

        _mediator.Verify(x => x.Send(
            It.Is<RemoveRoadSegmentsSqsRequest>(request => request.RoadSegmentIds.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // VAL-1
    [Theory]
    [InlineData(new int[0])]
    [InlineData(null)]
    public async Task WhenNoRoadSegmentIsGiven_ThenValidationError(int[]? wegsegmenten)
    {
        var act = () => Act(new DeleteRoadSegmentsV2Parameters { Wegsegmenten = wegsegmenten! });

        await act.Should().ThrowAsync<ValidationException>();
    }

    // VAL-1: an identifier that is not one at all.
    [Fact]
    public async Task WhenARoadSegmentIdentifierIsNotValid_ThenValidationError()
    {
        var act = () => Act(new DeleteRoadSegmentsV2Parameters { Wegsegmenten = [0] });

        await act.Should().ThrowAsync<ValidationException>();
    }

    // VAL-3
    [Fact]
    public async Task WhenMoreThanTheMaximumAreGiven_ThenValidationError()
    {
        var wegsegmenten = Enumerable.Range(1, MaximumRoadSegmentCount + 1).ToArray();

        var act = () => Act(new DeleteRoadSegmentsV2Parameters { Wegsegmenten = wegsegmenten });

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(x => x.ErrorMessage.Contains($"({MaximumRoadSegmentCount}) werd overschreden"));
    }

    // The ceiling counts what is actually removed, so a request naming the same segment a thousand times over is fine.
    [Fact]
    public async Task WhenTheMaximumIsOnlyExceededByDuplicates_ThenItIsAccepted()
    {
        var wegsegmenten = Enumerable.Range(1, MaximumRoadSegmentCount).Concat(Enumerable.Range(1, 10)).ToArray();

        var result = await Act(new DeleteRoadSegmentsV2Parameters { Wegsegmenten = wegsegmenten });

        result.Should().BeOfType<AcceptedResult>();
    }
}
