namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class GivenDowngradedCategoryForOutlined_ThenSucceeded : WhenChangeAttributes<GivenDowngradedCategoryForOutlined_ThenSucceededFixture>
{
    public GivenDowngradedCategoryForOutlined_ThenSucceeded(GivenDowngradedCategoryForOutlined_ThenSucceededFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
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
