namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Microsoft.SyndicationFeed;
    using Projections;
    using Projections.MunicipalityEvents;

    public class AtomEnvelopeFactory
    {
        private readonly Dictionary<string, DataContractSerializer> _eventSerializers;
        private readonly Dictionary<string, DataContractSerializer> _atomEntrySerializers;

        public AtomEnvelopeFactory()
        {
            _eventSerializers = Assembly
                .GetAssembly(typeof(MunicipalityWasRegistered))
                ?.GetTypes()
                .Where(t => t.Namespace != null &&
                            t.Namespace.EndsWith("MunicipalityEvents"))
                .ToDictionary(
                    type => type.Name,
                    type => new DataContractSerializer(type));

            _atomEntrySerializers = new Dictionary<string, DataContractSerializer>{
            {
                "https://data.vlaanderen.be/ns/gemeente", new DataContractSerializer(typeof(SyndicationContent<Gemeente>))
            }};
        }

        public object CreateEnvelope(IAtomEntry message)
        {
            using (var contentXmlReader =
                XmlReader.Create(
                    new StringReader(message.Description),
                    new XmlReaderSettings {Async = true}))
            {
                var atomEntrySerializer = FindAtomEntrySerializer(message);
                if (atomEntrySerializer == null)
                    return null;

                var atomEntry = new AtomEntry(message, atomEntrySerializer.ReadObject(contentXmlReader));

                using (var eventXmlReader =
                    XmlReader.Create(
                        new StringReader(((SyndicationContent<Gemeente>) atomEntry.Content).Event.OuterXml)))
                {
                    var serializer = FindEventSerializer(atomEntry);
                    if (serializer == null)
                        return null;

                    var @event = serializer.ReadObject(eventXmlReader);

                    var metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        [Envelope.PositionMetadataKey] = atomEntry.FeedEntry.Id,
                        [Envelope.EventNameMetadataKey] = atomEntry.FeedEntry.Title,
                        [Envelope.CreatedUtcMetadataKey] = atomEntry.FeedEntry.Published
                    };

                    var envelope =
                        new Envelope(
                                @event,
                                metadata)
                            .ToGenericEnvelope();

                    return envelope;
                }
            }
        }

        private DataContractSerializer FindAtomEntrySerializer(ISyndicationItem message)
        {
            var categoryName = message.Categories.First().Name;
            return _atomEntrySerializers.ContainsKey(categoryName) ? _atomEntrySerializers[categoryName] : null;
        }

        private DataContractSerializer FindEventSerializer(AtomEntry atomEntry)
        {
            var eventName = atomEntry.FeedEntry.Title.Split('-')[0];
            return _eventSerializers.ContainsKey(eventName) ? _eventSerializers[eventName] : null;
        }
    }
}
