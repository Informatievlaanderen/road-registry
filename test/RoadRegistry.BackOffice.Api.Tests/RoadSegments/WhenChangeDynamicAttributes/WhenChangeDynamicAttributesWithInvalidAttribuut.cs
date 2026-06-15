namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using Abstractions;
using Fixtures;
using RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeDynamicAttributes;
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
