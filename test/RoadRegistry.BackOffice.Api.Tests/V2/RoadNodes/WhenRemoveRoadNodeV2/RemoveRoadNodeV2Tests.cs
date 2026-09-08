namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadNodes.WhenRemoveRoadNodeV2;

using AutoFixture;
using BackOffice.Handlers.Sqs.RoadNodes.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.RoadNodes;
using RoadRegistry.Extensions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.ValueObjects;
using Xunit;

public class RemoveRoadNodeV2Tests : V2ReadEndpointTestBase
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadNodesController _controller;
    private RemoveRoadNodeV2SqsRequest? _capturedSqsRequest;

    public RemoveRoadNodeV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<RemoveRoadNodeV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (RemoveRoadNodeV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadNodesController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private RoadNodeReadItem SeedRoadNode(bool isRemoved = false)
    {
        var roadNodeWasAdded = Fixture.Create<RoadNodeWasAdded>();
        var readItem = new RoadNodeReadItem
        {
            RoadNodeId = roadNodeWasAdded.RoadNodeId,
            Type = RoadNodeTypeV2.EchteKnoop.ToString(),
            Grensknoop = roadNodeWasAdded.Grensknoop,
            Geometry = new RoadNodeGeometryProjections
            {
                Lambert72 = roadNodeWasAdded.Geometry.EnsureLambert72(),
                Lambert08 = roadNodeWasAdded.Geometry.EnsureLambert08()
            },
            RoadSegmentIds = [],
            Origin = roadNodeWasAdded.Provenance.ToEventTimestamp(),
            LastModified = roadNodeWasAdded.Provenance.ToEventTimestamp(),
            IsV2 = true,
            IsRemoved = isRemoved
        };
        Seed(readItem);
        return readItem;
    }

    private Task<IActionResult> Act(int id)
    {
        return _controller.RemoveRoadNodeV2(id, Store, CancellationToken.None);
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<RemoveRoadNodeV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenTheRequestIsValid_ThenItIsQueued()
    {
        var roadNode = SeedRoadNode();

        var result = await Act(roadNode.RoadNodeId.ToInt32());

        result.Should().BeOfType<AcceptedResult>();
        _capturedSqsRequest.Should().NotBeNull();
        _capturedSqsRequest!.RoadNodeId.Should().Be(roadNode.RoadNodeId);
    }

    // VAL-2
    [Fact]
    public async Task WhenTheRoadNodeDoesNotExist_ThenNotFound()
    {
        var result = await Act(Fixture.Create<int>());

        result.Should().BeOfType<NotFoundResult>();
        VerifyNothingWasQueued();
    }

    // VAL-3
    [Fact]
    public async Task WhenTheRoadNodeIsRemoved_ThenGone()
    {
        var roadNode = SeedRoadNode(isRemoved: true);

        var result = await Act(roadNode.RoadNodeId.ToInt32());

        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
        VerifyNothingWasQueued();
    }
}
