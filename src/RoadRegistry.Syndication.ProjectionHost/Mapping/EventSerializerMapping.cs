namespace RoadRegistry.Syndication.ProjectionHost.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    public class EventSerializerMapping
    {
        private readonly IReadOnlyDictionary<string, DataContractSerializer> _eventSerializers;

        private EventSerializerMapping(IReadOnlyDictionary<string, DataContractSerializer> eventSerializers)
        {
            _eventSerializers = eventSerializers;
        }

        public static EventSerializerMapping CreateForNamespaceOf(Type eventType)
        {
            var eventNamespace = eventType.Namespace;
            return new EventSerializerMapping(new ReadOnlyDictionary<string, DataContractSerializer>(
                Assembly
                    .GetAssembly(eventType)
                    ?.GetTypes()
                    .Where(t => t.Namespace != null &&
                                (t.Namespace == eventNamespace))
                    .ToDictionary(
                        type => type.Name,
                        type => new DataContractSerializer(type))
                ?? throw new InvalidOperationException($"No assembly found for event type '{eventType}'")));
        }

        public DataContractSerializer GetSerializerFor(string eventName)
        {
            if (_eventSerializers.ContainsKey(eventName))
                return _eventSerializers[eventName];

            throw new KeyNotFoundException($"No serializer for event with name '{eventName}' was found.");
        }

        public bool HasSerializerFor(string eventName) =>
            _eventSerializers.ContainsKey(eventName);
    }
}
