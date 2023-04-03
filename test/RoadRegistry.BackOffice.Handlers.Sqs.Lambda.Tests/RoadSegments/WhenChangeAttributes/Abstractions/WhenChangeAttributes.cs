namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeAttributes.Abstractions;

using Fixtures;
using Xunit.Abstractions;

public abstract class WhenChangeAttributes<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeAttributesFixture
{
    protected TFixture Fixture;
    protected ITestOutputHelper OutputHelper;

    protected WhenChangeAttributes(TFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        OutputHelper = testOutputHelper;
    }
}
