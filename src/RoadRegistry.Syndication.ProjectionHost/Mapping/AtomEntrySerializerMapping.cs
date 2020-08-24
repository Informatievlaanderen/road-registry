namespace RoadRegistry.Syndication.ProjectionHost.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using Projections;

    public class AtomEntrySerializerMapping
    {
        private readonly IReadOnlyDictionary<string, DataContractSerializer> _atomEntrySerializers;

        public AtomEntrySerializerMapping()
        {
            _atomEntrySerializers = new ReadOnlyDictionary<string, DataContractSerializer>(
                new Dictionary<string, DataContractSerializer>
                {
                    {
                        "https://data.vlaanderen.be/ns/gemeente",
                        new DataContractSerializer(typeof(SyndicationContent<Gemeente>))
                    },
                    {
                        "https://data.vlaanderen.be/ns/straatnaam",
                        new DataContractSerializer(typeof(SyndicationContent<StraatNaam>))
                    }
                });
        }

        public DataContractSerializer GetSerializerFor(string categoryName)
        {
            if (_atomEntrySerializers.ContainsKey(categoryName))
                return _atomEntrySerializers[categoryName];

            throw new KeyNotFoundException($"No serializer for category with name '{categoryName}' was found.");
        }

        public bool TryGetSerializerFor(string categoryName, out DataContractSerializer eventType) =>
            _atomEntrySerializers.TryGetValue(categoryName, out eventType);

        public bool HasSerializerFor(string categoryName) =>
            _atomEntrySerializers.ContainsKey(categoryName);
    }
}
