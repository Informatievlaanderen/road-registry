namespace RoadRegistry.LegacyStreamLoader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Events;
    using System.IO.Compression;

    public class LegacyStreamFileReader
    {

        public LegacyStreamFileReader(JsonSerializerSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public JsonSerializerSettings Settings { get; }

        public IEnumerable<StreamEvent> Read(Func<Stream> getZipContentStream)
        {
            if(null == getZipContentStream)
                yield break;

            var serializer = JsonSerializer.Create(Settings);

            // Import the legacy stream from a json file
            using (var zipContent = getZipContentStream())
            using (var archive = new ZipArchive(zipContent, ZipArchiveMode.Read))
            {
                var entry = archive.GetEntry("streams.json");
                using (var entryStream = entry.Open())
                using (var reader = new JsonTextReader(new StreamReader(entryStream)))
                {
                    reader.Read(); // StartArray
                    while (reader.Read() && reader.TokenType != JsonToken.EndArray) // StartObject
                    {
                        reader.Read(); // PropertyName - Stream
                        var stream = reader.ReadAsString();

                        reader.Read(); // PropertyName - Events
                        reader.Read(); // StartArray

                        while (reader.Read() && reader.TokenType != JsonToken.EndArray) // StartObject
                        {
                            reader.Read(); // PropertyName = TypeOfEvent
                            switch (reader.Value)
                            {
                                case nameof(ImportedOrganization):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent {
                                        Stream = stream,
                                        Event = serializer.Deserialize<ImportedOrganization>(reader)
                                    };
                                    break;

                                case nameof(ImportedGradeSeparatedJunction):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent {
                                        Stream = stream,
                                        Event = serializer.Deserialize<ImportedGradeSeparatedJunction>(reader)
                                    };
                                    break;

                                case nameof(ImportedReferencePoint):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent {
                                        Stream = stream,
                                        Event = serializer.Deserialize<ImportedReferencePoint>(reader)
                                    };
                                    break;

                                case nameof(ImportedRoadNode):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent {
                                        Stream = stream,
                                        Event = serializer.Deserialize<ImportedRoadNode>(reader)
                                    };
                                    break;

                                case nameof(ImportedRoadSegment):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent {
                                        Stream = stream,
                                        Event = serializer.Deserialize<ImportedRoadSegment>(reader)
                                    };
                                    break;

                                case nameof(ImportLegacyRegistryStarted):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent
                                    {
                                        Stream = stream,
                                        Event = serializer.Deserialize<ImportLegacyRegistryStarted>(reader)
                                    };
                                    break;

                                case nameof(ImportLegacyRegistryFinished):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent
                                    {
                                        Stream = stream,
                                        Event = serializer.Deserialize<ImportLegacyRegistryFinished>(reader)
                                    };
                                    break;
                            }

                            reader.Read(); //EndObject
                        }

                        reader.Read(); //EndObject
                    }
                }
            }
        }
    }
}
