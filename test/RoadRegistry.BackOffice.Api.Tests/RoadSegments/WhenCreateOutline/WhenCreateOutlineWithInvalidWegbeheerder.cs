namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Api.RoadSegments;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidWegbeheerder : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidMorphologyFixture>
{
    public WhenCreateOutlineWithInvalidWegbeheerder(WhenCreateOutlineWithInvalidMorphologyFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }
    
    [Fact]
    public async Task Wegbeheerder_WegbeheerderNietCorrect()
    {
        var request = Fixture.Request with
        {
            Wegbeheerder = null
        };
        await ItShouldHaveExpectedError(request, "WegbeheerderNietCorrect", null);
    }

    [Fact]
    public async Task Wegbeheerder_WegbeheerderNietGekend()
    {
        var wegbeheerder = new OrganizationId(Fixture.TestData.Segment1Added.MaintenanceAuthority.Code);

        var request = Fixture.Request with
        {
            Wegbeheerder = wegbeheerder
        };

        Fixture.CustomizeOrganizationRepository(new FakeOrganizationRepository().Seed(wegbeheerder, null));

        await ItShouldHaveExpectedError(request, "WegbeheerderNietGekend", null);
    }
}
