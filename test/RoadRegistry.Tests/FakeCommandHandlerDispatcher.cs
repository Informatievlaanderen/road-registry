namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice.Framework;

public class FakeCommandHandlerDispatcher
{
    public CommandHandlerDispatcher Dispatcher { get; }
    public IReadOnlyList<Command> Invocations => _invocations.AsReadOnly();

    private readonly List<Command> _invocations = [];

    public FakeCommandHandlerDispatcher()
    {
        Dispatcher = (command, _) =>
        {
            _invocations.Add(command);
            return Task.CompletedTask;
        };
    }
}
