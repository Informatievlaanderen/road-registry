namespace RoadRegistry.LegacyStreamExtraction
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using RoadRegistry.Events;

    internal class ExtractedStreamsWriter
    {
        public ExtractedStreamsWriter(FileInfo output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public FileInfo Output { get; }

        public async Task WriteAsync(
            IEnumerable<ImportedOrganization> organizations,
            IEnumerable<ImportedRoadNode> nodes,
            IEnumerable<ImportedRoadSegment> segments,
            IEnumerable<ImportedGradeSeparatedJunction> junctions,
            IEnumerable<ImportedReferencePoint> points)
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                DateParseHandling = DateParseHandling.DateTime,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            using (var fileStream = Output.OpenWrite())
            using (var archive = new ZipArchive(fileStream))
            {
                var entry = archive.CreateEntry("streams.json", CompressionLevel.Optimal);
                using (var entryStream = entry.Open())
                using (var writer = new JsonTextWriter(new StreamWriter(entryStream)))
                {
                    await writer.WriteStartArrayAsync(); // begin all streams

                    foreach (var organization in organizations)
                    {
                        await writer.WriteStartObjectAsync(); // begin stream

                        await writer.WritePropertyNameAsync("Stream");
                        await writer.WriteValueAsync("organization-" + organization.Code.ToLowerInvariant());

                        await writer.WritePropertyNameAsync("Events");

                        await writer.WriteStartArrayAsync(); // begin events

                        await writer.WriteStartObjectAsync(); // begin event
                        await writer.WritePropertyNameAsync(nameof(ImportedOrganization));
                        serializer.Serialize(writer, organization);
                        await writer.WriteEndObjectAsync(); // end event

                        await writer.WriteEndArrayAsync(); // end events

                        await writer.WriteEndObjectAsync(); // end stream
                    }

                    await writer.WriteStartObjectAsync(); // begin stream

                    await writer.WritePropertyNameAsync("Stream");
                    await writer.WriteValueAsync("roadnetwork");

                    await writer.WritePropertyNameAsync("Events");

                    await writer.WriteStartArrayAsync(); // begin events

                    foreach (var node in nodes)
                    {
                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync(nameof(ImportedRoadNode));
                        serializer.Serialize(writer, node);
                        await writer.WriteEndObjectAsync();
                    }

                    foreach (var segment in segments)
                    {
                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync(nameof(ImportedRoadSegment));
                        serializer.Serialize(writer, segment);
                        await writer.WriteEndObjectAsync();
                    }

                    foreach (var junction in junctions)
                    {
                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync(nameof(ImportedGradeSeparatedJunction));
                        serializer.Serialize(writer, junction);
                        await writer.WriteEndObjectAsync();
                    }

                    foreach (var point in points)
                    {
                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync(nameof(ImportedReferencePoint));
                        serializer.Serialize(writer, point);
                        await writer.WriteEndObjectAsync();
                    }

                    await writer.WriteEndArrayAsync(); // end events

                    await writer.WriteEndObjectAsync(); // end stream

                    await writer.WriteEndArrayAsync(); //end all streams

                    await writer.FlushAsync();

                    await entryStream.FlushAsync();
                }

                await fileStream.FlushAsync();
            }
        }
    }
}
