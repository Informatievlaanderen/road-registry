namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadNodes.WhenGetRoadNode;

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.V2.RoadNodes;
using RoadRegistry.Extensions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.ValueObjects;
using Xunit;

public class GetRoadNodeTests : V2ReadEndpointTestBase
{
    private readonly RoadNodesController _controller;

    public GetRoadNodeTests()
    {
        _controller = new RoadNodesController(CreateControllerContext());
        SetHttpContext(_controller);
    }

    [Fact]
    public async Task GivenExistingRoadNode_ThenOk()
    {
        var roadNodeWasAdded = Fixture.Create<RoadNodeWasAdded>();
        Seed(BuildReadItem(roadNodeWasAdded));

        var result = await _controller.GetRoadNodeV2((int)roadNodeWasAdded.RoadNodeId, ApiOptions, Store);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var detail = okResult.Value.Should().BeOfType<WegknoopV2Detail>().Subject;
        detail.Identificator.ObjectId.Should().Be(roadNodeWasAdded.RoadNodeId.ToString());
        detail.WegknoopType.Should().Be(RoadNodeTypeV2.EchteKnoop.ToDutchString());
    }

    [Fact]
    public async Task GivenUnknownRoadNode_ThenNotFound()
    {
        var result = await _controller.GetRoadNodeV2(Fixture.Create<int>(), ApiOptions, Store);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GivenRemovedRoadNode_ThenGone()
    {
        var roadNodeWasAdded = Fixture.Create<RoadNodeWasAdded>();
        var readItem = BuildReadItem(roadNodeWasAdded);
        readItem.IsRemoved = true;
        Seed(readItem);

        var result = await _controller.GetRoadNodeV2((int)roadNodeWasAdded.RoadNodeId, ApiOptions, Store);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
    }

    private static RoadNodeReadItem BuildReadItem(RoadNodeWasAdded e)
    {
        return new RoadNodeReadItem
        {
            RoadNodeId = e.RoadNodeId,
            Geometry = new RoadNodeGeometryProjections
            {
                Lambert72 = e.Geometry.EnsureLambert72(),
                Lambert08 = e.Geometry.EnsureLambert08()
            },
            Type = RoadNodeTypeV2.EchteKnoop.ToString(),
            Grensknoop = e.Grensknoop,
            RoadSegmentIds = [],
            Origin = e.Provenance.ToEventTimestamp(),
            LastModified = e.Provenance.ToEventTimestamp(),
            IsV2 = true
        };
    }
}
