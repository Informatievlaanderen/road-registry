namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeOutlineGeometryWithInvalidGeometry : WhenChangeOutlineGeometry<WhenChangeOutlineGeometryWithInvalidGeometryFixture>
{
    public WhenChangeOutlineGeometryWithInvalidGeometry(WhenChangeOutlineGeometryWithInvalidGeometryFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
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
