namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments.Parameters;
using BackOffice.Abstractions.RoadSegments;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegbeheerder : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegbeheerder(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Wegbeheerder_WegbeheerderVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Wegbeheerder.ToString(),
                Attribuutwaarde = null,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "WegbeheerderVerplicht", null);
    }

    [Fact]
    public async Task Wegbeheerder_WegbeheerderNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Wegbeheerder.ToString(),
                Attribuutwaarde = string.Empty,
                Wegsegmenten = new []{ Fixture.TestData.Segment1Added.Id }
            }
        }, "WegbeheerderNietCorrect", null);
    }
}
