namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegmentsOutline.Abstractions;

using Fixtures;
using Xunit.Abstractions;

public abstract class WhenCreateOutline<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenCreateOutlineFixture
{
    protected TFixture Fixture;
    protected ITestOutputHelper OutputHelper;

    protected WhenCreateOutline(TFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        OutputHelper = testOutputHelper;
    }
}
