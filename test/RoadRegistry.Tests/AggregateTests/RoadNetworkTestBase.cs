namespace RoadRegistry.Tests.AggregateTests;

using Framework;
using RoadRegistry.RoadNetwork;

public abstract class RoadNetworkTestBase : AggregateTestBase
{
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    protected RoadNetworkTestBase()
    {
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
