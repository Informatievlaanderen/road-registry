namespace RoadRegistry.BackOffice.Api.Tests.V2.GradeJunctions.WhenGetGradeJunction;

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.V2.GradeJunctions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.ValueObjects;
using Xunit;

public class GetGradeJunctionTests : V2ReadEndpointTestBase
{
    private readonly GradeJunctionsController _controller;

    public GetGradeJunctionTests()
    {
        _controller = new GradeJunctionsController(CreateControllerContext());
        SetHttpContext(_controller);
    }

    [Fact]
    public async Task GivenExistingGradeJunction_ThenOk()
    {
        var gradeJunctionWasAdded = Fixture.Create<GradeJunctionWasAdded>();
        Seed(BuildReadItem(gradeJunctionWasAdded));

        var result = await _controller.GetGradeJunctionV2((int)gradeJunctionWasAdded.GradeJunctionId, ApiOptions, Store);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var detail = okResult.Value.Should().BeOfType<GelijkgrondseKruisingV2Detail>().Subject;
        detail.Identificator.ObjectId.Should().Be(gradeJunctionWasAdded.GradeJunctionId.ToString());
        detail.KruisendeWegsegmenten.Should().HaveCount(2);
    }

    [Fact]
    public async Task GivenUnknownGradeJunction_ThenNotFound()
    {
        var result = await _controller.GetGradeJunctionV2(Fixture.Create<int>(), ApiOptions, Store);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GivenRemovedGradeJunction_ThenGone()
    {
        var gradeJunctionWasAdded = Fixture.Create<GradeJunctionWasAdded>();
        var readItem = BuildReadItem(gradeJunctionWasAdded);
        readItem.IsRemoved = true;
        Seed(readItem);

        var result = await _controller.GetGradeJunctionV2((int)gradeJunctionWasAdded.GradeJunctionId, ApiOptions, Store);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
    }

    private static GradeJunctionReadItem BuildReadItem(GradeJunctionWasAdded e)
    {
        return new GradeJunctionReadItem
        {
            GradeJunctionId = e.GradeJunctionId,
            RoadSegmentId1 = e.RoadSegmentId1,
            RoadSegmentId2 = e.RoadSegmentId2,
            Origin = e.Provenance.ToEventTimestamp(),
            LastModified = e.Provenance.ToEventTimestamp(),
            IsV2 = true
        };
    }
}
