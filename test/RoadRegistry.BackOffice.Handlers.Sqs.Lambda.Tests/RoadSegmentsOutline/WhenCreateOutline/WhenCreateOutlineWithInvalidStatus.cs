namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidStatus : WhenCreateOutline<WhenCreateOutlineWithInvalidStatusFixture>
{
    public WhenCreateOutlineWithInvalidStatus(WhenCreateOutlineWithInvalidStatusFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
