namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChange;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeWithValidRequest : WhenChange<WhenChangeWithValidRequestFixture>
{
    public WhenChangeWithValidRequest(WhenChangeWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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