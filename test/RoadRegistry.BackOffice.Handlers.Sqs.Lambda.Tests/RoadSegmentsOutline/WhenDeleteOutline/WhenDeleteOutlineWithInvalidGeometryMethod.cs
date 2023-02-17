namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.WhenDeleteOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenDeleteOutlineWithInvalidGeometryMethod : WhenDeleteOutline<WhenDeleteOutlineWithInvalidGeometryMethodFixture>
{
    public WhenDeleteOutlineWithInvalidGeometryMethod(WhenDeleteOutlineWithInvalidGeometryMethodFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public void ItShouldThrow()
    {
        if (Fixture.Exception is not null)
        {
            OutputHelper.WriteLine(Fixture.Exception.ToString());
        }

        Assert.True(Fixture.Result);
    }
}
