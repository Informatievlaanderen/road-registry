namespace RoadRegistry.LegacyStreamLoader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Events;

    public class LegacyStreamFileReader
    {
        public LegacyStreamFileReader(JsonSerializerSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public JsonSerializerSettings Settings { get; }

        public IEnumerable<object> Read(FileInfo file)
        {
            var serializer = JsonSerializer.Create(Settings);

            // Import the legacy stream from a json file
            using (var stream = file.OpenRead())
            {
                using (var reader = new JsonTextReader(new StreamReader(stream)))
                {
                    reader.Read(); // StartArray
                    while (reader.Read() && reader.TokenType != JsonToken.EndArray) // StartObject
                    {
                        reader.Read(); // PropertyName

                        switch (reader.Value)
                        {
                            case nameof(ImportedGradeSeparatedJunction):
                                reader.Read(); // StartObject (move to content for deserializer to work)
                                yield return serializer.Deserialize<ImportedGradeSeparatedJunction>(reader);
                                break;

                            case nameof(ImportedReferencePoint):
                                reader.Read(); // StartObject (move to content for deserializer to work)
                                yield return serializer.Deserialize<ImportedReferencePoint>(reader);
                                break;

                            case nameof(ImportedRoadNode):
                                reader.Read(); // StartObject (move to content for deserializer to work)
                                yield return serializer.Deserialize<ImportedRoadNode>(reader);
                                break;

                            case nameof(ImportedRoadSegment):
                                reader.Read(); // StartObject (move to content for deserializer to work)
                                yield return serializer.Deserialize<ImportedRoadSegment>(reader);
                                break;
                        }

                        reader.Read(); //EndObject
                    }
                }
            }
        }
    }
}
