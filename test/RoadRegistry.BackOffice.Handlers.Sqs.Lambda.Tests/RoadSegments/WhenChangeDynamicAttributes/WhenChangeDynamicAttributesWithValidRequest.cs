namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeDynamicAttributes;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeDynamicAttributesWithValidRequest : WhenChangeDynamicAttributes<WhenChangeDynamicAttributesWithValidRequestFixture>
{
    public WhenChangeDynamicAttributesWithValidRequest(WhenChangeDynamicAttributesWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
