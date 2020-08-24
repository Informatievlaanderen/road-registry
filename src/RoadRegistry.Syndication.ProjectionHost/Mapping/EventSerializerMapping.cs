namespace RoadRegistry.Syndication.ProjectionHost.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Projections.MunicipalityEvents;

    public class EventSerializerMapping
    {
        private readonly IReadOnlyDictionary<string, DataContractSerializer> _eventSerializers;

        public EventSerializerMapping()
        {
            _eventSerializers = new ReadOnlyDictionary<string, DataContractSerializer>(
                Assembly
                    .GetAssembly(typeof(MunicipalityWasRegistered))
                    ?.GetTypes()
                    .Where(t => t.Namespace != null &&
                                (t.Namespace.EndsWith("MunicipalityEvents") ||
                                 t.Namespace.EndsWith("StreetNameEvents")))
                    .ToDictionary(
                        type => type.Name,
                        type => new DataContractSerializer(type)));
        }

        public DataContractSerializer GetSerializerFor(string eventName)
        {
            if (_eventSerializers.ContainsKey(eventName))
                return _eventSerializers[eventName];

            throw new KeyNotFoundException($"No serializer for event with name '{eventName}' was found.");
        }

        public bool TryGetSerializerFor(string eventName, out DataContractSerializer eventType) =>
            _eventSerializers.TryGetValue(eventName, out eventType);

        public bool HasSerializerFor(string eventName) =>
            _eventSerializers.ContainsKey(eventName);
    }
}
