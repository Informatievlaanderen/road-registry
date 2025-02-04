namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

public class WhenWegcategorie : ChangeAttributesTestBase
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
                Wegcategorie = Fixture.Create<RoadSegmentCategory>().ToDutchString(),
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        };

        // Act
        var result = await GetResultAsync(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WithLegeWegcategorie_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegcategorie = string.Empty,
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        }, "WegcategorieNietCorrect", null);
    }
}
