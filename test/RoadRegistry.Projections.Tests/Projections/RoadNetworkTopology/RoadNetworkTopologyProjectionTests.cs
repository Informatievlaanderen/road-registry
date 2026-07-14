namespace RoadRegistry.Projections.Tests.Projections.RoadNetworkTopology;

using FluentAssertions;
using Infrastructure.MartenDb.Projections;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;

public class RoadNetworkTopologyProjectionTests
{
    [Fact]
    public void EnsureAllEventsAreHandled()
    {
        // Topology only tracks the junction's road-segment relationships; the junction point (geometry-changed events)
        // does not affect topology, so those events are consciously not handled here.
        var excludeEventTypes = new[]
        {
            typeof(GradeJunctionGeometryWasChanged),
            typeof(GradeSeparatedJunctionGeometryWasChanged),
            typeof(GradeSeparatedJunctionGeometryModified)
        };

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

        var missingEventTypes = allEventTypes.Except(usedEventTypes).Except(excludeEventTypes).ToArray();
        if (missingEventTypes.Any())
        {
            Assert.Fail($"Missing handlers for event types:{Environment.NewLine}{string.Join(Environment.NewLine, missingEventTypes.Select(x => x.Name).OrderBy(x => x))}");
        }
    }
}
