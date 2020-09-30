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
    using Projections.Syndication;

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

        public object CreateEnvelope<T>(IAtomEntry message)
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
                        new StringReader(((SyndicationContent<T>) atomEntry.Content).Event.OuterXml)))
                {
                    var serializer = FindEventSerializer(atomEntry);
                    if (serializer == null)
                        return null;

                    var @event = serializer.ReadObject(eventXmlReader);

                    var metadata = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        [Envelope.PositionMetadataKey] = Convert.ToInt64(atomEntry.FeedEntry.Id),
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
            if (!message.Categories.Any())
                throw new FormatException($"No category found in message with id {message.Id}.");

            var categoryName = message.Categories.First().Name;
            return _atomEntrySerializerMapping.HasSerializerFor(categoryName) ? _atomEntrySerializerMapping.GetSerializerFor(categoryName) : null;
        }

        private DataContractSerializer FindEventSerializer(AtomEntry atomEntry)
        {
            var splitTitle = atomEntry.FeedEntry.Title.Split('-');
            if(!splitTitle.Any())
                throw new FormatException($"Could not find event name in atom entry with id {atomEntry.FeedEntry.Id}. Title was '{atomEntry.FeedEntry.Title}'.");

            var eventName = splitTitle[0];
            return _eventSerializers.HasSerializerFor(eventName) ? _eventSerializers.GetSerializerFor(eventName) : null;
        }
    }
}
