namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

public class WhenMorfologischeWegklasse : ChangeAttributesTestBase
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
                MorfologischeWegklasse = Fixture.Create<RoadSegmentMorphology>().ToDutchString(),
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        };

        // Act
        var result = await GetResultAsync(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WithMorfologischeWegklasseNietCorrect_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                MorfologischeWegklasse = string.Empty,
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        }, "MorfologischeWegklasseNietCorrect", null);
    }
}
