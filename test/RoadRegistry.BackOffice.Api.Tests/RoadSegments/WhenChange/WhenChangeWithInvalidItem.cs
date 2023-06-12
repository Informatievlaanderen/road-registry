namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange;

using Abstractions;
using Api.RoadSegments.Change;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeWithInvalidItem : WhenChangeWithInvalidRequest<WhenChangeWithInvalidRequestFixture>
{
    public WhenChangeWithInvalidItem(WhenChangeWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task NullRequest_NotFound()
    {
        await ItShouldHaveExpectedError(null, "NotFound");
    }

    [Fact]
    public async Task NullItem_JsonInvalid()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            null
        }, "JsonInvalid");
    }
}
