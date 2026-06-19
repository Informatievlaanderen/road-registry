namespace RoadRegistry.BackOffice.Api.Tests.V2.GradeSeparatedJunctions.WhenGetGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.V2.GradeSeparatedJunctions;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Xunit;

public class GetGradeSeparatedJunctionTests : V2ReadEndpointTestBase
{
    private readonly GradeSeparatedJunctionsController _controller;

    public GetGradeSeparatedJunctionTests()
    {
        _controller = new GradeSeparatedJunctionsController(CreateControllerContext());
        SetHttpContext(_controller);
    }

    [Fact]
    public async Task GivenExistingGradeSeparatedJunction_ThenOk()
    {
        var gradeSeparatedJunctionWasAdded = Fixture.Create<GradeSeparatedJunctionWasAdded>();
        Seed(BuildReadItem(gradeSeparatedJunctionWasAdded));

        var result = await _controller.GetGradeSeparatedJunctionV2((int)gradeSeparatedJunctionWasAdded.GradeSeparatedJunctionId, ApiOptions, Store);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var detail = okResult.Value.Should().BeOfType<OngelijkgrondseKruisingV2Detail>().Subject;
        detail.Identificator.ObjectId.Should().Be(gradeSeparatedJunctionWasAdded.GradeSeparatedJunctionId.ToString());
        detail.OngelijkgrondseKruisingType.Should().Be(gradeSeparatedJunctionWasAdded.Type.ToDutchString());
    }

    [Fact]
    public async Task GivenUnknownGradeSeparatedJunction_ThenNotFound()
    {
        var result = await _controller.GetGradeSeparatedJunctionV2(Fixture.Create<int>(), ApiOptions, Store);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GivenRemovedGradeSeparatedJunction_ThenGone()
    {
        var gradeSeparatedJunctionWasAdded = Fixture.Create<GradeSeparatedJunctionWasAdded>();
        var readItem = BuildReadItem(gradeSeparatedJunctionWasAdded);
        readItem.IsRemoved = true;
        Seed(readItem);

        var result = await _controller.GetGradeSeparatedJunctionV2((int)gradeSeparatedJunctionWasAdded.GradeSeparatedJunctionId, ApiOptions, Store);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
    }

    private static GradeSeparatedJunctionReadItem BuildReadItem(GradeSeparatedJunctionWasAdded e)
    {
        return new GradeSeparatedJunctionReadItem
        {
            GradeSeparatedJunctionId = e.GradeSeparatedJunctionId,
            LowerRoadSegmentId = e.LowerRoadSegmentId,
            UpperRoadSegmentId = e.UpperRoadSegmentId,
            Type = e.Type.ToString(),
            Origin = e.Provenance.ToEventTimestamp(),
            LastModified = e.Provenance.ToEventTimestamp(),
            IsV2 = true
        };
    }
}
