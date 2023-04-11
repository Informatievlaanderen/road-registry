namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeOutlineGeometry.Abstractions;

using Fixtures;
using Xunit.Abstractions;

public abstract class WhenChangeOutlineGeometry<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeOutlineGeometryFixture
{
    protected TFixture Fixture;
    protected ITestOutputHelper OutputHelper;

    protected WhenChangeOutlineGeometry(TFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        OutputHelper = testOutputHelper;
    }
}
