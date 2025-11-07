namespace RoadRegistry.Tests.AggregateTests;

using AutoFixture;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.Tests.AggregateTests.Framework;

public abstract class RoadNetworkTestBase
{
    protected RoadNetworkTestData TestData { get; }
    protected IFixture Fixture { get; }
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    protected RoadNetworkTestBase()
    {
        TestData = new();
        Fixture = TestData.Fixture;
        _roadNetworkIdGenerator = new FakeRoadNetworkIdGenerator();
    }

    protected Task Run(Func<Scenario, IExpectEventsScenarioBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var scenarioBuilder = builder(new Scenario());

        var runner = new ScenarioRunner(_roadNetworkIdGenerator);
        return scenarioBuilder.AssertAsync(runner);
    }

    protected Task Run(Func<Scenario, IExpectExceptionScenarioBuilder> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var scenarioBuilder = builder(new Scenario());

        var runner = new ScenarioRunner(_roadNetworkIdGenerator);
        return scenarioBuilder.AssertAsync(runner);
    }
}
