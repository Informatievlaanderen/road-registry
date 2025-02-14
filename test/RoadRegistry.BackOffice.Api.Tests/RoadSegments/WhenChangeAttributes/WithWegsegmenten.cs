namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Api.RoadSegments.ChangeAttributes;

public class WithWegsegmenten : ChangeAttributesTestBase
{
    [Theory]
    [InlineData(null)]
    [InlineData(new int[0])]
    [InlineData(new[] { 0 })]
    public async Task WithInvalidWegsegmenten_ThenBadRequest(int[] wegsegmenten)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegbeheerder = TestData.ChangedByOrganization,
                Wegsegmenten = wegsegmenten
            }
        }, "JsonInvalid", null);
    }

    [Fact]
    public async Task WithUnknownWegsegmenten_ThenBadRequest()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegbeheerder = TestData.ChangedByOrganization,
                Wegsegmenten = [int.MaxValue]
            }
        }, "NotFound", null);
    }
}
