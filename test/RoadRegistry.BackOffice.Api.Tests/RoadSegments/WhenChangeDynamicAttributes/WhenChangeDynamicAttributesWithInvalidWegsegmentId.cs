namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using Abstractions;
using Api.RoadSegments.ChangeDynamicAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeWithInvalidWegsegmentId : WhenChangeDynamicAttributesWithInvalidRequest<WhenChangeDynamicAttributesWithInvalidRequestFixture>
{
    public WhenChangeWithInvalidWegsegmentId(WhenChangeDynamicAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WegsegmentId_JsonInvalid(int? wegsegmentId)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = wegsegmentId,
                Wegbreedte = new []{ new ChangeWidthAttributeParameters
                {
                    VanPositie = 0,
                    TotPositie = 10,
                    Breedte = "1"
                }}
            }
        }, "JsonInvalid", null);
    }

    [Fact]
    public async Task WegsegmentId_NotFound()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = int.MaxValue,
                Wegbreedte = new []{ new ChangeWidthAttributeParameters
                {
                    VanPositie = 0,
                    TotPositie = 10,
                    Breedte = "1"
                }}
            }
        }, "NotFound", null);
    }
}
