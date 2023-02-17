namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithValidRequest : WhenCreateOutline<WhenCreateOutlineWithValidRequestFixture>
{
    public WhenCreateOutlineWithValidRequest(WhenCreateOutlineWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public void ItShouldSucceed()
    {
        if (Fixture.Exception is not null)
        {
            OutputHelper.WriteLine(Fixture.Exception.ToString());
        }

        Assert.True(Fixture.Result);
    }
}
