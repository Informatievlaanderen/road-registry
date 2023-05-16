namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.WhenDeleteOutline.Abstractions;

using Fixtures;
using Xunit.Abstractions;

public abstract class WhenDeleteOutline<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenDeleteOutlineFixture
{
    protected TFixture Fixture;
    protected ITestOutputHelper OutputHelper;

    protected WhenDeleteOutline(TFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        OutputHelper = testOutputHelper;
    }
}