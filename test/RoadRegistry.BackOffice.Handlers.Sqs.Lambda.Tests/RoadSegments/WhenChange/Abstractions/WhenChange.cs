namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChange.Abstractions;

using Fixtures;
using Xunit.Abstractions;

public abstract class WhenChange<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeFixture
{
    protected TFixture Fixture;
    protected ITestOutputHelper OutputHelper;

    protected WhenChange(TFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        OutputHelper = testOutputHelper;
    }
}