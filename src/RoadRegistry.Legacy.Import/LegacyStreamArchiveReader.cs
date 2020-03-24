namespace RoadRegistry.Legacy.Import
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using System.IO.Compression;
    using BackOffice.Framework;
    using BackOffice.Messages;

    internal class LegacyStreamArchiveReader
    {
        public LegacyStreamArchiveReader(JsonSerializerSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public JsonSerializerSettings Settings { get; }

        public IEnumerable<StreamEvent> Read(Stream archiveStream)
        {
            if (archiveStream == null) throw new ArgumentNullException(nameof(archiveStream));

            var serializer = JsonSerializer.Create(Settings);

            // Import the legacy stream from a json file
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read))
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
                                    yield return new StreamEvent(new StreamName(stream),
                                        serializer.Deserialize<ImportedOrganization>(reader));
                                    break;

                                case nameof(ImportedGradeSeparatedJunction):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent(new StreamName(stream),
                                        serializer.Deserialize<ImportedGradeSeparatedJunction>(reader));
                                    break;

                                case nameof(ImportedRoadNode):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent(new StreamName(stream),
                                        serializer.Deserialize<ImportedRoadNode>(reader));
                                    break;

                                case nameof(ImportedRoadSegment):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent(new StreamName(stream),
                                        serializer.Deserialize<ImportedRoadSegment>(reader));
                                    break;

                                case nameof(BeganRoadNetworkImport):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent(new StreamName(stream),
                                        serializer.Deserialize<BeganRoadNetworkImport>(reader));
                                    break;

                                case nameof(CompletedRoadNetworkImport):
                                    reader.Read(); // StartObject (move to content for deserializer to work)
                                    yield return new StreamEvent(new StreamName(stream),
                                        serializer.Deserialize<CompletedRoadNetworkImport>(reader));
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
