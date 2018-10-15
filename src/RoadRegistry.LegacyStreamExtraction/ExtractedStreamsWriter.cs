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
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("streams.json", CompressionLevel.Optimal);
                using (var entryStream = entry.Open())
                using (var writer = new JsonTextWriter(new StreamWriter(entryStream)))
                {
                    await writer.WriteStartArrayAsync(); // begin all streams

                    foreach (var organization in organizations)
                    {
                        await WriteStream(
                            writer,
                            serializer,
                            "organization-" + organization.Code.ToLowerInvariant(),
                            new[] { organization }
                        );
                    }

                    await WriteStream(
                        writer,
                        serializer,
                        "roadnetwork",
                        async (wr, ser) =>
                        {
                            await WriteEvents(wr, ser, nodes);
                            await WriteEvents(wr, ser, segments);
                            await WriteEvents(wr, ser, junctions);
                            await WriteEvents(wr, ser, points);
                            await WriteEvents(wr, ser, nodes);
                        }
                    );

                    await writer.WriteEndArrayAsync(); //end all streams

                    await writer.FlushAsync();

                    await entryStream.FlushAsync();
                }

                await fileStream.FlushAsync();
            }
        }

        private static async Task WriteStream<TEvent>(
            JsonTextWriter writer,
            JsonSerializer serializer,
            string stream,
            IEnumerable<TEvent> events
        )
        {
            await WriteStream(
                writer,
                serializer,
                stream,
                async (wr, ser) => { await WriteEvents(wr, ser, events); }
            );
        }

        private static async Task WriteStream(
            JsonTextWriter writer,
            JsonSerializer serializer,
            string stream,
            Func<JsonTextWriter, JsonSerializer, Task> writeEvents
        )
        {
            await writer.WriteStartObjectAsync(); // begin stream

            await writer.WritePropertyNameAsync("Stream");
            await writer.WriteValueAsync(stream);
            await writer.WritePropertyNameAsync("Events");
            await writer.WriteStartArrayAsync(); // begin events

            await writeEvents(writer, serializer);

            await writer.WriteEndArrayAsync(); // end events
            await writer.WriteEndObjectAsync(); // end stream
        }

        private static async Task WriteEvents<TEvent>(
            JsonTextWriter writer,
            JsonSerializer serializer,
            IEnumerable<TEvent> events
        )
        {
            foreach(var @event in events)
            {
                await writer.WriteStartObjectAsync(); // begin event
                await writer.WritePropertyNameAsync(typeof(TEvent).Name);
                serializer.Serialize(writer, @event);
                await writer.WriteEndObjectAsync(); // end event
            }
        }
    }
}
