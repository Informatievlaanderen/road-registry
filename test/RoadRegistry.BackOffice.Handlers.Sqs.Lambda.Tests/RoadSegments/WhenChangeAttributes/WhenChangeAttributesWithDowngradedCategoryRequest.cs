namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithDowngradedCategoryRequest : WhenChangeAttributes<WhenChangeAttributesWithDowngradedCategoryFixture>
{
    public WhenChangeAttributesWithDowngradedCategoryRequest(WhenChangeAttributesWithDowngradedCategoryFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
