namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

[Collection("runsequential")]
public class WhenDeleteOutlineWithValidRequest : WhenDeleteOutline<WhenDeleteOutlineWithValidRequestFixture>
{
    public WhenDeleteOutlineWithValidRequest(WhenDeleteOutlineWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
