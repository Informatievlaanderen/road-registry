namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class Resolve
{
    public static CommandHandlerResolver WhenEqualToMessage(CommandHandlerModule[] modules)
    {
        return WhenEqualToMessage(modules.SelectMany(module => module.Handlers).ToArray());
    }

    public static CommandHandlerResolver WhenEqualToMessage(CommandHandlerModule module)
    {
        return WhenEqualToMessage(module.Handlers);
    }

    public static CommandHandlerResolver WhenEqualToMessage(CommandHandler[] handlers)
    {
        if (handlers == null) throw new ArgumentNullException(nameof(handlers));

        var cache = handlers.ToDictionary(handler => handler.Command);

        return message =>
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!cache.TryGetValue(message.Body.GetType(), out var handler)) throw new InvalidOperationException($"The command handler for {message.Body.GetType()} could not be found.");
            return handler.Handler;
        };
    }

    public static EventHandlerResolver WhenEqualToMessage(EventHandlerModule[] modules)
    {
        return WhenEqualToMessage(modules.SelectMany(module => module.Handlers).ToArray());
    }

    public static EventHandlerResolver WhenEqualToMessage(EventHandlerModule module)
    {
        return WhenEqualToMessage(module.Handlers);
    }

    public static EventHandlerResolver WhenEqualToMessage(EventHandler[] handlers)
    {
        if (handlers == null) throw new ArgumentNullException(nameof(handlers));

        var cache = handlers
            .GroupBy(handler => handler.Event)
            .ToDictionary(grouped => grouped.Key, grouped => grouped.ToArray());

        return message =>
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (!cache.TryGetValue(message.Body.GetType(), out var resolved)) return Array.Empty<Func<Event, CancellationToken, Task>>();
            return Array.ConvertAll(resolved, handler => handler.Handler);
        };
    }
}
