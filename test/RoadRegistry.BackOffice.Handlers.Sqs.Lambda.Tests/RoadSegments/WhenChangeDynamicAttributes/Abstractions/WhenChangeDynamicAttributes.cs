namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenChangeDynamicAttributes.Abstractions;

using Fixtures;
using Xunit.Abstractions;

public abstract class WhenChangeDynamicAttributes<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenChangeDynamicAttributesFixture
{
    protected TFixture Fixture;
    protected ITestOutputHelper OutputHelper;

    protected WhenChangeDynamicAttributes(TFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        OutputHelper = testOutputHelper;
    }
}