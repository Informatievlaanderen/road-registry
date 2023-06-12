namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange;

using Abstractions;
using Api.RoadSegments.Change;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeWithInvalidAttribuut : WhenChangeWithInvalidRequest<WhenChangeWithInvalidRequestFixture>
{
    public WhenChangeWithInvalidAttribuut(WhenChangeWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Attribuut_JsonInvalid()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new ()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id
            }
        }, "JsonInvalid", null);
    }
}
