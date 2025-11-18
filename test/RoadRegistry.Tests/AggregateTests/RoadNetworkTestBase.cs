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
        return Run((scenario, runner) => builder(scenario).Assert(runner));
    }
    protected Task Run(Func<Scenario, IExpectExceptionScenarioBuilder> builder)
    {
        return Run((scenario, runner) => builder(scenario).Assert(runner));
    }
    private Task Run(Func<Scenario, ScenarioRunner, Task> assert)
    {
        ArgumentNullException.ThrowIfNull(assert);

        var runner = new ScenarioRunner(_roadNetworkIdGenerator);

        return assert(new Scenario(), runner);
    }
}
