namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public static partial class ScenarioExtensions
{
    public static IScenarioGivenStateBuilder Given(
        this IScenarioInitialStateBuilder builder,
        string stream,
        params object[] events)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var name = new StreamName(stream);
        return builder.Given(events.Select(@event => new RecordedEvent(name, @event)));
    }

    public static IScenarioGivenStateBuilder Given(
        this IScenarioGivenStateBuilder builder,
        string stream,
        params object[] events)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var name = new StreamName(stream);
        return builder.Given(events.Select(@event => new RecordedEvent(name, @event)));
    }

    public static IScenarioThenStateBuilder Then(
        this IScenarioWhenStateBuilder builder,
        string stream,
        params object[] events)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var name = new StreamName(stream);
        return builder.Then(events.Select(@event => new RecordedEvent(name, @event)));
    }

    public static IScenarioThenStateBuilder Then(
        this IScenarioThenStateBuilder builder,
        string stream,
        params object[] events)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var name = new StreamName(stream);
        return builder.Then(events.Select(@event => new RecordedEvent(name, @event)));
    }
}
