namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments.ChangeAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidAttribuut : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidAttribuut(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Attribuut_JsonInvalid()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id }
            }
        }, "JsonInvalid", null);
    }
}
