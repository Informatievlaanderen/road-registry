namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenRemovingRoadSegments;

using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Api.RoadSegments;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class GivenRoadNetwork : RemoveRoadSegmentsTestBase
{
    [Fact]
    public async Task ThenAcceptedResult()
    {
        // Arrange
        var request = new DeleteRoadSegmentsParameters
        {
            Wegsegmenten = [TestData.Segment1Added.Id]
        };

        // Act
        var result = await GetResultAsync(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new int[0])]
    public async Task WithNullOrEmptyWegsegmenten_ThenBadRequest(int[] wegsegmenten)
    {
        await ItShouldHaveExpectedError(new DeleteRoadSegmentsParameters
        {
            Wegsegmenten = wegsegmenten
        }, "JsonInvalid", null);
    }

    [Fact]
    public async Task WithInvalidWegsegmenten_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new DeleteRoadSegmentsParameters
        {
            Wegsegmenten = [-1]
        }, "JsonInvalid", null);
    }

    [Fact]
    public async Task WithDuplicateWegsegmenten_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new DeleteRoadSegmentsParameters
        {
            Wegsegmenten = [TestData.Segment1Added.Id, TestData.Segment1Added.Id]
        }, "WegsegmentenNietUniek", null);
    }

    [Fact]
    public async Task WithNonExistingWegsegmenten_ThenBadRequest()
    {
        Mediator
            .Setup(x => x.Send(It.IsAny<DeleteRoadSegmentsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RoadSegmentsNotFoundException([new RoadSegmentId(1)]));

        await ItShouldHaveExpectedError(new DeleteRoadSegmentsParameters
        {
            Wegsegmenten = [1]
        }, "NotFound", null);
    }
}
