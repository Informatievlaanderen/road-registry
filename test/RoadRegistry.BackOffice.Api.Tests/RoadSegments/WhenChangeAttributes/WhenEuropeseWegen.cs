namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;

public class WhenEuropeseWegen : ChangeAttributesTestBase
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
                EuropeseWegen =
                [
                    new ChangeAttributeEuropeanRoad
                    {
                        EuNummer = Fixture.Create<EuropeanRoadNumber>().ToString()
                    }
                ],
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        };

        // Act
        var result = await GetResultAsync(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WithNietUniek_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                EuropeseWegen =
                [
                    new ChangeAttributeEuropeanRoad { EuNummer = "E40" },
                    new ChangeAttributeEuropeanRoad { EuNummer = "E40" }
                ]
            }
        }, "EuropeseWegenNietUniek", null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WithOntbrekendEuNummer_ThenBadRequest(string nummer)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                EuropeseWegen =
                [
                    new ChangeAttributeEuropeanRoad { EuNummer = nummer }
                ]
            }
        }, "EuropeseWegEuNummerVerplicht", null);
    }

    [Fact]
    public async Task WithEuNummerNietCorrect_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = [TestData.Segment1Added.Id],
                EuropeseWegen =
                [
                    new ChangeAttributeEuropeanRoad { EuNummer = "-" }
                ]
            }
        }, "EuropeseWegEuNummerNietCorrect", null);
    }
}
