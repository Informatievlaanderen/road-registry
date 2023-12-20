namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using Abstractions;
using Api.RoadSegments.ChangeDynamicAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeDynamicAttributesWithInvalidAttribuut : WhenChangeDynamicAttributesWithInvalidRequest<WhenChangeDynamicAttributesWithInvalidRequestFixture>
{
    public WhenChangeDynamicAttributesWithInvalidAttribuut(WhenChangeDynamicAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Attribuut_JsonInvalid()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new ()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id
            }
        }, "JsonInvalid", null);
    }
}
