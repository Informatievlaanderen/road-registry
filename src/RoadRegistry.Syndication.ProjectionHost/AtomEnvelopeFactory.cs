namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Mapping;
    using Microsoft.SyndicationFeed;
    using Projections;

    public class AtomEnvelopeFactory
    {
        private readonly EventSerializerMapping _eventSerializers;
        private readonly AtomEntrySerializerMapping _atomEntrySerializerMapping;

        public AtomEnvelopeFactory(
            EventSerializerMapping eventSerializerMapping,
            AtomEntrySerializerMapping atomEntrySerializerMapping)
        {
            _eventSerializers = eventSerializerMapping;
            _atomEntrySerializerMapping = atomEntrySerializerMapping;
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
            return _atomEntrySerializerMapping.HasSerializerFor(categoryName) ? _atomEntrySerializerMapping.GetSerializerFor(categoryName) : null;
        }

        private DataContractSerializer FindEventSerializer(AtomEntry atomEntry)
        {
            var eventName = atomEntry.FeedEntry.Title.Split('-')[0];
            return _eventSerializers.HasSerializerFor(eventName) ? _eventSerializers.GetSerializerFor(eventName) : null;
        }
    }
}
