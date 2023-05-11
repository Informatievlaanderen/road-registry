namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments;
using BackOffice.Abstractions.RoadSegments;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegsegmenten : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegsegmenten(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new int[0])]
    [InlineData(new int[] { 0 })]
    public async Task Wegsegmenten_JsonInvalid(int[] wegsegmenten)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Wegbeheerder.ToString(),
                Attribuutwaarde = Fixture.TestData.ChangedByOrganization,
                Wegsegmenten = wegsegmenten
            }
        }, "JsonInvalid", null);
    }

    [Fact]
    public async Task Wegsegmenten_NotFound()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Wegbeheerder.ToString(),
                Attribuutwaarde = Fixture.TestData.ChangedByOrganization,
                Wegsegmenten = new []{ int.MaxValue }
            }
        }, "NotFound", null);
    }
}
