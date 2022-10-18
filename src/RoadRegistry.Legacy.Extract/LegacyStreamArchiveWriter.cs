// ReSharper disable PossibleNullReferenceException

namespace RoadRegistry.Legacy.Extract
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Microsoft.IO;
    using Newtonsoft.Json;

    internal class LegacyStreamArchiveWriter
    {
        public LegacyStreamArchiveWriter(IBlobClient client, RecyclableMemoryStreamManager manager)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _serializer = JsonSerializer.Create(SerializerSettings);
        }

        private readonly IBlobClient _client;
        private readonly RecyclableMemoryStreamManager _manager;
        private readonly JsonSerializer _serializer;

        private static readonly JsonSerializerSettings SerializerSettings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        public async Task WriteAsync(IEnumerable<StreamEvent> events, CancellationToken cancellationToken = default)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            const int requires400Mb = 419_430_400;

            using (var content = _manager.GetStream(nameof(LegacyStreamArchiveWriter), requires400Mb))
            {
                using (var archive = new ZipArchive(content, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("streams.json", CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    using (var writer = new JsonTextWriter(new StreamWriter(entryStream)))
                    {
                        await writer.WriteStartArrayAsync(cancellationToken);
                        using (var enumerator = events.GetEnumerator())
                        {
                            if (enumerator.MoveNext())
                            {
                                var stream = enumerator.Current.Stream;

                                await WriteBeginStream(stream, writer, cancellationToken);

                                await WriteEvent(enumerator.Current.Event, writer, cancellationToken);

                                while (enumerator.MoveNext())
                                {
                                    // next
                                    if (stream != enumerator.Current.Stream)
                                    {
                                        await WriteEndStream(writer);

                                        stream = enumerator.Current.Stream;

                                        await WriteBeginStream(stream, writer, cancellationToken);
                                    }

                                    await WriteEvent(enumerator.Current.Event, writer, cancellationToken);
                                }

                                await WriteEndStream(writer);
                            }
                        }

                        await writer.WriteEndArrayAsync(cancellationToken);
                        await writer.FlushAsync(cancellationToken);
                    }
                }

                content.Position = 0;

                await _client.CreateBlobAsync(
                    new BlobName("import-streams.zip"),
                    Metadata.None,
                    ContentType.Parse("application/zip"),
                    content,
                    cancellationToken);
            }
        }

        private static async Task WriteBeginStream(
            StreamName stream,
            JsonWriter writer,
            CancellationToken cancellationToken)
        {
            await writer.WriteStartObjectAsync(cancellationToken); // begin stream

            await writer.WritePropertyNameAsync("Stream", cancellationToken);
            await writer.WriteValueAsync(stream.ToString(), cancellationToken);

            await writer.WritePropertyNameAsync("Events", cancellationToken);

            await writer.WriteStartArrayAsync(cancellationToken); // begin events
        }

        private static async Task WriteEndStream(JsonWriter writer)
        {
            await writer.WriteEndArrayAsync(); // end events

            await writer.WriteEndObjectAsync(); // end stream
        }

        private async Task WriteEvent(object @event, JsonWriter writer, CancellationToken cancellationToken)
        {
            await writer.WriteStartObjectAsync(cancellationToken); // begin event
            await writer.WritePropertyNameAsync(@event.GetType().Name, cancellationToken);
            _serializer.Serialize(writer, @event);
            await writer.WriteEndObjectAsync(cancellationToken); // end event
        }
    }
}
