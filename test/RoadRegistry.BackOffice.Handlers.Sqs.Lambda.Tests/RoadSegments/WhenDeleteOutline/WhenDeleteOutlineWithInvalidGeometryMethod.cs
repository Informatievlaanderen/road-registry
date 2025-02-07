namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

[Collection("runsequential")]
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
