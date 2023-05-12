namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidAttribuutwaarde : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidAttribuutwaarde(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Attribuutwaarde_JsonInvalid(string attribuutWaarde)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = null,
                Attribuutwaarde = attribuutWaarde,
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id }
            }
        }, "JsonInvalid", null);
    }
}