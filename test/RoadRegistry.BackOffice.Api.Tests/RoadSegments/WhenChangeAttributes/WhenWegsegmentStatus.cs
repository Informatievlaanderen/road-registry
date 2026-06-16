namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeAttributes;

public class WhenWegsegmentStatus : ChangeAttributesTestBase
{
    [Fact]
    public async Task ThenAcceptedResult()
    {
        // Arrange
        await GivenRoadNetwork();

        var request = new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmentstatus = Fixture.Create<RoadSegmentStatus>().ToDutchString(),
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        };

        // Act
        var result = await GetResultAsync(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WithLegeWegsegmentStatus_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmentstatus = string.Empty,
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        }, "WegsegmentStatusNietCorrect", null);
    }
}
