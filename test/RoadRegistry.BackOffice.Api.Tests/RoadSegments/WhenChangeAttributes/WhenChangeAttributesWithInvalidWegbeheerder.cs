namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments;
using Api.RoadSegments.ChangeAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegbeheerder : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegbeheerder(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Wegbeheerder_WegbeheerderNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegbeheerder = string.Empty,
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id }
            }
        }, "WegbeheerderNietCorrect", null);
    }
}
