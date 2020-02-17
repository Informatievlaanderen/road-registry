namespace RoadRegistry.BackOffice.EventHost
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Framework;

    public static class AcceptStreamMessage
    {
        public static AcceptStreamMessageFilter WhenEqualToMessageType(EventHandlerModule[] modules, EventMapping mapping)
        {
            var acceptableEventNames = new HashSet<string>(
                modules
                    .SelectMany(module => module.Handlers)
                    .Select(handler => handler.Event)
                    .Distinct()
                    .Where(mapping.HasEventName)
                    .Select(mapping.GetEventName)
            );
            return message => mapping.HasEventType(message.Type) && acceptableEventNames.Contains(message.Type);
        }
    }
}
