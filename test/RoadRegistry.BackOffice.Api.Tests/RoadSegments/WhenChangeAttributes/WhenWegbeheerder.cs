namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Api.RoadSegments.ChangeAttributes;
using Microsoft.AspNetCore.Mvc;

public class WhenWegbeheerder : ChangeAttributesTestBase
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
                Wegbeheerder = TestData.ChangedByOrganization,
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        };

        // Act
        var result = await GetResultAsync(request);

        // Assert
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task WithLegeWegbeheerder_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegbeheerder = string.Empty,
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        }, "WegbeheerderNietCorrect", null);
    }

    [Fact]
    public async Task WithWegbeheerderNietGekend_ThenBadRequest()
    {
        var wegbeheerder = new OrganizationId(TestData.Segment1Added.MaintenanceAuthority.Code);

        OrganizationCache.Seed(wegbeheerder, null);

        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegbeheerder = wegbeheerder,
                Wegsegmenten = [TestData.Segment1Added.Id]
            }
        }, "WegbeheerderNietGekend", null);
    }
}
