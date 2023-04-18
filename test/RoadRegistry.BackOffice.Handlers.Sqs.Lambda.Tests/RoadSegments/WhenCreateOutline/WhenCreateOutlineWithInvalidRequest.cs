namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidRequest : WhenCreateOutline<WhenCreateOutlineWithInvalidRequestFixture>
{
    public WhenCreateOutlineWithInvalidRequest(WhenCreateOutlineWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
