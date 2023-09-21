namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using Abstractions;
using Api.RoadSegments.ChangeDynamicAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeDynamicAttributesWithInvalidItem : WhenChangeDynamicAttributesWithInvalidRequest<WhenChangeDynamicAttributesWithInvalidRequestFixture>
{
    public WhenChangeDynamicAttributesWithInvalidItem(WhenChangeDynamicAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            null
        }, "JsonInvalid");
    }
}
