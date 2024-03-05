namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Api.RoadSegments.ChangeAttributes;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegbeheerder : WhenChangeAttributesWithInvalidRequest<WhenChangeAttributesWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegbeheerder(WhenChangeAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task Wegbeheerder_WegbeheerderNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegbeheerder = string.Empty,
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id }
            }
        }, "WegbeheerderNietCorrect", null);
    }

    [Fact]
    public async Task Wegbeheerder_WegbeheerderNietGekend()
    {
        var wegbeheerder = new OrganizationId(Fixture.TestData.Segment1Added.MaintenanceAuthority.Code);

        Fixture.CustomizeOrganizationCache(new FakeOrganizationCache().Seed(wegbeheerder, null));

        await ItShouldHaveExpectedError(new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegbeheerder = wegbeheerder,
                Wegsegmenten = new[] { Fixture.TestData.Segment1Added.Id }
            }
        }, "WegbeheerderNietGekend", null);
    }
}
