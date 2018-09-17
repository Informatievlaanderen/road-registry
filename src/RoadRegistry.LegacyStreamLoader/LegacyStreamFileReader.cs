namespace RoadRegistry.LegacyStreamLoader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Events;
    using System.IO.Compression;
    using Amazon.S3.Model;

    public class LegacyStreamFileReader
    {
        private readonly Action<string> _log;

        public LegacyStreamFileReader(
            JsonSerializerSettings settings,
            Action<string> logAction)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _log = logAction ?? throw new ArgumentNullException(nameof(logAction));
        }

        public JsonSerializerSettings Settings { get; }

        public IEnumerable<StreamEvent> Read(FileInfo file)
        {
            if (null == file || false == file.Exists)
                return new StreamEvent[0];

            _log($"Reading from {file.FullName}");
            return Read(file.OpenRead);
        }

        public IEnumerable<StreamEvent> Read(GetObjectResponse s3Response)
        {
            if (null == s3Response)
                return new StreamEvent[0];

            _log($"Reading from downloaded file S3:{s3Response.BucketName}/{s3Response.Key}");
            return Read(() => s3Response.ResponseStream);
        }

        public IEnumerable<StreamEvent> Read(Func<Stream> getZipContent)
        {
            var serializer = JsonSerializer.Create(Settings);

            // Import the legacy stream from a json file
            using (var zipContent = getZipContent())
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
