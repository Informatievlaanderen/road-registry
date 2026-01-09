namespace RoadRegistry.Projections.Tests.Projections.RoadNetworkTopology;

using FluentAssertions;
using Infrastructure.MartenDb.Projections;

public class RoadNetworkTopologyProjectionTests
{
    [Fact]
    public void EnsureAllEventsAreHandled()
    {
        var allEventTypes = typeof(IMartenEvent).Assembly
            .GetTypes()
            .Where(x => typeof(IMartenEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .ToArray();
        allEventTypes.Should().NotBeEmpty();

        var usedEventTypes = typeof(RoadNetworkTopologyProjection)
            .GetMethods()
            .Where(x => x.Name == nameof(RoadNetworkTopologyProjection.Project))
            .Select(x => x.GetParameters().First().ParameterType)
            .Select(type => type.GetGenericArguments().FirstOrDefault() ?? type)
            .ToArray();
        usedEventTypes.Should().NotBeEmpty();

        var missingEventTypes = allEventTypes.Except(usedEventTypes).ToArray();
        if (missingEventTypes.Any())
        {
            Assert.Fail($"Missing handlers for event types:{Environment.NewLine}{string.Join(Environment.NewLine, missingEventTypes.Select(x => x.Name).OrderBy(x => x))}");
        }
    }
}
