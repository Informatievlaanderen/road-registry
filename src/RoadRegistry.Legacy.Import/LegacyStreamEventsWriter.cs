namespace RoadRegistry.Legacy.Import;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

internal class LegacyStreamEventsWriter
{
    private static readonly EventMapping Mapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
    private readonly ILogger<LegacyStreamEventsWriter> _logger;

    private readonly IStreamStore _streamStore;

    public LegacyStreamEventsWriter(IStreamStore streamStore, ILogger<LegacyStreamEventsWriter> logger)
    {
        _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task WriteAsync(IEnumerable<StreamEvent> events)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));
        var expectedVersions = new ConcurrentDictionary<StreamId, int>();

        foreach (var batch in events.Batch(1000))
        foreach (var stream in batch.GroupBy(item => item.Stream, item => item.Event))
        {
            if (!expectedVersions.TryGetValue(stream.Key, out var expectedVersion)) expectedVersion = ExpectedVersion.NoStream;

            var watch = Stopwatch.StartNew();

            var appendResult = await _streamStore.AppendToStream(
                stream.Key,
                expectedVersion,
                stream
                    .Select(@event => new NewStreamMessage(
                        Deterministic.Create(Deterministic.Namespaces.Events, $"{stream.Key}-{expectedVersion++}"),
                        Mapping.GetEventName(@event.GetType()),
                        JsonConvert.SerializeObject(@event, SerializerSettings),
                        JsonConvert.SerializeObject(new Dictionary<string, string> { { "$version", "0" } }, SerializerSettings)
                    ))
                    .ToArray()
            );

            _logger.LogInformation("Append took {0}ms for stream {1}@{2}",
                watch.ElapsedMilliseconds,
                stream.Key,
                appendResult.CurrentVersion);

            expectedVersions[stream.Key] = appendResult.CurrentVersion;
        }
    }
}